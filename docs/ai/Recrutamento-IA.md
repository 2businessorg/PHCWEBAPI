# docs-ai — Recrutamento x IA

See full module notes: [../modulos/Recrutamento-IA.md](../modulos/Recrutamento-IA.md)

## Quick map

- Spec law: AH-REC-IA-v1 (AH-01…08) + BR-REC-IA-v1
- Engine: Qwen cloud when `RecruitmentIa:EnableCloudLlm=true` (`qwen-cloud-v6`, `ILocalChatModel`). Assisted decision is only `avancar`, `em_duvida`, or `nao_avancar`. Scorecard fields for the later report live in `u_justia`; Presidio audit lives in `u_auditia`. Offline rubric `rubric-evidence-v1` only when the flag is false. Embeddings REJECTED as sole engine.
- OCR: shared `IDocumentTextExtractor` (do not fork)
- Persist score: **SRT** `u_scoreia` + `u_justia`
- Queue: Hangfire on **PHCAPI.Host** job `Recruitment.AnalyzeCandidate`
- Product API: `POST /api/recruitment/{idrct}/analyze` (resolves `rct.idrct`). Flat `POST /api/recruitment/enqueue` is legacy.
- SCAMPOS short columns: `srt.u_prmveria`, `srt.u_auditia`
- Weights: read from `u_rec_ia_crt` (must reflect RCT); lab CRT-* seed is demo-only
