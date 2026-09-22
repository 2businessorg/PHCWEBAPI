# Guia de Testes Unitários - PHCAPI

## 📋 Estrutura Atual

O projeto já tem:
- **Framework:** xUnit 2.9.2
- **Mocking:** Moq 4.20.72
- **Geração de dados:** AutoFixture 4.18.1 + Bogus 35.6.1
- **Assertions:** FluentAssertions 6.12.2
- **Cobertura:** coverlet.collector 6.0.2

Projeto de exemplo: `tests/Parameters.Application.Tests/`

---

## 🚀 Passo 1: Criar Projeto de Testes para Dossiers

### 1.1 Criar a pasta e arquivo `.csproj`

```bash
mkdir tests\Dossiers.Application.Tests
```

### 1.2 Criar `Dossiers.Application.Tests.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
    <RollForward>LatestMajor</RollForward>
    <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
    <PreserveCompilationContext>true</PreserveCompilationContext>
  </PropertyGroup>

  <ItemGroup>
    <!-- Test Framework -->
    <PackageReference Include="AutoFixture" Version="4.18.1" />
    <PackageReference Include="AutoFixture.AutoMoq" Version="4.18.1" />
    <PackageReference Include="AutoFixture.Xunit2" Version="4.18.1" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.0.1" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    
    <!-- Test Coverage -->
    <PackageReference Include="coverlet.collector" Version="6.0.2">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    
    <!-- Mocking & Assertions -->
    <PackageReference Include="Moq" Version="4.20.72" />
    <PackageReference Include="FluentAssertions" Version="6.12.2" />
    <PackageReference Include="Bogus" Version="35.6.1" />
  </ItemGroup>

  <ItemGroup>
    <!-- Referências aos projetos sendo testados -->
    <ProjectReference Include="..\..\src\Modules\Dossiers\Dossiers.Application\Dossiers.Application.csproj" />
    <ProjectReference Include="..\..\src\Modules\Dossiers\Dossiers.Domain\Dossiers.Domain.csproj" />
  </ItemGroup>

</Project>
```

---

## 📝 Passo 2: Estrutura de Pastas de Testes

```
tests/
├── Dossiers.Application.Tests/
│   ├── Features/
│   │   └── CreateDossier/
│   │       └── CreateDossierCommandHandlerTests.cs
│   ├── Fixtures/
│   │   ├── DossierFixture.cs
│   │   └── ClientFixture.cs
│   └── Dossiers.Application.Tests.csproj
```

---

## 🧪 Passo 3: Criar Fixture (Dados de Teste)

**Arquivo:** `Fixtures/DossierFixture.cs`

```csharp
using Bogus;
using Dossiers.Application.DTOs;
using Dossiers.Domain.Entities;

namespace Dossiers.Application.Tests.Fixtures;

public class DossierFixture
{
    private readonly Faker _faker = new("pt_PT");

    public CreateDossierDTO CreateValidDossierDto()
    {
        return new CreateDossierDTO
        {
            Ndos = 1,
            Boano = 2026,
            No = 58,
            Estab = 0,
            Data = DateTime.Now.Date,
            Moeda = "MT",
            Linhas = new List<CreateDossierLineDTO>
            {
                new()
                {
                    Referencia = "REF-001",
                    Quantidade = 2,
                    PrecoUnitario = 5000.00m,
                    TabIva = 1
                }
            }
        };
    }

    public Bo CreateValidBo()
    {
        return new Bo
        {
            Bostamp = Guid.NewGuid().ToString("N").Substring(0, 25),
            Ndos = 1,
            Nmdos = "FATURA",
            Obrano = _faker.Random.Int(1, 999),
            Boano = 2026,
            No = 58,
            Estab = 0,
            Nome = _faker.Company.CompanyName(),
            Dataobra = DateTime.Now.Date,
            Dataopen = DateTime.Now.Date,
            Moeda = "MT",
            Totaldeb = _faker.Random.Decimal(100, 10000)
        };
    }

    public DossierClient CreateValidClient()
    {
        return new DossierClient
        {
            Clstamp = Guid.NewGuid().ToString("N").Substring(0, 25),
            No = 58,
            Estab = 0,
            Nome = _faker.Company.CompanyName(),
            Morada = _faker.Address.StreetAddress(),
            Local = _faker.Address.City(),
            Codpost = _faker.Address.ZipCode(),
            Preco = 1,
            Telefone = _faker.Phone.PhoneNumber(),
            Contacto = _faker.Name.FullName(),
            Email = _faker.Internet.Email()
        };
    }

    public St CreateValidStock()
    {
        return new St
        {
            Ststamp = Guid.NewGuid().ToString("N").Substring(0, 25),
            Ref = "REF-001",
            Design = "Produto Teste",
            Pv1 = 5000.00m,
            Pv2 = 4800.00m,
            Pv3 = 4600.00m,
            Pv4 = 4400.00m,
            Pv5 = 4200.00m,
            Tabiva = 1,
            Familia = "FAM-001",
            Pcusto = 2500.00m,
            Pcpond = 2400.00m,
            Pcult = 2450.00m,
            Cpoc = 1
        };
    }
}
```

---

## ✅ Passo 4: Teste Unitário Simples

**Arquivo:** `Features/CreateDossier/CreateDossierCommandHandlerTests.cs`

```csharp
using AutoFixture;
using AutoFixture.AutoMoq;
using Dossiers.Application.Features.CreateDossier;
using Dossiers.Application.Tests.Fixtures;
using Dossiers.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace Dossiers.Application.Tests.Features.CreateDossier;

public class CreateDossierCommandHandlerTests
{
    private readonly DossierFixture _fixture = new();
    private readonly IFixture _autoFixture = new Fixture().Customize(new AutoMoqCustomization());

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateDossier()
    {
        // Arrange
        var command = new CreateDossierCommand(_fixture.CreateValidDossierDto());
        
        var mockDossierRepository = new Mock<IDossierRepository>();
        var mockClientRepository = new Mock<IDossierClientRepository>();
        var mockCl2Repository = new Mock<ICl2Repository>();
        var mockStockRepository = new Mock<IStockRepository>();
        var mockMoedaRepository = new Mock<IMoedaRepository>();
        var mockTaxasIvaRepository = new Mock<ITaxasIvaRepository>();
        var mockTipoDossierRepository = new Mock<ITipoDossierRepository>();

        // Setup - Valores retornados pelos mocks
        var client = _fixture.CreateValidClient();
        var stock = _fixture.CreateValidStock();
        var tipo = new Ts { Ndos = 1, Nmdos = "FATURA", Qprecocusto = 3 };

        mockTipoDossierRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tipo);

        mockClientRepository
            .Setup(x => x.ExistsByNoEstabAsync(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        mockClientRepository
            .Setup(x => x.GetByNoEstabAsync(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        mockCl2Repository
            .Setup(x => x.GetByStampAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DossierClient { Codpais = "MZ", Descpais = "Moçambique" });

        mockMoedaRepository
            .Setup(x => x.GetDefaultAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("MT");

        mockMoedaRepository
            .Setup(x => x.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        mockStockRepository
            .Setup(x => x.GetByRefAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stock);

        mockDossierRepository
            .Setup(x => x.GetNextObranoAsync(It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(123);

        mockDossierRepository
            .Setup(x => x.ExistsByKeyAsync(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        mockTaxasIvaRepository
            .Setup(x => x.GetByCodeAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Taxasiva { Codigo = 1, Taxa = 16.00m });

        var handler = new CreateDossierCommandHandler(
            mockDossierRepository.Object,
            mockClientRepository.Object,
            mockCl2Repository.Object,
            mockStockRepository.Object,
            mockMoedaRepository.Object,
            mockTaxasIvaRepository.Object,
            mockTipoDossierRepository.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Ndos.Should().Be(1);
        result.Nome.Should().Be(client.Nome);
        result.Moeda.Should().Be("MT");
    }

    [Fact]
    public async Task Handle_ClientNotFound_ShouldThrowException()
    {
        // Arrange
        var command = new CreateDossierCommand(_fixture.CreateValidDossierDto());
        
        var mockDossierRepository = new Mock<IDossierRepository>();
        var mockClientRepository = new Mock<IDossierClientRepository>();
        var mockCl2Repository = new Mock<ICl2Repository>();
        var mockStockRepository = new Mock<IStockRepository>();
        var mockMoedaRepository = new Mock<IMoedaRepository>();
        var mockTaxasIvaRepository = new Mock<ITaxasIvaRepository>();
        var mockTipoDossierRepository = new Mock<ITipoDossierRepository>();

        mockTipoDossierRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Ts { Ndos = 1, Nmdos = "FATURA" });

        mockClientRepository
            .Setup(x => x.ExistsByNoEstabAsync(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new CreateDossierCommandHandler(
            mockDossierRepository.Object,
            mockClientRepository.Object,
            mockCl2Repository.Object,
            mockStockRepository.Object,
            mockMoedaRepository.Object,
            mockTaxasIvaRepository.Object,
            mockTipoDossierRepository.Object
        );

        // Act & Assert
        await Assert.ThrowsAsync<DossiersModuleException>(() => 
            handler.Handle(command, CancellationToken.None));
    }
}
```

---

## 🏃 Passo 5: Executar Testes

### Via Terminal
```bash
# Executar todos os testes
dotnet test

# Executar apenas Dossiers
dotnet test tests/Dossiers.Application.Tests/Dossiers.Application.Tests.csproj

# Com cobertura de código
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover
```

### Via Visual Studio
1. **Test Explorer** → `Ctrl + E, T`
2. Procure por `CreateDossierCommandHandlerTests`
3. Clique em **Run All** ou teste específico

---

## 📊 Padrão AAA (Arrange-Act-Assert)

```csharp
[Fact]
public void ExemploTeste()
{
    // ✅ ARRANGE: Preparar dados
    var entrada = new Entrada { Valor = 10 };
    
    // ✅ ACT: Executar a ação
    var resultado = Funcao(entrada);
    
    // ✅ ASSERT: Validar resultado
    resultado.Should().Be(10);
}
```

---

## 🎯 Boas Práticas

1. **Um conceito por teste**
   ```csharp
   [Fact]
   public void ValidarNomeDoCliente() { }  // ✅ Bom
   
   [Fact]
   public void TestDossier() { }  // ❌ Vago
   ```

2. **Nomes descritivos**
   ```csharp
   // ✅ Claro
   public async Task Handle_WithValidData_ShouldCreateDossier()
   
   // ❌ Confuso
   public async Task Test1()
   ```

3. **Arrange → Act → Assert**
   - Mantém testes legíveis e organizados

4. **Use fixtures para dados reutilizáveis**
   - Evita repetição
   - Facilita manutenção

5. **Mock apenas dependências externas**
   - Não mock o que está sendo testado
   - Mock BD, API, services

---

## 🔗 Próximos Passos

1. ✅ Criar `Dossiers.Application.Tests.csproj`
2. ✅ Criar fixtures de dados
3. ✅ Escrever primeiros testes do `CreateDossierCommandHandler`
4. ⏳ Expandir para validações, regras de negócio
5. ⏳ Testes de integração (com BD real)
6. ⏳ Medir cobertura de código

---

## 📚 Recursos Úteis

- [xUnit Documentation](https://xunit.net/)
- [Moq GitHub](https://github.com/moq/moq4)
- [FluentAssertions](https://fluentassertions.com/)
- [AutoFixture](https://github.com/AutoFixture/AutoFixture)
