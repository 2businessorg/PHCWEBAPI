-- =============================================
-- Script: Adicionar colunas de identificação de usuário à tabela u_logs
-- Data: 2026-02-17
-- Descrição: Adiciona userId e username para rastrear quem fez cada request
-- =============================================

USE [BILENE_DESENV];  -- Alterar para o seu banco de dados
GO

PRINT '🔄 Adicionando colunas de usuário à tabela u_logs...';
GO

-- Verificar se a coluna userId já existe
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'dbo.u_logs') 
    AND name = 'userId'
)
BEGIN
    ALTER TABLE [dbo].[u_logs]
    ADD [userId] NVARCHAR(450) NULL;
    
    PRINT '✅ Coluna [userId] adicionada com sucesso';
END
ELSE
BEGIN
    PRINT '⚠️ Coluna [userId] já existe';
END
GO

-- Verificar se a coluna username já existe
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'dbo.u_logs') 
    AND name = 'username'
)
BEGIN
    ALTER TABLE [dbo].[u_logs]
    ADD [username] NVARCHAR(256) NULL;
    
    PRINT '✅ Coluna [username] adicionada com sucesso';
END
ELSE
BEGIN
    PRINT '⚠️ Coluna [username] já existe';
END
GO

-- Criar índice composto para consultas por usuário
IF NOT EXISTS (
    SELECT * FROM sys.indexes 
    WHERE name = 'idx_ulogs_user' 
    AND object_id = OBJECT_ID(N'dbo.u_logs')
)
BEGIN
    CREATE NONCLUSTERED INDEX [idx_ulogs_user]
    ON [dbo].[u_logs] ([username], [userId])
    INCLUDE ([data], [code], [operation])
    WITH (FILLFACTOR = 90);
    
    PRINT '✅ Índice [idx_ulogs_user] criado com sucesso';
END
ELSE
BEGIN
    PRINT '⚠️ Índice [idx_ulogs_user] já existe';
END
GO

-- Criar índice para consultas por userId
IF NOT EXISTS (
    SELECT * FROM sys.indexes 
    WHERE name = 'idx_ulogs_userid' 
    AND object_id = OBJECT_ID(N'dbo.u_logs')
)
BEGIN
    CREATE NONCLUSTERED INDEX [idx_ulogs_userid]
    ON [dbo].[u_logs] ([userId])
    INCLUDE ([data], [username], [operation])
    WITH (FILLFACTOR = 90);
    
    PRINT '✅ Índice [idx_ulogs_userid] criado com sucesso';
END
ELSE
BEGIN
    PRINT '⚠️ Índice [idx_ulogs_userid] já existe';
END
GO

PRINT '✅ Script concluído com sucesso!';
PRINT '';
PRINT '📊 Estrutura atualizada da tabela u_logs:';
GO

-- Mostrar estrutura da tabela
SELECT 
    COLUMN_NAME as 'Coluna',
    DATA_TYPE as 'Tipo',
    CHARACTER_MAXIMUM_LENGTH as 'Tamanho',
    IS_NULLABLE as 'Nullable'
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'u_logs'
ORDER BY ORDINAL_POSITION;
GO

PRINT '';
PRINT '📊 Índices da tabela u_logs:';
GO

-- Mostrar índices
SELECT 
    i.name AS 'Nome do Índice',
    i.type_desc AS 'Tipo'
FROM sys.indexes i
WHERE i.object_id = OBJECT_ID(N'dbo.u_logs')
ORDER BY i.name;
GO
