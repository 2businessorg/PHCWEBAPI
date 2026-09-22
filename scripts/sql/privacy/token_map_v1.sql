-- TokenMap schema (pseudonymization v1)
-- Encrypted at rest; ACL: worker service account only; never send rows to cloud.
-- Placeholders only — apply via Ops; do not embed secrets here.

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'privacy')
    EXEC(N'CREATE SCHEMA privacy');
GO

IF OBJECT_ID(N'privacy.token_map', N'U') IS NULL
BEGIN
    CREATE TABLE privacy.token_map (
        id                BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        session_id        UNIQUEIDENTIFIER NOT NULL,
        document_id       NVARCHAR(128) NOT NULL,
        token             NVARCHAR(64) NOT NULL,
        entity_type       NVARCHAR(64) NOT NULL,
        original_value    VARBINARY(MAX) NOT NULL,   -- AES-GCM ciphertext
        normalized_value  VARBINARY(MAX) NULL,
        value_hmac        CHAR(64) NOT NULL,
        confidence        REAL NOT NULL,
        source_recognizer NVARCHAR(128) NULL,
        created_at        DATETIMEOFFSET NOT NULL CONSTRAINT DF_token_map_created DEFAULT (SYSUTCDATETIME()),
        expires_at        DATETIMEOFFSET NOT NULL,
        CONSTRAINT UQ_token_map_session_doc_token UNIQUE (session_id, document_id, token)
    );

    CREATE INDEX ix_token_map_expiry ON privacy.token_map (expires_at);
    CREATE INDEX ix_token_map_hmac ON privacy.token_map (session_id, document_id, value_hmac);
END
GO

-- Access log stub (metadata only — never plaintext originals)
IF OBJECT_ID(N'privacy.token_map_access_log', N'U') IS NULL
BEGIN
    CREATE TABLE privacy.token_map_access_log (
        id           BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        accessed_at  DATETIMEOFFSET NOT NULL CONSTRAINT DF_token_map_access_at DEFAULT (SYSUTCDATETIME()),
        session_id   UNIQUEIDENTIFIER NOT NULL,
        document_id  NVARCHAR(128) NOT NULL,
        token        NVARCHAR(64) NOT NULL,
        operation    NVARCHAR(64) NOT NULL,
        actor        NVARCHAR(128) NULL
    );

    CREATE INDEX ix_token_map_access_session ON privacy.token_map_access_log (session_id, accessed_at);
END
GO
