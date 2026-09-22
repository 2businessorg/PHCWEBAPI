# Recrutamento PHC x IA (v1)

Modulo `src/Modules/Pessoal/Recruitment` — score por rubrica + evidencia citada, HITL no PHC.

## AH-01…AH-08 enforced in code paths

| AH | Enforcement |
|----|-------------|
| AH-01 | `RubricEvidenceScorer` only scores `IRctCriteriaRepository` rows for the RCT |
| AH-02 | note&gt;0 requires quote ≤240 chars; `AssertQuoteIsSubset(u_texto)` |
| AH-03 | no evidence → note=0 + label `sem evidencia no CV` |
| AH-04 | `ForbiddenCopyGuard` + HITL title/footer; no auto-select endpoints |
| AH-05 | year conflicts → `conflito` flag; no silent full-weight average |
| AH-06 | justifications in pt-PT; no invented facts outside `u_texto` |
| AH-07 | `srt.u_modeloia` + `u_promptveria` + `u_stampia` always written |
| AH-08 | OCR fail → `cve.u_estadoia=erro`, `ClearScoreOnOcrError`, no `SaveScore` |

## BR highlights

- **BR-01**: enqueue only if crt≥1 **and** RCTCLB≥1 (lab GO exception via `AllowLabEnqueueWithoutRctclb` + `LabGoRef`)
- **BR-02**: score on `srt.u_scoreia` / `u_justia` — never canonical CVE multi-RCT score
- **BR-03**: job never writes selection/apurado/entrevista (asserted post-job)
- **BR-04**: `srt.condp` read-only for IA
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
| Soft-repro | `srt.u_modeloia`, `srt.u_promptveria`, `srt.u_stampia` |
| Audit reprocess | `srt.u_auditoriaia` |
| Outbox | `u_rec_ia_outbox` |
| Criteria+weights bridge | `u_rec_ia_crt` (prod must mirror RCT native weights; lab seed in SQL is demo-only) |

Script: `scripts/Recruitment_AddIaColumns.sql`

## Hangfire

- Job type: `Recruitment.Application.Jobs.AnalyzeCandidateJob`
- Name: `Recruitment.AnalyzeCandidate`
- Enqueue: `POST /api/recruitment/enqueue` → outbox row → `IBackgroundJobClient.Enqueue`

### How to enqueue

```http
POST /api/recruitment/enqueue
{
  "cveStamp": "...",
  "rctStamp": "...",
  "srtStamp": "..."
}
```

Requires CV in `anexos` (`oritable='cve'`, `recstamp=cvestamp`, `tipo=1`, bytes in `bdados`).

## HITL API

| Method | Route |
|--------|-------|
| GET | `/api/recruitment/rct/{rctStamp}/ranking` |
| GET | `/api/recruitment/rct/{rctStamp}/compare?firstSrtStamp=&otherSrtStamp=` |
| POST | `/api/recruitment/reprocess` |
| GET | `/api/recruitment/hitl-copy` |

Title/footer: score is **input**; human decides in PHC. Forbidden: «seleccionado/rejeitado/avancado pela IA».

## Orphan runbook (BR-12)

1. Host: `PHCAPI.Host` Hangfire server (`/hangfire`).
2. Detect: `IRecruitmentOutboxRepository.GetOrphansAsync(OrphanTtlMinutes)` where estado in pending/processing and heartbeat stale.
3. Action: restart Hangfire host; re-enqueue via `/api/recruitment/reprocess` with `requestedBy` (BR-06).
4. Kill-case: leave `estado_ia=pendente` forever without this procedure.

## Config

`appsettings.json` → `RecruitmentIa` (`EnableCloudLlm: false` by default).

## Tests

`tests/Recruitment.Application.Tests` — no citation→0; OCR fail→no score; SRT write shape; selection unchanged; Alfredo fixture notes.

## DTTest (Alfredo) static pre-check

See `docs/modulos/DTTest-Alfredo-Recrutamento-IA-static-precheck-2026-09-22.md` — SHA `14204ad`, endpoint map, AH-02/04/08 static Pass/Fail, curl runbook, gaps vs Pass Dinis.
