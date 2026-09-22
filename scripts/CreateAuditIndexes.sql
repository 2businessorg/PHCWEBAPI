-- ===============================================
-- Script: Criar Índices para Tabela ulogs
-- Objetivo: Melhorar performance de queries de auditoria
-- Data: 2024-02-10
-- ===============================================

USE PHC; -- ⚠️ ALTERE PARA O NOME DO SEU BANCO!
GO

-- Verificar se os índices já existem
PRINT '========================================';
PRINT 'Verificando índices existentes...';
PRINT '========================================';

SELECT 
    i.name AS IndexName,
    OBJECT_NAME(i.object_id) AS TableName,
    COL_NAME(ic.object_id, ic.column_id) AS ColumnName,
    i.type_desc AS IndexType
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
WHERE OBJECT_NAME(i.object_id) = 'ulogs'
ORDER BY i.name, ic.index_column_id;
GO

-- ===============================================
-- CRIAR ÍNDICES
-- ===============================================

-- ✅ 1. Índice na coluna Data (ordenação e filtro por data)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ulogs_Data' AND object_id = OBJECT_ID('ulogs'))
BEGIN
    PRINT 'Criando índice IX_ulogs_Data...';
    CREATE NONCLUSTERED INDEX IX_ulogs_Data
    ON ulogs (data DESC)
    INCLUDE (ulogsstamp, requestid, code, operation);
    PRINT '✅ Índice IX_ulogs_Data criado com sucesso!';
END
ELSE
BEGIN
    PRINT '⚠️ Índice IX_ulogs_Data já existe.';
END
GO

-- ✅ 2. Índice na coluna RequestId (correlação de logs)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ulogs_RequestId' AND object_id = OBJECT_ID('ulogs'))
BEGIN
    PRINT 'Criando índice IX_ulogs_RequestId...';
    CREATE NONCLUSTERED INDEX IX_ulogs_RequestId
    ON ulogs (requestid)
    INCLUDE (data, code, operation);
    PRINT '✅ Índice IX_ulogs_RequestId criado com sucesso!';
END
ELSE
BEGIN
    PRINT '⚠️ Índice IX_ulogs_RequestId já existe.';
END
GO

-- ✅ 3. Índice na coluna Operation (filtro por operação)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ulogs_Operation' AND object_id = OBJECT_ID('ulogs'))
BEGIN
    PRINT 'Criando índice IX_ulogs_Operation...';
    CREATE NONCLUSTERED INDEX IX_ulogs_Operation
    ON ulogs (operation)
    INCLUDE (data, ulogsstamp);
    PRINT '✅ Índice IX_ulogs_Operation criado com sucesso!';
END
ELSE
BEGIN
    PRINT '⚠️ Índice IX_ulogs_Operation já existe.';
END
GO

-- ✅ 4. Índice composto Data + RequestId (query mais comum)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ulogs_Data_RequestId' AND object_id = OBJECT_ID('ulogs'))
BEGIN
    PRINT 'Criando índice IX_ulogs_Data_RequestId...';
    CREATE NONCLUSTERED INDEX IX_ulogs_Data_RequestId
    ON ulogs (data DESC, requestid)
    INCLUDE (code, operation);
    PRINT '✅ Índice IX_ulogs_Data_RequestId criado com sucesso!';
END
ELSE
BEGIN
    PRINT '⚠️ Índice IX_ulogs_Data_RequestId já existe.';
END
GO

-- ===============================================
-- ESTATÍSTICAS DOS ÍNDICES
-- ===============================================

PRINT '';
PRINT '========================================';
PRINT 'Índices criados/existentes:';
PRINT '========================================';

SELECT 
    i.name AS IndexName,
    i.type_desc AS IndexType,
    STATS_DATE(i.object_id, i.index_id) AS LastUpdated,
    p.rows AS RowCount
FROM sys.indexes i
INNER JOIN sys.partitions p ON i.object_id = p.object_id AND i.index_id = p.index_id
WHERE OBJECT_NAME(i.object_id) = 'ulogs'
ORDER BY i.name;
GO

-- ===============================================
-- TESTAR PERFORMANCE
-- ===============================================

PRINT '';
PRINT '========================================';
PRINT 'Testando performance...';
PRINT '========================================';

-- Limpar cache (CUIDADO: Não usar em produção!)
-- DBCC DROPCLEANBUFFERS;
-- DBCC FREEPROCCACHE;

-- Teste 1: Buscar por data
SET STATISTICS TIME ON;
SET STATISTICS IO ON;

PRINT 'Teste 1: Buscar por data (últimos 7 dias)';
SELECT TOP 50 *
FROM ulogs
WHERE data >= DATEADD(DAY, -7, GETDATE())
ORDER BY data DESC;

PRINT 'Teste 2: Buscar por RequestId';
SELECT *
FROM ulogs
WHERE requestid = 'test-request-id'
ORDER BY data DESC;

PRINT 'Teste 3: Buscar por Operation';
SELECT TOP 50 *
FROM ulogs
WHERE operation LIKE '%GetAll%'
ORDER BY data DESC;

SET STATISTICS TIME OFF;
SET STATISTICS IO OFF;

PRINT '';
PRINT '========================================';
PRINT '✅ Script executado com sucesso!';
PRINT '========================================';
GO
