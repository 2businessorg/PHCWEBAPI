# docs-ai — Recrutamento x IA

See full module notes: [../modulos/Recrutamento-IA.md](../modulos/Recrutamento-IA.md)

## Quick map

- Spec law: AH-REC-IA-v1 (AH-01…08) + BR-REC-IA-v1
- Engine: deterministic `rubric-evidence-v1` / `deterministic-v1` (embeddings REJECTED as sole engine)
- OCR: shared `IDocumentTextExtractor` (do not fork)
- Persist score: **SRT** `u_scoreia` + `u_justia`
- Queue: Hangfire on **PHCAPI.Host** job `Recruitment.AnalyzeCandidate`
- Weights: read from `u_rec_ia_crt` (must reflect RCT); lab CRT-* seed is demo-only
