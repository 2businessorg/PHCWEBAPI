-- =============================================
-- Script: Criação de tabelas u_provider e u_providervalues
-- Descrição: Sistema de gestão de configurações de APIs externas
-- Data: 2026-02-17
-- =============================================

-- =============================================
-- Tabela: u_provider (Cabeçalho)
-- Descrição: Armazena informações dos provedores de API e seus ambientes
-- =============================================
CREATE TABLE u_provider (
    -- Chave primária (estilo PHC stamp)
    u_providerstamp VARCHAR(50) NOT NULL DEFAULT '',
    
    -- Identificação do provider
    codigo INT NOT NULL DEFAULT 0,
    provedor VARCHAR(100) NOT NULL DEFAULT '',
    environment VARCHAR(50) NOT NULL DEFAULT 'Development',
    descricao NVARCHAR(255) NOT NULL DEFAULT '',
    
    -- Estado
    ativo BIT NOT NULL DEFAULT 1,
    
    -- Auditoria (criação)
    ousrinis VARCHAR(50) NOT NULL DEFAULT '',
    ousrdata DATETIME NOT NULL DEFAULT '19000101',
    ousrhora VARCHAR(8) NOT NULL DEFAULT '00:00:00',
    
    -- Auditoria (última alteração)
    usrinis VARCHAR(50) NOT NULL DEFAULT '',
    usrdata DATETIME NOT NULL DEFAULT '19000101',
    usrhora VARCHAR(8) NOT NULL DEFAULT '00:00:00',
    
    -- Constraints
    CONSTRAINT PK_u_provider PRIMARY KEY (u_providerstamp),
    CONSTRAINT CK_u_provider_environment CHECK (environment IN ('Development', 'Staging', 'Production')),
    CONSTRAINT UQ_u_provider_provedor_env UNIQUE (provedor, environment)
);
GO

-- Índices para otimização de queries
CREATE NONCLUSTERED INDEX IX_u_provider_codigo_env 
    ON u_provider(codigo, environment) 
    INCLUDE (provedor, ativo);
GO

CREATE NONCLUSTERED INDEX IX_u_provider_provedor 
    ON u_provider(provedor) 
    INCLUDE (environment, ativo);
GO

-- =============================================
-- Tabela: u_providervalues (Linhas)
-- Descrição: Armazena propriedades key-value para cada provider/operação
-- =============================================
CREATE TABLE u_providervalues (
    -- Chave primária (estilo PHC stamp)
    u_providervaluesstamp VARCHAR(50) NOT NULL DEFAULT '',
    
    -- Foreign Key para o cabeçalho
    u_providerstamp VARCHAR(50) NOT NULL DEFAULT '',
    
    -- Identificação da operação e propriedade
    operationcode VARCHAR(100) NOT NULL DEFAULT '',
    chave VARCHAR(100) NOT NULL DEFAULT '',
    valor NVARCHAR(MAX) NOT NULL DEFAULT '',
    
    -- Segurança
    encriptado BIT NOT NULL DEFAULT 0,
    
    -- Ordem (opcional, para ordenação de exibição)
    ordem INT NOT NULL DEFAULT 0,
    
    -- Estado
    ativo BIT NOT NULL DEFAULT 1,
    
    -- Auditoria (criação)
    ousrinis VARCHAR(50) NOT NULL DEFAULT '',
    ousrdata DATETIME NOT NULL DEFAULT '19000101',
    ousrhora VARCHAR(8) NOT NULL DEFAULT '00:00:00',
    
    -- Auditoria (última alteração)
    usrinis VARCHAR(50) NOT NULL DEFAULT '',
    usrdata DATETIME NOT NULL DEFAULT '19000101',
    usrhora VARCHAR(8) NOT NULL DEFAULT '00:00:00',
    
    -- Constraints
    CONSTRAINT PK_u_providervalues PRIMARY KEY (u_providervaluesstamp),
    CONSTRAINT FK_u_providervalues_provider 
        FOREIGN KEY (u_providerstamp) 
        REFERENCES u_provider(u_providerstamp)
        ON DELETE CASCADE,
    CONSTRAINT UQ_u_providervalues_provider_op_key 
        UNIQUE (u_providerstamp, operationcode, chave)
);
GO

-- Índices para otimização de queries
CREATE NONCLUSTERED INDEX IX_u_providervalues_provider 
    ON u_providervalues(u_providerstamp, operationcode) 
    INCLUDE (chave, valor, encriptado, ativo);
GO

CREATE NONCLUSTERED INDEX IX_u_providervalues_operation 
    ON u_providervalues(operationcode) 
    INCLUDE (chave, valor);
GO

-- =============================================
-- Comentários nas tabelas (SQL Server Extended Properties)
-- =============================================
EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Tabela de cabeçalho para configurações de provedores de API externos', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE',  @level1name = N'u_provider';
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Tabela de linhas com propriedades key-value para cada provider/operação', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE',  @level1name = N'u_providervalues';
GO

-- =============================================
-- Script de verificação
-- =============================================
PRINT '✅ Tabelas criadas com sucesso!';
PRINT '';
PRINT 'Tabelas criadas:';
PRINT '  - u_provider (Cabeçalho)';
PRINT '  - u_providervalues (Linhas)';
PRINT '';
PRINT 'Índices criados: 4';
PRINT 'Constraints: 5';
GO
