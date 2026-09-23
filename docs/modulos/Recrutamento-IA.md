# Recrutamento PHC x IA (v1)

Modulo `src/Modules/Pessoal/Recruitment` — score por rubrica + evidencia citada, HITL no PHC.

## AH-01…AH-08 enforced in code paths

| AH | Enforcement |
|----|-------------|
| AH-01 | `RubricEvidenceScorer` only scores `IRctCriteriaRepository` rows for the RCT |
| AH-02 | note&gt;0 requires quote ≤240 chars; `AssertQuoteIsSubset(u_texto)` |
| AH-03 | no evidence → note=0 + label `sem evidencia no CV` |
| AH-04 | `ForbiddenCopyGuard` + HITL title/footer; no auto-select endpoints (BR-09: never write `selection` / `condp`) |
| AH-05 | year conflicts → `conflito` flag; no silent full-weight average |
| AH-06 | justifications in pt-PT; no invented facts outside `u_texto` |
| AH-07 | `srt.u_modeloia` + `u_prmveria` + `u_stampia` always written |
| AH-08 | OCR fail → `cve.u_estadoia=erro`, `ClearScoreOnOcrError`, no `SaveScore` |

## BR highlights

- **BR-01**: enqueue only if crt≥1 **and** RCTCLB≥1 (lab GO exception via `AllowLabEnqueueWithoutRctclb` + `LabGoRef`)
- **BR-02**: score on `srt.u_scoreia` / `u_justia` — never canonical CVE multi-RCT score
- **BR-03**: job never writes selection/apurado/entrevista (asserted post-job)
- **BR-04**: `srt.condp` read-only for IA
- **BR-09**: ranking and analyze/reprocess never write `selection` or `condp` — human decision only (pre-seleccao = ordered list for RH)
- **BR-05**: `IPhcAvisoService` / `PhcAvisoServiceStub` — hook `XcUtil.criaAvs` (no parallel channel)
- **BR-07/08**: OCR fail path; `EnableCloudLlm` defaults **false**
- **BR-10**: hire→employee **out of scope**
- **BR-12**: queue host = **this Hangfire host** (`PHCAPI.Host`, schema `PHCHANGFIRE`)

## Columns chosen (hypotheses → documented)

| Purpose | Column |
|---------|--------|
| estado_ia | `cve.u_estadoia` |
| OCR text | `anexos.u_texto` |
| Score | `srt.u_scoreia` (avoids native `score` collision) |
| Justification JSON | `srt.u_justia` |
| Soft-repro | `srt.u_modeloia`, `srt.u_prmveria`, `srt.u_stampia` |
| Audit reprocess | `srt.u_auditia` |
| Vacancy id | `rct.idrct` (resolved to `rctstamp`; not stored as a new column) |
| Outbox | `u_rec_ia_outbox` |
| Criteria+weights bridge | `u_rec_ia_crt` (prod must mirror RCT native weights; lab seed in SQL is demo-only) |

Script: `scripts/Recruitment_AddIaColumns.sql`

## Hangfire

- Job type: `Recruitment.Application.Jobs.AnalyzeCandidateJob`
- Name: `Recruitment.AnalyzeCandidate`
- Product enqueue: `POST /api/recruitment/{idrct}/analyze` → resolve `idrct`→`rctstamp` → one outbox row + one `Recruitment.AnalyzeCandidate` job per SRT that has a CV.
- Legacy flat enqueue `POST /api/recruitment/enqueue` (three stamps in the body) still responds, with `Deprecation: true`. It is not the product path.

Analysis runs only from these explicit endpoints. Saving a candidature does not enqueue.

### How to analyze a vacancy

```http
POST /api/recruitment/{idrct}/analyze
{
  "requestedBy": "denilson"
}
```

`requestedBy` and `labGoRef` are optional on analyze. The handler selects every SRT of that vacancy with a CV in `anexos` (`oritable='cve'`, `recstamp=cvestamp`, `tipo=1`, non-empty `bdados`) and calls `TryEnqueueAsync` + Hangfire for each. BR-01 (`crt≥1` and `RCTCLB≥1`) still refuses inside `TryEnqueueAsync`.

```http
GET /api/recruitment/{idrct}/status
```

Counts `pendente` / `ok` / `erro` from `cve.u_estadoia` across the vacancy's SRT rows (`semEstado` covers null or unknown).

```http
POST /api/recruitment/{idrct}/reprocess
{
  "srtStamp": "...",
  "requestedBy": "denilson"
}
```

Reprocesses one candidature. `requestedBy` is required (BR-06). The SRT must belong to the resolved vacancy.

### Legacy enqueue (not the product path)

```http
POST /api/recruitment/enqueue
{
  "cveStamp": "...",
  "rctStamp": "...",
  "srtStamp": "..."
}
```

## HITL API

| Method | Route | Role |
|--------|-------|------|
| POST | `/api/recruitment/{idrct}/analyze` | Batch enqueue (product) |
| GET | `/api/recruitment/{idrct}/status` | pending/ok/erro counts |
| GET | `/api/recruitment/{idrct}/ranking` | Score order for human pre-selection |
| POST | `/api/recruitment/{idrct}/reprocess` | One candidature |
| GET | `/api/recruitment/rct/{rctStamp}/ranking` | Same ranking by stamp |
| GET | `/api/recruitment/rct/{rctStamp}/compare?firstSrtStamp=&otherSrtStamp=` | Quote diff |
| POST | `/api/recruitment/enqueue` | Legacy, deprecated |
| POST | `/api/recruitment/reprocess` | Legacy, deprecated |
| GET | `/api/recruitment/hitl-copy` | Allowed copy |

Title/footer: score is **input**; human decides in PHC. Forbidden: «seleccionado/rejeitado/avancado pela IA».

## Orphan runbook (BR-12)

1. Host: `PHCAPI.Host` Hangfire server (`/hangfire`).
2. Detect: `IRecruitmentOutboxRepository.GetOrphansAsync(OrphanTtlMinutes)` where estado in pending/processing and heartbeat stale.
3. Action: restart Hangfire host; re-enqueue via `POST /api/recruitment/{idrct}/reprocess` with `srtStamp` + `requestedBy` (BR-06).
4. Kill-case: leave `estado_ia=pendente` forever without this procedure.

## Config

`appsettings.json` → `RecruitmentIa` (`EnableCloudLlm: false` by default).

## Tests

`tests/Recruitment.Application.Tests` — no citation→0; OCR fail→no score; SRT write shape; selection unchanged; batch analyze by `idrct` (one Hangfire job per CV); ranking by `idrct` does not write `selection`/`condp`.

## DTTest static pre-check (Alfredo)

See `docs/modulos/DTTest-Alfredo-Recrutamento-IA-static-precheck-2026-09-22.md` — main@`14204ad`, Pass/Fail with file:line (AH-02/04/08 + enqueue + side-effects), route map for Imran smoke on Denilson host. No Alfredo live curl (Host is laptop-local only).
