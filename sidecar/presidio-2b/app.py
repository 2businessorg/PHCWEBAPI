"""
Presidio sidecar for 2Business PHCWEBAPI pseudonymization v1.
Stateless regarding TokenMap — map lives in .NET on-prem.
Endpoints: GET /health, POST /analyze, POST /anonymize

Contact recognizers (EMAIL, PHONE PT, PHONE MZ +258, NIF) are registered for
both en and pt. /analyze also merges contact_patterns.find_contacts so a
missing spaCy model cannot drop them. Models are loaded only when already
installed (image build / setup). This process never downloads models.
"""
from __future__ import annotations

import re
from pathlib import Path
from typing import Any

from fastapi import FastAPI
from pydantic import BaseModel, Field
from presidio_analyzer import AnalyzerEngine, Pattern, PatternRecognizer, RecognizerRegistry, RecognizerResult
from presidio_analyzer.nlp_engine import NlpEngineProvider
from presidio_anonymizer import AnonymizerEngine
from presidio_anonymizer.entities import OperatorConfig

from contact_patterns import CONTACT_PATTERNS, find_contacts

APP_DIR = Path(__file__).resolve().parent
DICTS = APP_DIR / "dicts"
ANALYZER_LANGUAGES = ("en", "pt")
MODEL_CANDIDATES = (
    ("en", "en_core_web_sm"),
    ("pt", "pt_core_news_sm"),
)

app = FastAPI(title="presidio-2b", version="1.1.0")


def _load_dict(name: str) -> list[str]:
    path = DICTS / name
    if not path.exists():
        return []
    return [
        line.strip()
        for line in path.read_text(encoding="utf-8").splitlines()
        if line.strip() and not line.startswith("#")
    ]


def _add_pattern(
    registry: RecognizerRegistry,
    entity: str,
    name: str,
    pattern: str,
    score: float,
) -> None:
    for language in ANALYZER_LANGUAGES:
        registry.add_recognizer(
            PatternRecognizer(
                supported_entity=entity,
                patterns=[Pattern(name, pattern, score)],
                name=f"{name}_{language}",
                supported_language=language,
            )
        )


def build_registry() -> RecognizerRegistry:
    registry = RecognizerRegistry()
    registry.load_predefined_recognizers()

    for entity, name, pattern, score in CONTACT_PATTERNS:
        _add_pattern(registry, entity, name, pattern, score)

    _add_pattern(registry, "NISS", "niss_pt", r"\b\d{11}\b", 0.4)

    for entity, filename in (
        ("EMPLOYER", "employers_pt.txt"),
        ("INTERNAL_SYSTEM", "internal_systems.txt"),
    ):
        terms = _load_dict(filename)
        if not terms:
            continue
        escaped = "|".join(re.escape(t) for t in terms)
        _add_pattern(registry, entity, f"dict_{entity.lower()}", rf"\b(?:{escaped})\b", 0.7)

    return registry


def _installed_models() -> list[dict[str, str]]:
    try:
        from spacy.util import is_package
    except Exception:
        return []

    installed: list[dict[str, str]] = []
    for lang_code, model_name in MODEL_CANDIDATES:
        try:
            if is_package(model_name):
                installed.append({"lang_code": lang_code, "model_name": model_name})
        except Exception:
            continue
    return installed


def build_analyzer() -> tuple[AnalyzerEngine | None, list[str]]:
    """
    Load spaCy models that are already installed. Never fall back to a blank
    model as the only detector and never download weights at request time.
    None means pattern-only: find_contacts still redacts EMAIL/PHONE/NIF.
    """
    registry = build_registry()
    models = _installed_models()
    if not models:
        return None, []

    languages = [model["lang_code"] for model in models]
    try:
        provider = NlpEngineProvider(
            nlp_configuration={
                "nlp_engine_name": "spacy",
                "models": models,
            }
        )
        nlp_engine = provider.create_engine()
        engine = AnalyzerEngine(
            registry=registry,
            nlp_engine=nlp_engine,
            supported_languages=languages,
        )
        return engine, languages
    except Exception:
        return None, []


analyzer, analyzer_languages = build_analyzer()
anonymizer = AnonymizerEngine()


class AnalyzeIn(BaseModel):
    text: str
    language: str = "pt"


class AnonymizeIn(BaseModel):
    text: str
    language: str = "pt"
    operators: dict[str, str] = Field(default_factory=dict)


def _presidio_entities(text: str, language: str) -> list[dict[str, Any]]:
    if analyzer is None:
        return []

    languages_to_try = [language or "pt"]
    if "en" not in languages_to_try and (not analyzer_languages or "en" in analyzer_languages):
        languages_to_try.append("en")

    last_error: Exception | None = None
    for lang in languages_to_try:
        try:
            results = analyzer.analyze(text=text, language=lang)
        except Exception as exc:
            last_error = exc
            continue

        entities: list[dict[str, Any]] = []
        for result in results:
            metadata = getattr(result, "recognition_metadata", None) or {}
            recognizer = metadata.get("recognizer_name") if isinstance(metadata, dict) else None
            entities.append(
                {
                    "entity_type": result.entity_type,
                    "start": result.start,
                    "end": result.end,
                    "score": float(result.score),
                    "recognizer_name": recognizer or getattr(result, "recognizer_name", "presidio"),
                }
            )
        return entities

    if last_error is not None:
        return []
    return []


def collect_entities(text: str, language: str) -> list[dict[str, Any]]:
    entities = _presidio_entities(text, language)
    covered = {(item["start"], item["end"], item["entity_type"]) for item in entities}
    for hit in find_contacts(text):
        key = (hit["start"], hit["end"], hit["entity_type"])
        if key in covered:
            continue
        entities.append(hit)
        covered.add(key)
    return entities


@app.get("/health")
def health() -> dict[str, str]:
    return {
        "status": "ok",
        "service": "presidio-2b",
        "version": "1",
        "nlp": ",".join(analyzer_languages) if analyzer_languages else "pattern-only",
    }


@app.post("/analyze")
def analyze(payload: AnalyzeIn) -> dict[str, Any]:
    return {"entities": collect_entities(payload.text, payload.language or "pt")}


@app.post("/anonymize")
def anonymize(payload: AnonymizeIn) -> dict[str, Any]:
    """
    Optional helper. Canonical TokenMap is owned by .NET.
    Default operator replaces with <ENTITY_TYPE> placeholders.
    """
    language = payload.language or "pt"
    entities = collect_entities(payload.text, language)
    results = [
        RecognizerResult(
            entity_type=item["entity_type"],
            start=item["start"],
            end=item["end"],
            score=float(item["score"]),
        )
        for item in entities
    ]

    operators = {
        "DEFAULT": OperatorConfig("replace", {"new_value": "<REDACTED>"}),
    }
    for entity_type, replacement in payload.operators.items():
        operators[entity_type] = OperatorConfig("replace", {"new_value": replacement})

    anonymized = anonymizer.anonymize(text=payload.text, analyzer_results=results, operators=operators)
    return {"text": anonymized.text, "items": [item.to_dict() for item in anonymized.items]}
