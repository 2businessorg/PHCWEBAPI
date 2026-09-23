# Pseudonymization v1 (Presidio)

Reversible **pseudonymization** (not irreversible anonymization) before any cloud LLM egress.
Residual re-identification risk is explicit and non-zero. OCR `u_texto` stays on-prem (BR-08).

## Ownership

| Piece | Location | Owner |
|-------|----------|-------|
| Contracts (`IDocumentPseudonymizer`, DTOs, entity catalog, skills allowlist) | `src/Shared/Shared.Abstractions/Privacy/Pseudonymization` | Shared / Platform |
| Pipeline, TokenMap, leak checker, safe detoken, Presidio HTTP client, DI | `src/Shared/Shared.Infrastructure/Privacy/Pseudonymization` | Shared / Platform |
| SQL schema (encrypted fields, TTL, access-log stub) | `scripts/sql/privacy/token_map_v1.sql` | Shared + Ops |
| Presidio sidecar | `sidecar/presidio-2b` | Platform (stateless; no TokenMap) |
| First consumer (thin adapter behind `EnableCloudLlm`) | `Recruitment.Application/Privacy` | Pessoal-RH / Recrutamento |

Anti one-off: do **not** copy Presidio regex into Recruitment workers — consume `IDocumentPseudonymizer`.

## Token format

ASCII grammar `{{ENTITY_TYPE_hex}}` (Windows-1252 safe). Example: `{{EMAIL_ADDRESS_a1b2c3d4}}`.

## Feature flags

| Flag | Default | Meaning |
|------|---------|---------|
| `RecruitmentIa:EnableCloudLlm` | property **false**; product `appsettings.json` **true** | Product gate for Qwen. Missing config stays on the offline rubric. |
| `Pseudonymization:RequireSidecarForCloudEgress` | true | CloudEgress fail-closed if sidecar down |
| `Pseudonymization:EnableHybridNerV2` | false | v2 stub (GLiNER/spaCy) |
| `Pseudonymization:EnableQuasiIdGeneralization` | false | v3 stub |

When `EnableCloudLlm=false`, Hangfire/OCR/scoring stay on-prem and **no sidecar is required**.

## Host registration

```csharp
builder.Services.AddPseudonymization(builder.Configuration);
```

Config placeholders: `appsettings.json` section `Pseudonymization` and `appsettings.Pseudonymization.example.json`.
**Never commit real keys or connection strings.**

## Pipeline (v1)

Normalize → Detect (Presidio + regex/dict/context) → Classify → Resolve → Tokenize + encrypted TokenMap → independent LeakCheck (fail-closed for CloudEgress) → Qwen via `ILocalChatModel` on the pseudonymized text only (`QwenCloudScoreEngine`) → Safe detoken (parser + session whitelist + type check).

## Sidecar

See `sidecar/presidio-2b/README.md`. Endpoints: `/health`, `/analyze`, `/anonymize`.

EMAIL, PHONE (PT and Mozambique `+258`) and NIF recognizers are registered for both `en` and `pt`, and `/analyze` merges the same patterns if the spaCy language falls back. `en_core_web_sm` / `pt_core_news_sm` are downloaded at image build, not on the first request. A missing model keeps the sidecar in pattern-only mode. Do not turn off `RequireSidecarForCloudEgress` or the leak checker. The C# detector and `IndependentLeakChecker` share those phone patterns, including `+258`.

## DoD checklist (architecture section 25) — what landed

- [x] `IDocumentPseudonymizer` in shared lib (not only Recruitment folder)
- [x] Sidecar Presidio with custom recognizers + versioned placeholder dicts
- [x] TokenMap schema + AES-GCM encryption + TTL + access-log stub (InMemory store default; SQL script provided)
- [x] Leak checker independent fail-closed (CloudEgress)
- [x] Detoken parser + session scope + reject unknown (StrictDetoken)
- [x] Allowlist professional skills (do not tokenize)
- [x] `EnableCloudLlm` property default false (offline if unset); product host sets true and fails closed without Presidio/Qwen (BR-08)
- [ ] Metrics precision/recall/leakage in lab (deferred — lab harness out of scope for this slice)
- [x] Documentation: pseudonymization, no 100% guarantee
- [x] No real secrets in repo (Development appsettings ConnectionStrings untouched)

## Out of scope (this PR)

SOAP lab, prod sidecar deploy, rubric changes, scrubbing `appsettings.Development.json`, v2 NER, v3 quasi-ID beyond stubs.
