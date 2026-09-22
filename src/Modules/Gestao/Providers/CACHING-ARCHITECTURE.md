# Provider Caching Architecture

## 📋 Overview

Sistema de caching configurável para otimizar acesso às configurações de providers (TFR, MPDC, JWT, etc.) armazenadas na base de dados. Implementa **Decorator Pattern** e **Strategy Pattern** para máxima flexibilidade sem código hardcoded.

## 🏗️ Architecture Patterns

### Decorator Pattern
- **CachedProviderRepository** decora **ProviderRepositoryEFCore**
- Adiciona lógica de cache transparentemente
- Sem modificar código existente do repositório
- Permite ligar/desligar cache via configuração

### Strategy Pattern
- **IProviderCachePolicy** define interface para políticas de cache
- **ConfigurableProviderCachePolicy** lê políticas do `appsettings.json`
- Cada provider pode ter TTL, prioridade e regras diferentes
- Zero hardcoding - totalmente configuration-driven

## 🔧 Components

### 1. Configuration Models
📁 `Providers.Infrastructure/Caching/Configuration/`

- **ProviderCachingOptions**: Configuração principal do sistema de cache
- **ProviderPolicySettings**: Política de cache por provider (TTL, Priority, ExcludeOperations)
- **PreWarmConfig**: Configuração para pré-aquecimento na inicialização
- **MonitoringSettings**: Configuração de logs (hits, misses, evictions)

### 2. Cache Policy
📁 `Providers.Infrastructure/Caching/`

- **IProviderCachePolicy**: Interface para políticas de cache
  - `GetTTL(providerName)`: Retorna tempo de vida no cache
  - `GetPriority(providerName)`: Retorna prioridade (High, Normal, Low)
  - `ShouldCache(providerName, operationCode)`: Decide se deve cachear
  - `GetCacheKey(providerName, operationCode, environment)`: Gera chave única

- **ConfigurableProviderCachePolicy**: Implementação que lê do IOptions
  - Lookup em `ProviderPolicies` dictionary
  - Fallback para `DefaultPolicy` se provider não configurado
  - Suporta qualquer provider sem mudanças no código

### 3. Cached Repository (Decorator)
📁 `Providers.Infrastructure/Caching/CachedProviderRepository.cs`

**Comportamento:**
- ✅ **GetByProviderAndEnvironmentAsync**: Cacheado (consulta mais frequente)
- ⚡ Cache HIT: Retorna do `IMemoryCache` sem ir à BD
- 💾 Cache MISS: Busca na BD via `_innerRepository` e armazena no cache
- 🔄 **Add/Update/Delete**: Invalidam cache automaticamente
- ➡️ **GetAll/GetPaged/Exists**: Pass-through (não cacheados)

**Cache Key Format:**
```
Provider:{providerName}:{operationCode?}:{environment?}
```
Exemplos:
- `Provider:JWT:Development`
- `Provider:TFR:GET_ALL_CONSIGNMENTS:Development`
- `Provider:MPDC:Development`

### 4. Pre-Warming Service
📁 `Providers.Infrastructure/Caching/ProviderCachePreWarmingService.cs`

IHostedService que executa no startup:
1. Lê lista `PreWarmProviders` do appsettings.json
2. Para cada provider configurado, executa `GetByProviderAndEnvironmentAsync`
3. Como usa o decorator, os dados são automaticamente cacheados
4. Logs de sucesso/falha + elapsed time
5. Garante que cache está populado antes do primeiro request

## ⚙️ Configuration (appsettings.json)

```json
{
  "Caching": {
    "Providers": {
      "Enabled": true,
      "MaxCacheSizeMB": 100,
      "EnablePreWarming": true,
      
      "DefaultPolicy": {
        "TTLMinutes": 5,
        "Priority": "Normal",
        "CacheAll": true
      },
      
      "ProviderPolicies": {
        "JWT": {
          "TTLMinutes": 2,
          "Priority": "High",
          "CacheAll": true,
          "Description": "Short TTL for security"
        },
        "TFR": {
          "TTLMinutes": 15,
          "Priority": "Normal",
          "CacheAll": false,
          "ExcludeOperations": ["AUTH"],
          "Description": "Transnet Freight Rail API"
        },
        "MPDC": {
          "TTLMinutes": 15,
          "Priority": "Normal",
          "CacheAll": false,
          "ExcludeOperations": ["AUTH"]
        }
      },
      
      "PreWarmProviders": [
        {"Provider": "JWT", "Environment": "Development"},
        {"Provider": "TFR", "OperationCode": "GET_ALL_CONSIGNMENTS", "Environment": "Development"},
        {"Provider": "MPDC", "OperationCode": "NOTIFYCONSIGMENTS", "Environment": "Development"}
      ],
      
      "Monitoring": {
        "LogCacheHits": true,
        "LogCacheMisses": true,
        "LogEvictions": false
      }
    }
  }
}
```

## 🔌 Dependency Injection

📁 `Providers.Infrastructure/DependencyInjection.cs`

### Caching Enabled Flow:
```csharp
services.Configure<ProviderCachingOptions>(...);
services.AddMemoryCache(options => options.SizeLimit = ...);
services.AddSingleton<IProviderCachePolicy, ConfigurableProviderCachePolicy>();

// Decorator Pattern
services.AddScoped<ProviderRepositoryEFCore>(); // Inner
services.AddScoped<IProviderRepository>(sp => 
    new CachedProviderRepository(
        sp.GetRequiredService<ProviderRepositoryEFCore>(),
        sp.GetRequiredService<IMemoryCache>(),
        ...));

services.AddHostedService<ProviderCachePreWarmingService>();
```

### Caching Disabled Flow:
```csharp
services.AddScoped<IProviderRepository, ProviderRepositoryEFCore>(); // Direct
```

## 📊 Flow Diagram

```
┌─────────────────────────────────────────────────────────┐
│                     Application Startup                 │
└──────────────────────┬──────────────────────────────────┘
                       │
                       ↓
         ┌─────────────────────────────┐
         │  Read appsettings.json      │
         │  - Connection Strings       │
         │  - JWT Config               │
         │  - Caching.Providers        │
         └──────────────┬──────────────┘
                       │
                       ↓
         ┌─────────────────────────────┐
         │  Connect to Database        │
         │  (using ConnectionString)   │
         └──────────────┬──────────────┘
                       │
                       ↓
         ┌─────────────────────────────┐
         │  Pre-Warming Service        │
         │  Load TFR/MPDC configs      │
         │  Populate IMemoryCache      │
         └──────────────┬──────────────┘
                       │
                       ↓
         ┌─────────────────────────────┐
         │   Application Ready         │
         └─────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│                   Runtime Request Flow                  │
└──────────────────────┬──────────────────────────────────┘
                       │
        GET /providers/TFR/AUTH/Development
                       │
                       ↓
         ┌─────────────────────────────┐
         │  ProvidersController        │
         └──────────────┬──────────────┘
                       │
                       ↓
         ┌─────────────────────────────┐
         │  GetProviderConfigQuery     │
         │  (via MediatR)              │
         └──────────────┬──────────────┘
                       │
                       ↓
         ┌─────────────────────────────┐
         │  IProviderRepository        │
         │  (CachedProviderRepository) │
         └──────────────┬──────────────┘
                       │
            ┌──────────┴──────────┐
            ↓                     ↓
    ┌──────────────┐      ┌──────────────────┐
    │  Cache HIT?  │      │   Cache MISS     │
    │  Return from │      │   Query Database │
    │  IMemoryCache│      │   Store in Cache │
    └──────────────┘      └──────────────────┘
```

## 🚀 Benefits

### Performance
- ⚡ Reduz latência em 90%+ para configurações cacheadas
- 💾 Diminui carga na base de dados
- 🔥 Pre-warming garante cache quente no startup

### Flexibility
- 🔧 Zero hardcoding - tudo configurável
- 🎯 Políticas específicas por provider
- 🔄 Fácil adicionar novos providers (só no appsettings.json)
- 🛠️ Pode desabilitar cache globalmente sem code changes

### Maintainability
- 📦 Decorator Pattern separa concerns claramente
- 🧪 Fácil de testar (pode injetar mock do cache)
- 📝 Configuração declarativa
- 🔍 Monitoring com logs configuráveis

### Scalability
- 📈 Ready para Redis L2 cache (só trocar IMemoryCache por IDistributedCache)
- 🌐 Suporta múltiplas instâncias quando migrar para Redis
- ⚖️ Cache size limit com compaction automática

## 🔐 Bootstrap Problem Solution

### O Problema:
Connection strings estão na BD → Precisamos connection string para conectar à BD → **Circular dependency**

### A Solução Híbrida:

| Tipo de Config           | Local                | Razão                            |
|--------------------------|----------------------|----------------------------------|
| Connection Strings       | ✅ appsettings.json | Bootstrap - precisa antes da BD  |
| JWT Secret               | ✅ appsettings.json | Bootstrap - auth inicial         |
| Cache Policies           | ✅ appsettings.json | Metadados - raramente mudam      |
| TFR/MPDC Endpoints       | 🗄️ BD              | Dinâmicos - Admin UI pode alterar|
| API Credentials          | 🗄️ BD (encrypted)  | Segurança - não no Git           |

## 📈 Monitoring

Logs disponíveis (configuráveis):
- **Cache Hits**: Provider encontrado no cache
- **Cache Misses**: Provider não estava no cache, buscou da BD
- **Cache Evictions**: Entrada removida do cache (expired, size limit, etc.)
- **Pre-warming Results**: Sucesso/falha na inicialização

Example Log:
```
[INF] Starting provider cache pre-warming for 3 configurations
[DBG] Cache MISS: Provider:TFR:Development - Provider: TFR, Environment: Development
[DBG] Cached Provider: TFR, Environment: Development, TTL: 00:15:00
[DBG] Cache HIT: Provider:TFR:Development - Provider: TFR, Environment: Development
[INF] Provider cache pre-warming completed in 245ms. Success: 3, Failed: 0
```

## 🧪 Testing Strategy

### Unit Tests
- Test `ConfigurableProviderCachePolicy` com diferentes configurações
- Mock IOptions para simular vários cenários
- Verificar GetTTL, GetPriority, ShouldCache com diferentes providers

### Integration Tests
- Test `CachedProviderRepository` com IMemoryCache real
- Verificar cache hit/miss behavior
- Testar invalidação de cache em Add/Update/Delete

### Load Tests
- Medir performance gain com cache vs sem cache
- Testar compaction quando size limit atingido
- Verificar comportamento sob alta concorrência

## 🔮 Future Enhancements

### L2 Distributed Cache (Redis)
```csharp
// Hybrid L1 (IMemoryCache) + L2 (Redis)
services.AddStackExchangeRedisCache(options => ...);
services.AddScoped<IProviderRepository>(sp =>
    new RedisL2CacheDecorator(
        new CachedProviderRepository(...),
        ...));
```

### Cache Warming per Environment
```json
"PreWarmProviders": [
  {"Provider": "TFR", "Environment": "Production"},
  {"Provider": "TFR", "Environment": "Development"}
]
```

### Advanced Eviction Policies
- Sliding expiration (refresh TTL on access)
- Dependency-based eviction (invalidar caches relacionados)
- Time-of-day policies (TTL diferente por horário)

## 📚 References

- [Decorator Pattern - Clean Architecture](https://github.com/jasontaylordev/CleanArchitecture)
- [IMemoryCache Best Practices](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/memory)
- [Options Pattern in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options)
