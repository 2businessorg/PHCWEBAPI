-- =============================================
-- 🚀 SCRIPT DE OTIMIZAÇÃO - ÍNDICES PARA TABELA u_logs
-- =============================================
-- Database: BILENE_DESENV
-- Table: u_logs (Logs de Auditoria)
-- Purpose: Melhorar performance de consultas de logs com filtros dinâmicos
-- 
-- IMPORTANTE: Execute este script em ambiente de DESENVOLVIMENTO primeiro
--             e monitore o impacto antes de aplicar em PRODUÇÃO
-- =============================================

USE [BILENE_DESENV];
GO

PRINT '========================================';
PRINT 'Iniciando criação de índices em u_logs';
PRINT '========================================';
PRINT '';

-- =============================================
-- 1️⃣ ÍNDICE PRINCIPAL: Data (Campo mais consultado)
-- =============================================
-- Justificativa: 
--   - Todas as consultas ordenam por Data (DESC)
--   - Filtros por intervalo de data são comuns
--   - Melhora drasticamente ORDER BY e WHERE em datas
-- =============================================
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_u_logs_Data' AND object_id = OBJECT_ID('u_logs'))
BEGIN
    PRINT '✅ Criando índice: IX_u_logs_Data';
    CREATE NONCLUSTERED INDEX IX_u_logs_Data
    ON u_logs (data DESC)
    INCLUDE (u_logsstamp, requestId, code, operation, ip)
    WITH (
        PAD_INDEX = OFF,
        STATISTICS_NORECOMPUTE = OFF,
        SORT_IN_TEMPDB = ON,
        DROP_EXISTING = OFF,
        ONLINE = OFF,
        ALLOW_ROW_LOCKS = ON,
        ALLOW_PAGE_LOCKS = ON
    );
    PRINT '✅ Índice IX_u_logs_Data criado com sucesso!';
    PRINT '';
END
ELSE
BEGIN
    PRINT '⚠️  Índice IX_u_logs_Data já existe - pulando criação';
    PRINT '';
END
GO

-- =============================================
-- 2️⃣ ÍNDICE: RequestId (Correlation ID)
-- =============================================
-- Justificativa:
--   - Busca por Correlation ID é uma operação crítica
--   - Permite rastrear todos os logs de uma mesma request
--   - Queries com WHERE requestId = @requestId
-- =============================================
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_u_logs_RequestId' AND object_id = OBJECT_ID('u_logs'))
BEGIN
    PRINT '✅ Criando índice: IX_u_logs_RequestId';
    CREATE NONCLUSTERED INDEX IX_u_logs_RequestId
    ON u_logs (requestId)
    INCLUDE (data, code, operation, responseDesc)
    WITH (
        PAD_INDEX = OFF,
        STATISTICS_NORECOMPUTE = OFF,
        SORT_IN_TEMPDB = ON,
        DROP_EXISTING = OFF,
        ONLINE = OFF,
        ALLOW_ROW_LOCKS = ON,
        ALLOW_PAGE_LOCKS = ON
    );
    PRINT '✅ Índice IX_u_logs_RequestId criado com sucesso!';
    PRINT '';
END
ELSE
BEGIN
    PRINT '⚠️  Índice IX_u_logs_RequestId já existe - pulando criação';
    PRINT '';
END
GO

-- =============================================
-- 3️⃣ ÍNDICE: Code (Código de Resposta)
-- =============================================
-- Justificativa:
--   - Filtro comum para separar sucessos (0000) de erros (0015, etc.)
--   - Queries com WHERE code = @code
--   - Permite análise rápida de taxa de erro
-- =============================================
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_u_logs_Code' AND object_id = OBJECT_ID('u_logs'))
BEGIN
    PRINT '✅ Criando índice: IX_u_logs_Code';
    CREATE NONCLUSTERED INDEX IX_u_logs_Code
    ON u_logs (code)
    INCLUDE (data, operation, requestId)
    WITH (
        PAD_INDEX = OFF,
        STATISTICS_NORECOMPUTE = OFF,
        SORT_IN_TEMPDB = ON,
        DROP_EXISTING = OFF,
        ONLINE = OFF,
        ALLOW_ROW_LOCKS = ON,
        ALLOW_PAGE_LOCKS = ON
    );
    PRINT '✅ Índice IX_u_logs_Code criado com sucesso!';
    PRINT '';
END
ELSE
BEGIN
    PRINT '⚠️  Índice IX_u_logs_Code já existe - pulando criação';
    PRINT '';
END
GO

-- =============================================
-- 4️⃣ ÍNDICE: IP Address
-- =============================================
-- Justificativa:
--   - Útil para rastrear atividades de um IP específico
--   - Queries com WHERE ip = @ip ou ip LIKE @ip
--   - Segurança: identificar IPs suspeitos
-- =============================================
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_u_logs_Ip' AND object_id = OBJECT_ID('u_logs'))
BEGIN
    PRINT '✅ Criando índice: IX_u_logs_Ip';
    CREATE NONCLUSTERED INDEX IX_u_logs_Ip
    ON u_logs (ip)
    INCLUDE (data, operation, requestId)
    WITH (
        PAD_INDEX = OFF,
        STATISTICS_NORECOMPUTE = OFF,
        SORT_IN_TEMPDB = ON,
        DROP_EXISTING = OFF,
        ONLINE = OFF,
        ALLOW_ROW_LOCKS = ON,
        ALLOW_PAGE_LOCKS = ON
    );
    PRINT '✅ Índice IX_u_logs_Ip criado com sucesso!';
    PRINT '';
END
ELSE
BEGIN
    PRINT '⚠️  Índice IX_u_logs_Ip já existe - pulando criação';
    PRINT '';
END
GO

-- =============================================
-- 5️⃣ ÍNDICE COMPOSTO: Data + Code (Query Optimization)
-- =============================================
-- Justificativa:
--   - Combinar filtros comuns: "logs de erro nas últimas 24h"
--   - WHERE data >= @startDate AND code != '0000'
--   - Melhor performance que usar dois índices separados
-- =============================================
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_u_logs_Data_Code' AND object_id = OBJECT_ID('u_logs'))
BEGIN
    PRINT '✅ Criando índice composto: IX_u_logs_Data_Code';
    CREATE NONCLUSTERED INDEX IX_u_logs_Data_Code
    ON u_logs (data DESC, code)
    INCLUDE (operation, requestId, responseDesc)
    WITH (
        PAD_INDEX = OFF,
        STATISTICS_NORECOMPUTE = OFF,
        SORT_IN_TEMPDB = ON,
        DROP_EXISTING = OFF,
        ONLINE = OFF,
        ALLOW_ROW_LOCKS = ON,
        ALLOW_PAGE_LOCKS = ON
    );
    PRINT '✅ Índice IX_u_logs_Data_Code criado com sucesso!';
    PRINT '';
END
ELSE
BEGIN
    PRINT '⚠️  Índice IX_u_logs_Data_Code já existe - pulando criação';
    PRINT '';
END
GO

-- =============================================
-- 6️⃣ FULLTEXT INDEX: Para campos de texto (content, responseText, responseDesc)
-- =============================================
-- Justificativa:
--   - Campos JSON grandes (content, responseText)
--   - LIKE '%texto%' é lento sem Full-Text Search
--   - Permite pesquisas textuais muito mais rápidas
-- =============================================
-- NOTA: Full-Text requer configuração adicional no SQL Server
-- Se o banco não suportar, este bloco pode ser comentado

-- Criar Full-Text Catalog se não existir
IF NOT EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE name = 'u_logs_fulltext_catalog')
BEGIN
    PRINT '✅ Criando Full-Text Catalog: u_logs_fulltext_catalog';
    CREATE FULLTEXT CATALOG u_logs_fulltext_catalog AS DEFAULT;
    PRINT '✅ Full-Text Catalog criado com sucesso!';
    PRINT '';
END
ELSE
BEGIN
    PRINT '⚠️  Full-Text Catalog já existe - pulando criação';
    PRINT '';
END
GO

-- Verificar se a tabela tem PRIMARY KEY (necessário para Full-Text)
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
    WHERE TABLE_NAME = 'u_logs' 
    AND CONSTRAINT_TYPE = 'PRIMARY KEY'
)
BEGIN
    PRINT '⚠️  AVISO: Tabela u_logs não tem PRIMARY KEY';
    PRINT '⚠️  Full-Text Index requer PRIMARY KEY - criando no campo u_logsstamp';
    PRINT '';
    
    -- Criar PRIMARY KEY se não existir
    ALTER TABLE u_logs
    ADD CONSTRAINT PK_u_logs PRIMARY KEY (u_logsstamp);
    
    PRINT '✅ PRIMARY KEY criada em u_logsstamp';
    PRINT '';
END
GO

-- Criar Full-Text Index nos campos de texto
IF NOT EXISTS (
    SELECT 1 
    FROM sys.fulltext_indexes 
    WHERE object_id = OBJECT_ID('u_logs')
)
BEGIN
    PRINT '✅ Criando Full-Text Index em campos de texto';
    
    CREATE FULLTEXT INDEX ON u_logs
    (
        content LANGUAGE 1046,        -- Português
        responseText LANGUAGE 1046,   -- Português
        responseDesc LANGUAGE 1046,   -- Português
        operation LANGUAGE 1046       -- Português
    )
    KEY INDEX PK_u_logs
    ON u_logs_fulltext_catalog
    WITH (
        CHANGE_TRACKING AUTO,
        STOPLIST = SYSTEM
    );
    
    PRINT '✅ Full-Text Index criado com sucesso!';
    PRINT '';
END
ELSE
BEGIN
    PRINT '⚠️  Full-Text Index já existe - pulando criação';
    PRINT '';
END
GO

-- =============================================
-- 📊 ESTATÍSTICAS: Atualizar estatísticas dos índices
-- =============================================
PRINT '✅ Atualizando estatísticas da tabela u_logs...';
UPDATE STATISTICS u_logs WITH FULLSCAN;
PRINT '✅ Estatísticas atualizadas!';
PRINT '';
GO

-- =============================================
-- 📈 RELATÓRIO: Verificar índices criados
-- =============================================
PRINT '========================================';
PRINT '📊 RELATÓRIO DE ÍNDICES CRIADOS';
PRINT '========================================';
PRINT '';

SELECT 
    i.name AS [Nome do Índice],
    CASE 
        WHEN i.is_primary_key = 1 THEN 'PRIMARY KEY'
        WHEN i.type_desc = 'CLUSTERED' THEN 'CLUSTERED'
        ELSE 'NONCLUSTERED'
    END AS [Tipo],
    STUFF((
        SELECT ', ' + c.name
        FROM sys.index_columns ic
        INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
        WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.is_included_column = 0
        ORDER BY ic.key_ordinal
        FOR XML PATH('')
    ), 1, 2, '') AS [Colunas Indexadas],
    STUFF((
        SELECT ', ' + c.name
        FROM sys.index_columns ic
        INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
        WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.is_included_column = 1
        ORDER BY ic.index_column_id
        FOR XML PATH('')
    ), 1, 2, '') AS [Colunas Incluídas]
FROM sys.indexes i
WHERE i.object_id = OBJECT_ID('u_logs')
AND i.name IS NOT NULL
ORDER BY i.name;

PRINT '';
PRINT '========================================';
PRINT '✅ Script executado com SUCESSO!';
PRINT '========================================';
PRINT '';
PRINT '📌 PRÓXIMOS PASSOS:';
PRINT '   1. Monitorar performance das queries';
PRINT '   2. Verificar uso dos índices com: sys.dm_db_index_usage_stats';
PRINT '   3. Ajustar índices conforme padrões de uso real';
PRINT '';
GO

-- =============================================
-- 📊 QUERY DE MONITORAMENTO: Verificar uso dos índices
-- =============================================
-- Executar esta query periodicamente para ver quais índices estão sendo usados
/*
SELECT 
    OBJECT_NAME(s.object_id) AS [Tabela],
    i.name AS [Índice],
    s.user_seeks AS [Buscas],
    s.user_scans AS [Scans],
    s.user_lookups AS [Lookups],
    s.user_updates AS [Updates],
    s.last_user_seek AS [Última Busca],
    s.last_user_scan AS [Último Scan]
FROM sys.dm_db_index_usage_stats s
INNER JOIN sys.indexes i ON s.object_id = i.object_id AND s.index_id = i.index_id
WHERE OBJECT_NAME(s.object_id) = 'u_logs'
ORDER BY s.user_seeks + s.user_scans + s.user_lookups DESC;
*/
