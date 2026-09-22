-- =====================================================
-- DELETE INVOICE CASCADE
-- Elimina uma fatura e todas as suas linhas e dados relacionados
-- =====================================================

DECLARE @ndoc INT = 21          -- Tipo de documento
DECLARE @fno INT = 3            -- Número de fatura
DECLARE @ftano INT = 2026       -- Ano da fatura

DECLARE @ftStamp VARCHAR(25) = (
    SELECT ftstamp 
    FROM ft 
    WHERE ndoc = @ndoc 
    AND fno = @fno 
    AND ftano = @ftano
)

IF @ftStamp IS NOT NULL
BEGIN
    PRINT 'Iniciando deleção em cascata para fatura: ' + CAST(@ndoc AS VARCHAR) + '/' + CAST(@fno AS VARCHAR) + '/' + CAST(@ftano AS VARCHAR)
    PRINT 'FtStamp: ' + @ftStamp
    
    -- 1. Deleter FI2 (linhas adicionais)
    DECLARE @fi2Count INT = (
        SELECT COUNT(*) FROM fi2
        WHERE fi2stamp IN (
            SELECT fistamp FROM fi WHERE ftstamp = @ftStamp
        )
    )
    DELETE FROM fi2
    WHERE fi2stamp IN (
        SELECT fistamp FROM fi WHERE ftstamp = @ftStamp
    )
    PRINT 'FI2 deletadas: ' + CAST(@fi2Count AS VARCHAR)
    
    -- 2. Deleter FI (linhas da fatura)
    DECLARE @fiCount INT = (
        SELECT COUNT(*) FROM fi WHERE ftstamp = @ftStamp
    )
    DELETE FROM fi
    WHERE ftstamp = @ftStamp
    PRINT 'FI deletadas: ' + CAST(@fiCount AS VARCHAR)
    
    -- 3. Deleter FT3 (dados adicionais da fatura)
    DECLARE @ft3Count INT = (
        SELECT COUNT(*) FROM ft3 WHERE ft3stamp = @ftStamp
    )
    DELETE FROM ft3
    WHERE ft3stamp = @ftStamp
    PRINT 'FT3 deletadas: ' + CAST(@ft3Count AS VARCHAR)
    
    -- 4. Deleter FT2 (dados secundários da fatura)
    DECLARE @ft2Count INT = (
        SELECT COUNT(*) FROM ft2 WHERE ft2stamp = @ftStamp
    )
    DELETE FROM ft2
    WHERE ft2stamp = @ftStamp
    PRINT 'FT2 deletadas: ' + CAST(@ft2Count AS VARCHAR)
    
    -- 5. Deleter FT (cabeçalho da fatura)
    DELETE FROM ft
    WHERE ndoc = @ndoc 
    AND fno = @fno 
    AND ftano = @ftano
    PRINT 'FT deletada com sucesso'
    
END
ELSE
BEGIN
    PRINT 'ERRO: Fatura não encontrada (' + CAST(@ndoc AS VARCHAR) + '/' + CAST(@fno AS VARCHAR) + '/' + CAST(@ftano AS VARCHAR) + ')'
END
