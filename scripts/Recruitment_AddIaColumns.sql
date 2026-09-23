-- =============================================================================
-- Recrutamento x IA — schema overlay (v1)
-- Run against demo BD (e.g. OnTS_2BusinessIA). Review column names on customer DBs.
-- Chosen U_* columns (hypotheses verified vs PHC U_ convention — adjust if collide):
--   cve.u_estadoia, anexos.u_texto, srt.u_scoreia / u_justia / u_modeloia /
--   u_prmveria / u_stampia / u_auditia
-- SCAMPOS: the name after u_ is at most 8 characters.
-- Retired long names (do not create): u_promptveria, u_auditoriaia.
-- Outbox: u_rec_ia_outbox | Criteria bridge: u_rec_ia_crt
-- =============================================================================

IF COL_LENGTH('cve', 'u_estadoia') IS NULL
    ALTER TABLE cve ADD u_estadoia VARCHAR(20) NULL;

IF COL_LENGTH('anexos', 'u_texto') IS NULL
    ALTER TABLE anexos ADD u_texto NVARCHAR(MAX) NULL;

IF COL_LENGTH('srt', 'u_scoreia') IS NULL
    ALTER TABLE srt ADD u_scoreia DECIMAL(9,4) NULL;

IF COL_LENGTH('srt', 'u_justia') IS NULL
    ALTER TABLE srt ADD u_justia NVARCHAR(MAX) NULL;

IF COL_LENGTH('srt', 'u_modeloia') IS NULL
    ALTER TABLE srt ADD u_modeloia VARCHAR(100) NULL;

IF COL_LENGTH('srt', 'u_prmveria') IS NULL AND COL_LENGTH('srt', 'u_promptveria') IS NOT NULL
    EXEC sp_rename 'srt.u_promptveria', 'u_prmveria', 'COLUMN';

IF COL_LENGTH('srt', 'u_prmveria') IS NULL
    ALTER TABLE srt ADD u_prmveria VARCHAR(50) NULL;

IF COL_LENGTH('srt', 'u_stampia') IS NULL
    ALTER TABLE srt ADD u_stampia DATETIME NULL;

IF COL_LENGTH('srt', 'u_auditia') IS NULL AND COL_LENGTH('srt', 'u_auditoriaia') IS NOT NULL
    EXEC sp_rename 'srt.u_auditoriaia', 'u_auditia', 'COLUMN';

IF COL_LENGTH('srt', 'u_auditia') IS NULL
    ALTER TABLE srt ADD u_auditia NVARCHAR(MAX) NULL;
GO

IF OBJECT_ID('u_rec_ia_outbox', 'U') IS NULL
BEGIN
    CREATE TABLE u_rec_ia_outbox (
        id              BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_u_rec_ia_outbox PRIMARY KEY,
        cvestamp        VARCHAR(50)  NOT NULL,
        rctstamp        VARCHAR(50)  NOT NULL,
        srtstamp        VARCHAR(50)  NOT NULL,
        anexostamp      VARCHAR(50)  NOT NULL,
        estado          VARCHAR(20)  NOT NULL,
        created_at_utc  DATETIME2    NOT NULL,
        started_at_utc  DATETIME2    NULL,
        heartbeat_at_utc DATETIME2   NULL,
        completed_at_utc DATETIME2   NULL,
        error_msg       NVARCHAR(500) NULL,
        lab_go_ref      NVARCHAR(200) NULL
    );
    CREATE INDEX IX_u_rec_ia_outbox_estado ON u_rec_ia_outbox(estado, heartbeat_at_utc);
    CREATE INDEX IX_u_rec_ia_outbox_srt ON u_rec_ia_outbox(srtstamp, estado);
END
GO

IF OBJECT_ID('u_rec_ia_crt', 'U') IS NULL
BEGIN
    CREATE TABLE u_rec_ia_crt (
        id          BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_u_rec_ia_crt PRIMARY KEY,
        rctstamp    VARCHAR(50)  NOT NULL,
        codigo      VARCHAR(40)  NOT NULL,
        rotulo      NVARCHAR(200) NOT NULL,
        peso        DECIMAL(9,4) NOT NULL,
        hints       NVARCHAR(1000) NULL,
        ordem       INT NOT NULL CONSTRAINT DF_u_rec_ia_crt_ordem DEFAULT(0),
        CONSTRAINT UQ_u_rec_ia_crt UNIQUE (rctstamp, codigo)
    );
END
GO

-- =============================================================================
-- LAB SEED ONLY (demo Maputo ERP support). Production MUST copy weights from
-- native RCT characteristics — never leave hardcoded forever (Gate Dinis).
-- Replace @LabRctStamp before running.
-- =============================================================================
/*
DECLARE @LabRctStamp VARCHAR(50) = 'REPLACE_WITH_RCTSTAMP';

DELETE FROM u_rec_ia_crt WHERE rctstamp = @LabRctStamp;

INSERT INTO u_rec_ia_crt (rctstamp, codigo, rotulo, peso, hints, ordem) VALUES
(@LabRctStamp, 'CRT-EXP',  N'Experiencia profissional relevante (anos)', 25,
 'ERP|suporte|helpdesk|implementacao|anos', 1),
(@LabRctStamp, 'CRT-FORM', N'Formacao academica / tecnica', 15,
 'licenciatura|curso tecnico|certificacao|bacharel', 2),
(@LabRctStamp, 'CRT-STACK',N'Stack / ferramentas (ERP, SQL, Excel, tickets)', 25,
 'PHC|SAP|Primavera|SQL|Excel|Jira|ServiceNow', 3),
(@LabRctStamp, 'CRT-IDIO', N'Idiomas (PT obrigatorio; EN diferencial)', 15,
 'portugues|ingles|English|Portuguese|fluente', 4),
(@LabRctStamp, 'CRT-SOFT', N'Soft skills operacionais', 10,
 'atendimento|comunicacao|equipa|documentacao|turno', 5),
(@LabRctStamp, 'CRT-MZ',   N'Disponibilidade / contexto MZ', 10,
 'Maputo|Mozambique|Mocambique|presencial|imediato', 6);
*/
