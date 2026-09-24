"""
EMAIL, PHONE (PT and Mozambique +258) and NIF patterns.

These run as Presidio recognizers for every language the analyzer actually
loads (en and pt) and again in analyze() so a missing spaCy model cannot
drop them. Stdlib only — unit-tested without Presidio.
"""
from __future__ import annotations

import re

EMAIL_PATTERN = r"\b[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}\b"
PHONE_PT_PATTERN = r"(?:\+351[\s\-]?)?9\d{2}(?:[\s\-]?\d{3}){2}"
PHONE_MZ_PATTERN = r"(?:\+258[\s\-]?)?8[2-7](?:[\s\-]?\d){7}"
NIF_PATTERN = r"\b[123568]\d{8}\b"

# (entity_type, recognizer_name, pattern, score)
CONTACT_PATTERNS: tuple[tuple[str, str, str, float], ...] = (
    ("EMAIL_ADDRESS", "email", EMAIL_PATTERN, 0.85),
    ("PHONE_NUMBER", "phone_pt", PHONE_PT_PATTERN, 0.6),
    ("PHONE_NUMBER", "phone_mz", PHONE_MZ_PATTERN, 0.6),
    ("NIF", "nif_pt", NIF_PATTERN, 0.6),
)

_COMPILED = tuple((entity, name, re.compile(pattern), score) for entity, name, pattern, score in CONTACT_PATTERNS)


def find_contacts(text: str) -> list[dict]:
    hits: list[dict] = []
    if not text:
        return hits
    for entity_type, name, pattern, score in _COMPILED:
        for match in pattern.finditer(text):
            hits.append(
                {
                    "entity_type": entity_type,
                    "start": match.start(),
                    "end": match.end(),
                    "score": score,
                    "recognizer_name": name,
                }
            )
    return hits
