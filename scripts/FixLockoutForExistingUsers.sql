-- ============================================================================
-- 🛡️ SECURITY FIX: Habilitar Lockout para Usuários Existentes
-- ============================================================================
-- Este script corrige usuários criados ANTES da implementação do lockout
-- Executar no banco: BILENE_DESENV
-- ============================================================================

USE BILENE_DESENV;
GO

-- 1️⃣ Verificar status atual dos usuários
SELECT 
    UserName,
    Email,
    LockoutEnabled,
    LockoutEnd,
    AccessFailedCount,
    CASE 
        WHEN LockoutEnd IS NOT NULL AND LockoutEnd > GETUTCDATE() THEN 'BLOQUEADO'
        WHEN AccessFailedCount >= 3 THEN 'PRÓXIMO DE BLOQUEAR'
        ELSE 'OK'
    END AS Status
FROM AspNetUsers
ORDER BY AccessFailedCount DESC, LockoutEnabled ASC;

-- 2️⃣ Habilitar lockout para TODOS os usuários
UPDATE AspNetUsers
SET LockoutEnabled = 1
WHERE LockoutEnabled = 0;

-- 3️⃣ Resetar contadores de falhas (OPCIONAL - descomente se quiser limpar histórico)
-- UPDATE AspNetUsers SET AccessFailedCount = 0, LockoutEnd = NULL;

-- 4️⃣ Verificar resultado
SELECT 
    COUNT(*) AS TotalUsers,
    SUM(CASE WHEN LockoutEnabled = 1 THEN 1 ELSE 0 END) AS UsersWithLockout,
    SUM(CASE WHEN LockoutEnabled = 0 THEN 1 ELSE 0 END) AS UsersWithoutLockout,
    SUM(CASE WHEN LockoutEnd IS NOT NULL AND LockoutEnd > GETUTCDATE() THEN 1 ELSE 0 END) AS CurrentlyLocked
FROM AspNetUsers;

-- 5️⃣ Ver usuários bloqueados no momento
SELECT 
    UserName,
    Email,
    AccessFailedCount,
    LockoutEnd,
    DATEDIFF(MINUTE, GETUTCDATE(), LockoutEnd) AS MinutosRestantes
FROM AspNetUsers
WHERE LockoutEnd IS NOT NULL AND LockoutEnd > GETUTCDATE();

-- 6️⃣ DESBLOQUEAR usuário específico (se necessário para testes)
-- UPDATE AspNetUsers 
-- SET AccessFailedCount = 0, LockoutEnd = NULL 
-- WHERE UserName = 'SEU_USERNAME_AQUI';

GO
