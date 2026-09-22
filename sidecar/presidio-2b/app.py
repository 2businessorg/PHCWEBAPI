"""
Presidio sidecar for 2Business PHCWEBAPI pseudonymization v1.
Stateless regarding TokenMap — map lives in .NET on-prem.
Endpoints: GET /health, POST /analyze, POST /anonymize
"""
from __future__ import annotations

import re
from pathlib import Path
from typing import Any

from fastapi import FastAPI
from pydantic import BaseModel, Field
from presidio_analyzer import AnalyzerEngine, Pattern, PatternRecognizer, RecognizerRegistry
from presidio_analyzer.nlp_engine import NlpEngineProvider
from presidio_anonymizer import AnonymizerEngine
from presidio_anonymizer.entities import OperatorConfig

APP_DIR = Path(__file__).resolve().parent
DICTS = APP_DIR / "dicts"

app = FastAPI(title="presidio-2b", version="1.0.0")


def _load_dict(name: str) -> list[str]:
    path = DICTS / name
    if not path.exists():
        return []
    return [line.strip() for line in path.read_text(encoding="utf-8").splitlines() if line.strip() and not line.startswith("#")]


def build_registry() -> RecognizerRegistry:
    registry = RecognizerRegistry()
    registry.load_predefined_recognizers()

    nif = PatternRecognizer(
        supported_entity="NIF",
        patterns=[Pattern("nif_pt", r"\b[123568]\d{8}\b", 0.6)],
        name="NifPtRecognizer",
        supported_language="pt",
    )
    niss = PatternRecognizer(
        supported_entity="NISS",
        patterns=[Pattern("niss_pt", r"\b\d{11}\b", 0.4)],
        name="NissPtRecognizer",
        supported_language="pt",
    )
    phone = PatternRecognizer(
        supported_entity="PHONE_NUMBER",
        patterns=[Pattern("phone_pt", r"(?:\+351\s?)?(?:9\d{2}[\s\-]?\d{3}[\s\-]?\d{3})", 0.6)],
        name="PhonePtRecognizer",
        supported_language="pt",
    )
    registry.add_recognizer(nif)
    registry.add_recognizer(niss)
    registry.add_recognizer(phone)

    # Dictionary recognizers — placeholder terms only (no real client data).
    for entity, filename in (
        ("EMPLOYER", "employers_pt.txt"),
        ("INTERNAL_SYSTEM", "internal_systems.txt"),
    ):
        terms = _load_dict(filename)
        if not terms:
            continue
        # Simple OR pattern; keep short for v1.
        escaped = "|".join(re.escape(t) for t in terms)
        registry.add_recognizer(
            PatternRecognizer(
                supported_entity=entity,
                patterns=[Pattern(f"dict_{entity.lower()}", rf"\b(?:{escaped})\b", 0.7)],
                name=f"Dict{entity}Recognizer",
                supported_language="pt",
            )
        )

    return registry


def build_analyzer() -> AnalyzerEngine:
    # Prefer small spaCy model when present; fall back to built-in.
    try:
        provider = NlpEngineProvider(
            nlp_configuration={
                "nlp_engine_name": "spacy",
                "models": [{"lang_code": "pt", "model_name": "pt_core_news_sm"}],
            }
        )
        nlp_engine = provider.create_engine()
        return AnalyzerEngine(registry=build_registry(), nlp_engine=nlp_engine, supported_languages=["pt", "en"])
    except Exception:
        return AnalyzerEngine(registry=build_registry(), supported_languages=["en", "pt"])


analyzer = build_analyzer()
anonymizer = AnonymizerEngine()


class AnalyzeIn(BaseModel):
    text: str
    language: str = "pt"


class AnonymizeIn(BaseModel):
    text: str
    language: str = "pt"
    # Optional caller-supplied stable placeholders; map canonical store remains in .NET
    operators: dict[str, str] = Field(default_factory=dict)


@app.get("/health")
def health() -> dict[str, str]:
    return {"status": "ok", "service": "presidio-2b", "version": "1"}


@app.post("/analyze")
def analyze(payload: AnalyzeIn) -> dict[str, Any]:
    language = payload.language or "pt"
    try:
        results = analyzer.analyze(text=payload.text, language=language)
    except Exception:
        # Fallback to English NLP if pt model missing
        results = analyzer.analyze(text=payload.text, language="en")

    entities = []
    for r in results:
        entities.append(
            {
                "entity_type": r.entity_type,
                "start": r.start,
                "end": r.end,
                "score": float(r.score),
                "recognizer_name": getattr(r, "recognition_metadata", {}) and r.recognition_metadata.get("recognizer_name")
                or getattr(r, "recognizer_name", "presidio"),
            }
        )
    return {"entities": entities}


@app.post("/anonymize")
def anonymize(payload: AnonymizeIn) -> dict[str, Any]:
    """
    Optional helper. Canonical TokenMap is owned by .NET.
    Default operator replaces with <ENTITY_TYPE> placeholders.
    """
    language = payload.language or "pt"
    try:
        results = analyzer.analyze(text=payload.text, language=language)
    except Exception:
        results = analyzer.analyze(text=payload.text, language="en")

    operators = {
        "DEFAULT": OperatorConfig("replace", {"new_value": "<REDACTED>"}),
    }
    for entity_type, replacement in payload.operators.items():
        operators[entity_type] = OperatorConfig("replace", {"new_value": replacement})

    anonymized = anonymizer.anonymize(text=payload.text, analyzer_results=results, operators=operators)
    return {"text": anonymized.text, "items": [i.to_dict() for i in anonymized.items]}
