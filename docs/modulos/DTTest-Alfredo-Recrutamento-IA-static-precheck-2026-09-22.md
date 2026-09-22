# Static code review — Recrutamento × IA — main@14204ad

**Date:** 2026-09-22  
**Scope:** STATIC only (Alfredo cannot reach Host — Denilson laptop `http://127.0.0.1:7298` only).  
**SHA:** `14204ad2cbe3fbf6f3434b38ea13386b5c7790f3` = `main` = `origin/main`  
**Product code:** unchanged. Live curl: **out of scope for Alfredo** (Imran smoke on Denilson host).

Paths relative to repo root. Line numbers from tree at `14204ad` / this docs branch (module files unchanged).

---

## Pass / Fail table (hard DoD + enqueue + side-effects)

| ID | Criterion | Verdict | Evidence (file:line) |
|----|-----------|---------|----------------------|
| **AH-02** | `note>0` requires quote ⊆ `u_texto`; invent quote → fail | **PASS** | Quote asserted at score time: `Recruitment.Application/Scoring/RubricEvidenceScorer.cs:68`, kill helper `:189-200` throws `AH-02: citacao nao pertence a u_texto`. Job re-checks every `note>0` before persist: `Recruitment.Application/Jobs/AnalyzeCandidateJob.cs:158-163`. Empty/no match → note=0, no quote: `RubricEvidenceScorer.cs:56-63`, `:101-112`. Unit invent→throw: `tests/.../RubricEvidenceScorerTests.cs:108-114`. Unit no-skill→0: `:39-50`. |
| **AH-04** | Zero product copy «seleccionado / rejeitado / avançado pela IA» | **PASS*** | Ban list: `Recruitment.Domain/Constants/IaEstados.cs:38-45`. Guard: `Scoring/ForbiddenCopyGuard.cs:10-28`. Applied: scorer `:82`; job justifications `:162`, JSON `:188`; ranking title/footer `Features/GetRctRanking/GetRctRankingQuery.cs:63-64`; compare title `Features/CompareCandidates/CompareCandidatesQuery.cs:47`; aviso stub `Infrastructure/Notifications/PhcAvisoServiceStub.cs:44-49`. HITL allowed copy: `IaEstados.cs:29-33`. No auto-select routes: controller comment `Presentation/.../RecruitmentIaController.cs:16-18`. Unit: `RubricEvidenceScorerTests.cs:118-134`. |
| **AH-08** | OCR fail / empty text → `estado_ia=erro`, **no** score | **PASS** | Gate: `AnalyzeCandidateJob.cs:116-124` — `ClearScoreOnOcrErrorAsync` `:119`, `SetEstadoIa(Erro)` `:120`, `MarkError` `:121`, **return before** `Score`/`SaveScore`. Clear SQL nulls IA cols only: `Infrastructure/Repositories/SrtAndOutboxRepositories.cs:71-88`. `SaveScore` never touches selection/condp: `:46-56`. Unit: `AnalyzeAndEnqueueTests.cs:20-86` (`ClearScore` Once, `SaveScore` Never, scorer Never). |
| **E1** | Enqueue without crt / without RCTCLB → refuse; no score job | **PASS** | Counts: `Services/RecruitmentEnqueueService.cs:58-59`. Refuse crt&lt;1: `:65-75` (`Enqueued=false`, return **before** outbox). Refuse clb&lt;1 (unless lab GO): `:78-88`. Outbox+`pendente` only after pass: `:103-126`. Lab exception gated: `:62-63`, options default off `Options/RecruitmentIaOptions.cs:22-23` + `Host/appsettings.json` `AllowLabEnqueueWithoutRctclb: false`. Units: `AnalyzeAndEnqueueTests.cs:208-241` (Strict outbox → no insert). |
| **E2 / side-effect** | Fail/cancel mid → recoverable; no ghost score; selection/`condp` unchanged | **PASS (fail path) / PARTIAL (cancel)** | Snapshot before work: `AnalyzeCandidateJob.cs:81-83`. Post OCR-fail + post-success assert: `:123`, `:212`, `:237-251` (throws if selection or `condp` changed). Fail helper sets `erro` + outbox error: `:215-219` (**does not** always ClearScore — only OCR path clears). `SaveScore` UPDATE list excludes selection/condp: `SrtAndOutboxRepositories.cs:46-56`. Selection columns configured read-only snapshot: `Options/RecruitmentSchemaOptions.cs:44`. **Gap:** no HTTP cancel; Hangfire abort only. Fail-after-prior-score without OCR clear leaves previous `u_scoreia` until reprocess/OCR-clear. |

\*AH-04 residuals (do not flip static PASS of ban enforcement):

1. `RecruitmentIaController.cs:105` — `hitl-copy.note` **documents** forbidden phrases (meta, not decision copy).  
2. `ForbiddenPhrases` uses ASCII `avancado` / `selecionado`; Unicode `avançado` (ç) not listed (`IaEstados.cs:40-44`).  
3. No PHC UI/email/export strings in this repo to review.

---

## `/api/recruitment` route map (Imran live smoke on Denilson host)

Controller: `Recruitment.Presentation/REST/Controllers/RecruitmentIaController.cs`  
Prefix: `:21` `[Route("api/recruitment")]` · Auth: `AppPolicies.ApiAccess` · Module pack: `Gestão` (`Host/appsettings.json` RouteModules).

| # | Method | Route | Req | Res | Smoke assert |
|---|--------|-------|-----|-----|--------------|
| 1 | POST | `/api/recruitment/enqueue` | body `{ cveStamp, rctStamp, srtStamp, labGoRef? }` `:109-115` | `EnqueueAnalysisResultDto` `{ enqueued, outboxId?, message, criteriaCount, intervenienteCount }` | E1: no crt/RCTCLB → `enqueued=false`, no Hangfire job. Happy: `enqueued=true` → job `Recruitment.AnalyzeCandidate` |
| 2 | GET | `/api/recruitment/rct/{rctStamp}/ranking` | path | `RctRankingDto` + `RankedCandidateDto[]` (score, estadoIa, breakdown.quote) | AH-08 row: `estadoIa=erro`, `scoreTotal=null`. AH-02: quotes ⊆ `anexos.u_texto`. AH-04: title/footer = HitlCopy only |
| 3 | GET | `/api/recruitment/rct/{rctStamp}/compare` | query `firstSrtStamp`, `otherSrtStamp`, `topDiffCount?` | `CandidateComparisonDto` + `CriterionDiffDto[]` | Diffs carry quotes, not moral narrative |
| 4 | POST | `/api/recruitment/reprocess` | body `{ cveStamp, rctStamp, srtStamp, requestedBy, labGoRef? }` `:117-124` | same as enqueue (+ audit in message) | BR-06: `requestedBy` required; selection cols unchanged |
| 5 | GET | `/api/recruitment/hitl-copy` | — | `{ title, footer, semEvidencia, note }` `:98-106` | Title/footer OK; **exclude `note` from naive AH-04 sweep** |

**Not exposed (by design):** auto-select / auto-reject / cancel-job.

**Worker (Hangfire, not REST):** `AnalyzeCandidateJob.JobName` = `Recruitment.AnalyzeCandidate` (`Jobs/AnalyzeCandidateJob.cs:22`). Dashboard: `http://127.0.0.1:7298/hangfire` (Denilson only).

**DTO defs:** `Recruitment.Application/DTOs/RecruitmentDtos.cs`.

---

## Static conclusions for Dinis gate prep

| Hard ID | Static | Blocks Pass if live fails |
|---------|--------|---------------------------|
| AH-02 | **PASS** | P0 |
| AH-04 | **PASS*** | P0 |
| AH-08 | **PASS** | P0 |
| E1 enqueue refuse | **PASS** | process |
| E2 side-effects | **PASS fail-path / PARTIAL cancel** | process |

**Alfredo:** no Host access — this table is the deliverable.  
**Imran:** run smoke checklist on Denilson `127.0.0.1:7298` using route map above + SQL overlay `scripts/Recruitment_AddIaColumns.sql`.

**Known static gaps (not Fail on AH-02/04/08):** ranking read-path does not re-`AssertQuoteIsSubset` against live `u_texto` (`GetRctRankingQuery.cs` parse-only `:68-81`); `FailAsync` does not clear prior score; no cancel API; lab weights table `u_rec_ia_crt` (prod must mirror RCT).
