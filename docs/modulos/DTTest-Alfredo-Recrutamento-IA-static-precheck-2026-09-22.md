# DTTest Alfredo — Recrutamento × IA — static pre-check (2026-09-22)

**Repo:** `2businessorg/PHCWEBAPI` (local often `PHCAPI`)  
**Amarras:** AH-REC-IA-v1 · DTTest lab script 2026-09-22 · docs `docs/modulos/Recrutamento-IA.md`  
**Scope:** READ-ONLY static + unit-test evidence. **Not** live Pass Dinis.  
**Authoring:** Cloud Agent investigation for Alfredo.

---

## 1) Commit / SHA confirmation

| Check | Result |
|-------|--------|
| Local `main` HEAD | `14204ad2cbe3fbf6f3434b38ea13386b5c7790f3` |
| Subject | `feat(recruitment): Recrutamento × IA — AH/BR/HITL (PR #2)` |
| `origin/main` | **same** `14204ad` (confirmed via fetch) |
| Module tree | `src/Modules/Pessoal/Recruitment/{Domain,Application,Infrastructure,Presentation}` |
| SQL overlay | `scripts/Recruitment_AddIaColumns.sql` |
| Docs | `docs/modulos/Recrutamento-IA.md`, `docs/ai/Recrutamento-IA.md` |
| Tests | `tests/Recruitment.Application.Tests` |

**Verdict:** Recruitment IA module **is present** on `main` at squash `14204ad`.

---

## 2) Endpoint map — `/api/recruitment/...`

**Controller:** `src/Modules/Pessoal/Recruitment/Recruitment.Presentation/REST/Controllers/RecruitmentIaController.cs`  
**Route prefix:** `api/recruitment`  
**Auth:** `[Authorize(Policy = AppPolicies.ApiAccess)]` on all actions  
**ModuleAuthorization:** prefix `/api/recruitment` → pack `Gestão` (`src/PHCAPI.Host/appsettings.json`)

| Method | Route | Body / query | Response shape | Handler |
|--------|-------|--------------|----------------|---------|
| POST | `/api/recruitment/enqueue` | `{ cveStamp, rctStamp, srtStamp, labGoRef? }` | `SingleItemResponseDTO<EnqueueAnalysisResultDto>` — always **HTTP 200**; refuse = `enqueued:false` | `EnqueueAnalysisCommand` → `RecruitmentEnqueueService` → Hangfire `AnalyzeCandidateJob` |
| GET | `/api/recruitment/rct/{rctStamp}/ranking` | path `rctStamp` | `RctRankingDto` (title/footer HITL + candidates: score, estadoIa, breakdown w/ quote) | `GetRctRankingQuery` |
| GET | `/api/recruitment/rct/{rctStamp}/compare` | `firstSrtStamp`, `otherSrtStamp`, `topDiffCount=3` | `CandidateComparisonDto` (top crt diffs + quotes) | `CompareCandidatesQuery` |
| POST | `/api/recruitment/reprocess` | `{ cveStamp, rctStamp, srtStamp, requestedBy, labGoRef? }` | same as enqueue (+ audit note in message) | `ReprocessAnalysisCommand` (BR-06) |
| GET | `/api/recruitment/hitl-copy` | — | `{ title, footer, semEvidencia, note }` | inline constants |

**DTOs:** `src/Modules/Pessoal/Recruitment/Recruitment.Application/DTOs/RecruitmentDtos.cs`

```
EnqueueAnalysisResultDto { enqueued, outboxId?, message, criteriaCount, intervenienteCount }
RankedCandidateDto { position, srtStamp, cveStamp, candidateName?, scoreTotal?, estadoIa?, condp?, modelo?, promptVer?, stampIa?, breakdown[] }
CriterionBreakdownDto { code, label, weight, note, quote?, quoteOffset?, semEvidencia, conflito, conflictQuotes[], justificationPt }
JustificationPayloadDto (persisted in srt.u_justia): { engine, prompt_ver, stamp_utc, used_llm, total, breakdown }
```

**Worker (not HTTP):** Hangfire job `Recruitment.AnalyzeCandidate`  
`src/Modules/Pessoal/Recruitment/Recruitment.Application/Jobs/AnalyzeCandidateJob.cs`  
Scheduled by `HangfireRecruitmentJobScheduler` after successful outbox insert.

**No endpoints for:** auto-select / auto-reject / cancel-job (AH-04 / BR-03). Cancel ≈ Hangfire dashboard abort + outbox orphan / reprocess path.

---

## 3) Static DoD pre-check (AH hard + enqueue)

### AH-02 — quote ⊆ `u_texto` (invent → fail) → **PASS (static / unit)**

| Evidence | Path |
|----------|------|
| Quote generated from contiguous excerpt of OCR text; `AssertQuoteIsSubset` | `RubricEvidenceScorer.cs` L68–69, L189–201 |
| Job re-asserts every `note>0` quote before `SaveScore` | `AnalyzeCandidateJob.cs` L158–163 |
| Unit: invented quote throws `*AH-02*` | `RubricEvidenceScorerTests.AssertQuoteIsSubset_InventedQuote_ThrowsAh02` |
| Unit: no skill → note=0, no quote | `Score_NoCitationForCriterion_ContributionIsZero` |
| Docs claim | `docs/modulos/Recrutamento-IA.md` AH-02 row |

**Live attack still required:** after happy path, falsify `srt.u_justia` quote (or inject via debugger) and confirm UI/API treats as fail / RH falsifier. Ranking **does not** re-validate quotes against live `anexos.u_texto` on read — only write path enforces.

### AH-04 — ban «seleccionado/rejeitado/avançado pela IA» → **PASS (static) with notes**

| Evidence | Path |
|----------|------|
| Forbidden list | `HitlCopy.ForbiddenPhrases` in `IaEstados.cs` |
| Runtime guard | `ForbiddenCopyGuard.cs`; used in scorer, job JSON, ranking title/footer, compare |
| Unit theory | `ForbiddenCopyGuardTests` |
| Tree grep (production copy) | **No** decision-attributing product strings outside ban-list / test InlineData / meta note |
| HITL title/footer | explicit “input ao RH / decisao humana” |

**Notes / residual risk for live A4 sweep:**

1. `GET /hitl-copy` **documents** the ban in `note` and therefore **contains** the substrings — exclude meta endpoints from naive string sweep, or expect a documented exception.
2. Forbidden list uses ASCII `avancado` / `selecionado`; Unicode `avançado` (ç) is **not** listed — add if live PT copy uses cedilla.
3. No PHC UI / email / export in this repo to sweep — Alfredo must include PHC chrome if any.

### AH-08 — OCR error / empty `u_texto` → no score; `estado_ia=erro` → **PASS (static / unit)**

| Evidence | Path |
|----------|------|
| On `!ocr.Success \|\| empty text`: `ClearScoreOnOcrErrorAsync`, `SetEstadoIa(erro)`, `MarkError`, **no** `SaveScore` / **no** `Score()` | `AnalyzeCandidateJob.cs` L116–124 |
| SQL clears `u_scoreia` + just/modelo/prompt/stamp | `SrtScoreRepository.ClearScoreOnOcrErrorAsync` |
| Unit | `AnalyzeCandidateJobTests.Execute_OcrFailure_SetsErro_NoScorePersisted` |
| Empty text in scorer → all `NoEvidence` (note=0) | `RubricEvidenceScorer.ScoreCriterion` |

**Live attack:** corrupt PDF / empty `bdados` OCR → assert ranking shows `estadoIa=erro`, `scoreTotal=null`, Triar manual in PHC (API has no Triar action — human in PHC).

### E1 — enqueue without crt / RCTCLB → **PASS (static / unit)**

| Case | Behaviour |
|------|-----------|
| `crtCount < 1` | `Enqueued=false`; message crt; **no** outbox (`MockBehavior.Strict` proves no call) |
| `clbCount < 1` and not lab GO | refuse; no outbox |
| Lab exception | only if `AllowLabEnqueueWithoutRctclb=true` **and** `LabGoRef` set |
| Default config | `AllowLabEnqueueWithoutRctclb: false` |

**Caveat:** HTTP still **200** with soft refuse (not 4xx). Side-effect: on refuse, `cve.u_estadoia` is **not** set to scored/ok (estado only written after successful outbox insert → `pendente`).

### E2 — cancel / fail mid-job → **PARTIAL (static)**

| Path | State |
|------|-------|
| OCR/fail paths | `outbox.estado=error`, `cve.u_estadoia=erro`, score cleared or never saved; BR-03/04 assert selection/`condp` unchanged |
| Hangfire `AutomaticRetry(Attempts=2, Fail)` | retries then fail |
| **No HTTP cancel** | abort via `/hangfire` or kill worker; orphans via `GetOrphansAsync(OrphanTtlMinutes)` + `/reprocess` |
| Ghost score | OCR path clears; mid-fail before SaveScore leaves prior score unless cleared — **watch** fail-after-partial-write (AH-02 throw after Score but before Save is OK; FailAsync sets erro but does **not** always `ClearScore`) |

**E2 live must:** force Hangfire fail mid-flight; assert no phantom “ok” score; audit outbox `error_msg`.

### E3 / E4 (brief)

- **E3:** outbox idempotent while `pending|processing` same `srtstamp` (`TryEnqueueAsync` IF EXISTS → null). Reprocess after `done/error` allowed.
- **E4:** empty crt refused at enqueue and again in job (`AH-01: lista crt vazia`).

---

## 4) How Alfredo runs live DTTest

### 4.1 Host project

```text
Project: src/PHCAPI.Host
URL:     http://localhost:7298  (launchSettings “PHCAPI Development (7298)”)
Hangfire dashboard: /hangfire
Swagger: /swagger
```

```powershell
cd <repo>\src\PHCAPI.Host
dotnet run
# or: <repo>\scripts\start-phcapi.ps1
```

### 4.2 Required SQL (demo BD e.g. OnTS_2BusinessIA)

1. Run `scripts/Recruitment_AddIaColumns.sql` (adds `cve.u_estadoia`, `anexos.u_texto`, `srt.u_*ia*`, `u_rec_ia_outbox`, `u_rec_ia_crt`).
2. Uncomment lab seed block; set `@LabRctStamp` to lab RCT; insert CRT-* weights.
3. Ensure RCT has ≥1 row in `rctclb` **or** enable lab GO flags (below).
4. Ensure CVE has CV anexo: `anexos.oritable='cve'`, `recstamp=cvestamp`, `tipo=1`, bytes in `bdados`.
5. Know stamps: `cveStamp`, `rctStamp`, `srtStamp`.

### 4.3 Config / env

| Key | Where | Lab value |
|-----|-------|-----------|
| `ConnectionStrings:DBconnect` | `appsettings.json` / Development / user-secrets / env | demo SQL Server |
| `RecruitmentIa:Enabled` | `true` | |
| `RecruitmentIa:EnableCloudLlm` | **`false`** (v1 deterministic) | |
| `RecruitmentIa:AllowLabEnqueueWithoutRctclb` | `false` unless GO Denilson lab exception | |
| `RecruitmentIa:LabGoRef` | null or GO ref string | |
| `DocumentTextExtraction:*` | OCR / Tesseract paths if PDFs scanned | |
| `JWT:*` | valid secret for login | |
| `ASPNETCORE_ENVIRONMENT` | `Development` | |

User must have module pack **Gestão** (ModuleAuthorization for `/api/recruitment`).

### 4.4 curl / HTTP attack plan

Replace `BASE`, stamps, credentials.

```bash
BASE=http://localhost:7298

# 0) Login
TOKEN=$(curl -s -X POST "$BASE/api/auth/login" \
  -H 'Content-Type: application/json' \
  -d '{"username":"USER","password":"PASS"}' | jq -r '.token // .Token // .data.token')

AUTH="Authorization: Bearer $TOKEN"

# E1 — refuse: RCT without crt / without RCTCLB
curl -s -X POST "$BASE/api/recruitment/enqueue" -H "$AUTH" -H 'Content-Type: application/json' \
  -d '{"cveStamp":"CVE","rctStamp":"RCT_NO_CRT","srtStamp":"SRT"}' | jq .
# Expect: enqueued=false, criteriaCount=0 (or intervenienteCount=0), NO new outbox row, u_estadoia not flipped to ok

# A1 — happy path enqueue
curl -s -X POST "$BASE/api/recruitment/enqueue" -H "$AUTH" -H 'Content-Type: application/json' \
  -d '{"cveStamp":"CVE_OK","rctStamp":"RCT_LAB","srtStamp":"SRT_OK"}' | jq .
# Expect: enqueued=true, outboxId set → watch Hangfire Recruitment.AnalyzeCandidate

# Poll ranking
curl -s "$BASE/api/recruitment/rct/RCT_LAB/ranking" -H "$AUTH" | jq .
# Expect: scoreTotal>0, breakdown[].quote ⊆ anexos.u_texto, modelo/promptVer present (AH-07)

# Compare #1 vs #k
curl -s "$BASE/api/recruitment/rct/RCT_LAB/compare?firstSrtStamp=SRT1&otherSrtStamp=SRT2&topDiffCount=3" \
  -H "$AUTH" | jq .

# AH-02 falsifier (SQL after success)
# UPDATE anexos SET u_texto = u_texto WHERE ...;  -- keep real text
# Manually craft JSON in srt.u_justia with quote NOT in u_texto → RH falsifier / next job AssertQuoteIsSubset on reprocess
# Prefer: enqueue CV without skill X → note=0 SemEvidencia (A2)

# AH-08 — OCR fail
# Use unscannable/empty CV bytes → enqueue → expect estadoIa=erro, scoreTotal=null, outbox error
curl -s "$BASE/api/recruitment/rct/RCT_LAB/ranking" -H "$AUTH" | jq '.item.candidates[] | {srtStamp,estadoIa,scoreTotal}'

# E2 — fail mid: Dashboard /hangfire → delete/fail processing job; assert outbox error/orphan + no selection side-effect
# SQL: SELECT estado,apurado,entrevista,condp,u_scoreia,u_estadoia FROM srt/cve BEFORE/AFTER

# Reprocess (BR-06)
curl -s -X POST "$BASE/api/recruitment/reprocess" -H "$AUTH" -H 'Content-Type: application/json' \
  -d '{"cveStamp":"CVE","rctStamp":"RCT","srtStamp":"SRT","requestedBy":"alfredo.dttest"}' | jq .

# AH-04 sweep
curl -s "$BASE/api/recruitment/hitl-copy" -H "$AUTH" | jq .
rg -i 'seleccionad|rejeitad.*ia|avan[cç]ad.*ia|pela ia' src docs --glob '!**/bin/**'
# Exclude intentional ban documentation in hitl-copy.note / ForbiddenPhrases / tests
```

### 4.5 SQL checks during attacks

```sql
SELECT cvestamp, u_estadoia FROM cve WHERE cvestamp = @cve;
SELECT anexosstamp, LEFT(u_texto,200) FROM anexos WHERE oritable='cve' AND recstamp=@cve;
SELECT srtstamp, u_scoreia, u_modeloia, u_promptveria, u_stampia, condp, estado, apurado, entrevista
FROM srt WHERE srtstamp=@srt;
SELECT id, estado, error_msg, heartbeat_at_utc FROM u_rec_ia_outbox WHERE srtstamp=@srt ORDER BY id DESC;
SELECT * FROM u_rec_ia_crt WHERE rctstamp=@rct;
```

### 4.6 Fixture notes in repo

`tests/Recruitment.Application.Tests/Fixtures/AlfredoDtTestFixtureNotes.cs` — demo OnTS_2BusinessIA checklist (quote ⊆ texto, OCR→erro/null score, condp/selection unchanged).

---

## 5) Gaps blocking Pass Dinis

Hard DoD from DTTest: if **≥1** of AH-02 / AH-04 / AH-08 fail live → **NÃO PASSA**. Static is green; live still open.

| # | Gap | Severity | Why it blocks / delays Pass |
|---|-----|----------|------------------------------|
| G1 | **No live DTTest execution yet** (script: “execução quando houver ecrã/PR”) | P0 process | Static ≠ Pass Dinis |
| G2 | **No PHC verification UI** in this repo (ranking API only; Triar/aceitar humano = PHC) | P0 UX | Checklist ecrã smoke unmet in-API |
| G3 | **AH-02 read-path** does not re-check quotes vs `u_texto` | P1 | DB-tampered `u_justia` can still appear in ranking |
| G4 | **E2 cancel** has no first-class API; fail-after-score-without-clear edge | P1 | Side-effect audit incomplete until live |
| G5 | Soft enqueue refuse (**HTTP 200** + `enqueued:false`) | P2 | Clients may miss BR-01 refuse |
| G6 | `PhcAvisoServiceStub` only logs — real `XcUtil.criaAvs` not wired | P1 BR-05 | Not full Analisar notify path |
| G7 | Weights via **`u_rec_ia_crt` lab seed** — prod must mirror native RCT (Gate Dinis) | P0 prod | Lab OK; product Pass blocked if hardcode remains |
| G8 | AH-04: `hitl-copy.note` + missing Unicode `avançado` | P2 | Naive A4 sweep / PT cedilla |
| G9 | OCR/Tesseract host deps + real CV anexos on demo BD | P1 ops | AH-08 live needs illegible PDF fixture |
| G10 | Module pack **Gestão** + JWT + `DBconnect` to demo BD | P1 ops | Alfredo blocked without lab auth/DB |
| G11 | Folha AH itself: **“Não é Pass de produto”** — freeze host fila + GO Denilson still required for slice | P0 scope | Lab gate ≠ product Pass |

### Static matrix (this report)

| Ataque | Static | Live |
|--------|--------|------|
| AH-02 | **PASS** | TBD Alfredo |
| AH-04 | **PASS*** (*notes G8) | TBD sweep UI/PHC |
| AH-08 | **PASS** | TBD OCR fixture |
| E1 | **PASS** | TBD |
| E2 | **PARTIAL** | TBD Hangfire abort |
| A1–A8 | unit covers A1/A2/A4/A5/A8 pieces | full script pending |

**Veredicto gate (agora):** static pre-check **não bloqueia** arranque do lab DTTest; **Pass Dinis ainda NÃO** — falta evidência live + ecrã HITL PHC + fecho G3/G6/G7 conforme severidade.

**Top 3 must-fix antes de demo Dinis:**

1. Live AH-02 / AH-08 / A4 com dumps `u_texto` + ranking JSON.  
2. Enqueue E1/E2 com before/after `condp|estado|apurado|entrevista`.  
3. Confirm ranking surface RH (PHC or temporary HITL consumer) shows quotes + human actions.

**Blocks demo?** Static code path: **não**. Live/process: **sim** until G1+G2+hard DoD live green.

---

## File index (quick)

```
src/Modules/Pessoal/Recruitment/Recruitment.Presentation/REST/Controllers/RecruitmentIaController.cs
src/Modules/Pessoal/Recruitment/Recruitment.Application/{Jobs,Scoring,Services,Features,DTOs,Options}/
src/Modules/Pessoal/Recruitment/Recruitment.Infrastructure/{Repositories,Hangfire*,Notifications}/
src/Modules/Pessoal/Recruitment/Recruitment.Domain/{Constants/IaEstados.cs,Entities,Repositories}/
scripts/Recruitment_AddIaColumns.sql
docs/modulos/Recrutamento-IA.md
tests/Recruitment.Application.Tests/
src/PHCAPI.Host/{Program.cs,appsettings.json,Properties/launchSettings.json}
```
