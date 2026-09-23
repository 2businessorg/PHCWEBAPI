# Presidio sidecar (presidio-2b) — PHCWEBAPI pseudonymization v1

Stateless Analyzer/Anonymizer for the shared `IDocumentPseudonymizer` .NET library.
**TokenMap encryption and storage stay in .NET on-prem.** This sidecar must never persist originals.

## Endpoints

| Method | Path | Purpose |
|--------|------|---------|
| GET | `/health` | Liveness |
| POST | `/analyze` | `{ "text", "language" }` → entities |
| POST | `/anonymize` | Optional redaction helper (map still owned by C#) |

## Lab vs prod

`RequireSidecarForCloudEgress` and the independent leak checker stay on. This sidecar does not disable either of them.

| | Lab host | Prod image |
|---|---|---|
| spaCy models | Optional. If `pt_core_news_sm` / `en_core_web_sm` are missing, `/health` reports `nlp=pattern-only` and the process still starts. | `Dockerfile` downloads `en_core_web_sm` and `pt_core_news_sm` at **image build**. No model download on the first request. |
| EMAIL, PHONE (PT `+351` and Mozambique `+258` 82–87), NIF | Regex recognizers registered for **both** `en` and `pt`. `/analyze` also merges `contact_patterns.find_contacts`, so a language fallback to `en` cannot drop them. | Same patterns, plus PERSON/LOCATION from the installed spaCy models. |
| Blank spaCy | Not used as the detector. A blank model does not see contacts and is not a startup shortcut. | Not used. |

Install models once, at setup or image build, when you want NER beyond the contact patterns:

```bash
python -m spacy download en_core_web_sm
python -m spacy download pt_core_news_sm
```

## Run locally

```bash
cd sidecar/presidio-2b
python -m venv .venv
# Windows: .venv\Scripts\activate
pip install -r requirements.txt
python -m unittest test_contact_patterns.py
# optional NER: python -m spacy download en_core_web_sm && python -m spacy download pt_core_news_sm
uvicorn app:app --host 127.0.0.1 --port 5001
```

## Docker

```bash
docker build -t presidio-2b ./sidecar/presidio-2b
docker run --rm -p 5001:5001 presidio-2b
```

## Config (C# host)

```json
{
  "Pseudonymization": {
    "EnableCloudLlm": false,
    "PresidioBaseUrl": "http://127.0.0.1:5001",
    "TokenMapEncryptionKeyBase64": ""
  }
}
```

Keys and connection strings belong in secret stores — never commit real secrets.

## v1 vs later

- **v1:** Presidio + regex/dict/context (also mirrored in C# for leak checker independence)
- **v2 stub:** GLiNER/spaCy ensemble (not wired)
- **v3 stub:** quasi-ID generalization (not wired)

## Ownership

Shared Platform owns the C# contract; this sidecar is an optional analyzer backend.
Recruitment consumes via `EnableCloudLlm` gate (BR-08). OCR `u_texto` stays on-prem without this gate.
