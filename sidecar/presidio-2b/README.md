# Presidio sidecar (presidio-2b) — PHCWEBAPI pseudonymization v1

Stateless Analyzer/Anonymizer for the shared `IDocumentPseudonymizer` .NET library.
**TokenMap encryption and storage stay in .NET on-prem.** This sidecar must never persist originals.

## Endpoints

| Method | Path | Purpose |
|--------|------|---------|
| GET | `/health` | Liveness |
| POST | `/analyze` | `{ "text", "language" }` → entities |
| POST | `/anonymize` | Optional redaction helper (map still owned by C#) |

## Run locally

```bash
cd sidecar/presidio-2b
python -m venv .venv
# Windows: .venv\Scripts\activate
pip install -r requirements.txt
# optional: python -m spacy download pt_core_news_sm
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
