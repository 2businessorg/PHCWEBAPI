using Dossiers.Application.Errors;
using Dossiers.Application.Features.CreateDossier;
using Dossiers.Application.Tests.Fixtures;
using Dossiers.Domain.Entities;
using Dossiers.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace Dossiers.Application.Tests.Features.CreateDossier;

/// <summary>
/// Testes unitários para o CreateDossierCommandHandler
/// </summary>
public class CreateDossierCommandHandlerTests
{
    private readonly DossierFixture _fixture = new();
    /*
    #region Tests - Sucesso

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateDossier()
    {
        // Arrange
        var dossierDto = _fixture.CreateValidDossierDto();
        var command = new CreateDossierCommand(dossierDto);

        var client = _fixture.CreateValidClient();
        var stock = _fixture.CreateValidStock();
        var tipo = _fixture.CreateValidTipoDossier();
        var taxasIva = _fixture.CreateValidTaxasIva();

        var mockDossierRepository = new Mock<IDossierRepository>();
        var mockClientRepository = new Mock<IDossierClientRepository>();
        var mockSupplierRepository = new Mock<ISupplierRepository>();
        var mockEntityRepository = new Mock<IEntityRepository>();
        var mockContactRepository = new Mock<IContactRepository>();
        var mockCl2Repository = new Mock<ICl2Repository>();
        var mockStockRepository = new Mock<IStockRepository>();
        var mockMoedaRepository = new Mock<IMoedaRepository>();
        var mockTaxasIvaRepository = new Mock<ITaxasIvaRepository>();
        var mockTipoDossierRepository = new Mock<ITipoDossierRepository>();

        // Setup dos mocks
        SetupValidMocks(
            mockTipoDossierRepository, mockClientRepository, mockCl2Repository,
            mockMoedaRepository, mockStockRepository, mockDossierRepository,
            mockTaxasIvaRepository, tipo, client, stock, taxasIva);

        var handler = new CreateDossierCommandHandler(
            mockDossierRepository.Object,
            mockClientRepository.Object,
            mockSupplierRepository.Object,
            mockEntityRepository.Object,
            mockContactRepository.Object,
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
        result.Linhas.Should().HaveCount(1);
        result.Linhas.First().Ref.Should().Be("REF-001");
    }

    [Fact]
    public async Task Handle_WithMultipleLines_ShouldCreateDossierWithAllLines()
    {
        // Arrange
        var dossierDto = _fixture.CreateDossierWithMultipleLines();
        var command = new CreateDossierCommand(dossierDto);

        var client = _fixture.CreateValidClient();
        var stock = _fixture.CreateValidStock();
        var tipo = _fixture.CreateValidTipoDossier();
        var taxasIva = _fixture.CreateValidTaxasIva();

        var mockDossierRepository = new Mock<IDossierRepository>();
        var mockClientRepository = new Mock<IDossierClientRepository>();
        var mockSupplierRepository = new Mock<ISupplierRepository>();
        var mockEntityRepository = new Mock<IEntityRepository>();
        var mockContactRepository = new Mock<IContactRepository>();
        var mockCl2Repository = new Mock<ICl2Repository>();
        var mockStockRepository = new Mock<IStockRepository>();
        var mockMoedaRepository = new Mock<IMoedaRepository>();
        var mockTaxasIvaRepository = new Mock<ITaxasIvaRepository>();
        var mockTipoDossierRepository = new Mock<ITipoDossierRepository>();

        SetupValidMocks(
            mockTipoDossierRepository, mockClientRepository, mockCl2Repository,
            mockMoedaRepository, mockStockRepository, mockDossierRepository,
            mockTaxasIvaRepository, tipo, client, stock, taxasIva);

        var handler = new CreateDossierCommandHandler(
            mockDossierRepository.Object,
            mockClientRepository.Object,
            mockSupplierRepository.Object,
            mockEntityRepository.Object,
            mockContactRepository.Object,
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
        result.Linhas.Should().HaveCount(2);
        result.Linhas.Should().Contain(l => l.Ref == "REF-001");
        result.Linhas.Should().Contain(l => l.Ref == "REF-002");
    }

    [Fact]
    public async Task Handle_WithMinimalData_ShouldResolveDefaults()
    {
        // Arrange
        var dossierDto = _fixture.CreateDossierWithMinimalData();
        var command = new CreateDossierCommand(dossierDto);

        var client = _fixture.CreateValidClient();
        var stock = _fixture.CreateValidStock();
        var tipo = _fixture.CreateValidTipoDossier();
        var taxasIva = _fixture.CreateValidTaxasIva();

        var mockDossierRepository = new Mock<IDossierRepository>();
        var mockClientRepository = new Mock<IDossierClientRepository>();
        var mockSupplierRepository = new Mock<ISupplierRepository>();
        var mockEntityRepository = new Mock<IEntityRepository>();
        var mockContactRepository = new Mock<IContactRepository>();
        var mockCl2Repository = new Mock<ICl2Repository>();
        var mockStockRepository = new Mock<IStockRepository>();
        var mockMoedaRepository = new Mock<IMoedaRepository>();
        var mockTaxasIvaRepository = new Mock<ITaxasIvaRepository>();
        var mockTipoDossierRepository = new Mock<ITipoDossierRepository>();

        SetupValidMocks(
            mockTipoDossierRepository, mockClientRepository, mockCl2Repository,
            mockMoedaRepository, mockStockRepository, mockDossierRepository,
            mockTaxasIvaRepository, tipo, client, stock, taxasIva);

        var handler = new CreateDossierCommandHandler(
            mockDossierRepository.Object,
            mockClientRepository.Object,
            mockSupplierRepository.Object,
            mockEntityRepository.Object,
            mockContactRepository.Object,
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
        // Nome deve ser resolvido do cliente
        result.Nome.Should().Be(client.Nome);
        // Moeda deve ser a padrão
        result.Moeda.Should().Be("MT");
    }

    #endregion

    #region Tests - Falhas Esperadas

    [Fact]
    public async Task Handle_InvalidTipoDossier_ShouldThrowException()
    {
        // Arrange
        var dossierDto = _fixture.CreateValidDossierDto();
        var command = new CreateDossierCommand(dossierDto);

        var mockDossierRepository = new Mock<IDossierRepository>();
        var mockClientRepository = new Mock<IDossierClientRepository>();
        var mockSupplierRepository = new Mock<ISupplierRepository>();
        var mockEntityRepository = new Mock<IEntityRepository>();
        var mockContactRepository = new Mock<IContactRepository>();
        var mockCl2Repository = new Mock<ICl2Repository>();
        var mockStockRepository = new Mock<IStockRepository>();
        var mockMoedaRepository = new Mock<IMoedaRepository>();
        var mockTaxasIvaRepository = new Mock<ITaxasIvaRepository>();
        var mockTipoDossierRepository = new Mock<ITipoDossierRepository>();

        mockTipoDossierRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Ts?)null);

        var handler = new CreateDossierCommandHandler(
            mockDossierRepository.Object,
            mockClientRepository.Object,
            mockSupplierRepository.Object,
            mockEntityRepository.Object,
            mockContactRepository.Object,
            mockCl2Repository.Object,
            mockStockRepository.Object,
            mockMoedaRepository.Object,
            mockTaxasIvaRepository.Object,
            mockTipoDossierRepository.Object
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DossiersModuleException>(
            () => handler.Handle(command, CancellationToken.None));

        exception.Code.Should().Be(DossiersErrorCatalog.InvalidTipoDossier.Code);
    }

    [Fact]
    public async Task Handle_ClientNotFound_ShouldThrowException()
    {
        // Arrange
        var dossierDto = _fixture.CreateValidDossierDto();
        var command = new CreateDossierCommand(dossierDto);

        var tipo = _fixture.CreateValidTipoDossier();

        var mockDossierRepository = new Mock<IDossierRepository>();
        var mockClientRepository = new Mock<IDossierClientRepository>();
        var mockSupplierRepository = new Mock<ISupplierRepository>();
        var mockEntityRepository = new Mock<IEntityRepository>();
        var mockContactRepository = new Mock<IContactRepository>();
        var mockCl2Repository = new Mock<ICl2Repository>();
        var mockStockRepository = new Mock<IStockRepository>();
        var mockMoedaRepository = new Mock<IMoedaRepository>();
        var mockTaxasIvaRepository = new Mock<ITaxasIvaRepository>();
        var mockTipoDossierRepository = new Mock<ITipoDossierRepository>();

        mockTipoDossierRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tipo);

        var handler = new CreateDossierCommandHandler(
            mockDossierRepository.Object,
            mockClientRepository.Object,
            mockSupplierRepository.Object,
            mockEntityRepository.Object,
            mockContactRepository.Object,
            mockCl2Repository.Object,
            mockStockRepository.Object,
            mockMoedaRepository.Object,
            mockTaxasIvaRepository.Object,
            mockTipoDossierRepository.Object
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DossiersModuleException>(
            () => handler.Handle(command, CancellationToken.None));

        exception.Code.Should().Be(DossiersErrorCatalog.ClientNotFound.Code);
    }

    [Fact]
    public async Task Handle_InvalidCurrency_ShouldThrowException()
    {
        // Arrange
        var dossierDto = _fixture.CreateValidDossierDto();
        dossierDto.Moeda = "INVALID";
        var command = new CreateDossierCommand(dossierDto);

        var client = _fixture.CreateValidClient();
        var tipo = _fixture.CreateValidTipoDossier();

        var mockDossierRepository = new Mock<IDossierRepository>();
        var mockClientRepository = new Mock<IDossierClientRepository>();
        var mockSupplierRepository = new Mock<ISupplierRepository>();
        var mockEntityRepository = new Mock<IEntityRepository>();
        var mockContactRepository = new Mock<IContactRepository>();
        var mockCl2Repository = new Mock<ICl2Repository>();
        var mockStockRepository = new Mock<IStockRepository>();
        var mockMoedaRepository = new Mock<IMoedaRepository>();
        var mockTaxasIvaRepository = new Mock<ITaxasIvaRepository>();
        var mockTipoDossierRepository = new Mock<ITipoDossierRepository>();

        mockTipoDossierRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tipo);

        mockCl2Repository
            .Setup(x => x.GetByStampAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Cl2());

        mockMoedaRepository
            .Setup(x => x.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new CreateDossierCommandHandler(
            mockDossierRepository.Object,
            mockClientRepository.Object,
            mockSupplierRepository.Object,
            mockEntityRepository.Object,
            mockContactRepository.Object,
            mockCl2Repository.Object,
            mockStockRepository.Object,
            mockMoedaRepository.Object,
            mockTaxasIvaRepository.Object,
            mockTipoDossierRepository.Object
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DossiersModuleException>(
            () => handler.Handle(command, CancellationToken.None));

        exception.Code.Should().Be(DossiersErrorCatalog.InvalidCurrency.Code);

    }

    [Fact]
    public async Task Handle_ReferenceNotFound_ShouldThrowException()
    {
        // Arrange
        var dossierDto = _fixture.CreateValidDossierDto();
        var command = new CreateDossierCommand(dossierDto);

        var client = _fixture.CreateValidClient();
        var tipo = _fixture.CreateValidTipoDossier();

        var mockDossierRepository = new Mock<IDossierRepository>();
        var mockClientRepository = new Mock<IDossierClientRepository>();
        var mockSupplierRepository = new Mock<ISupplierRepository>();
        var mockEntityRepository = new Mock<IEntityRepository>();
        var mockContactRepository = new Mock<IContactRepository>();
        var mockCl2Repository = new Mock<ICl2Repository>();
        var mockStockRepository = new Mock<IStockRepository>();
        var mockMoedaRepository = new Mock<IMoedaRepository>();
        var mockTaxasIvaRepository = new Mock<ITaxasIvaRepository>();
        var mockTipoDossierRepository = new Mock<ITipoDossierRepository>();

        mockTipoDossierRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tipo);

        mockCl2Repository
            .Setup(x => x.GetByStampAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Cl2());

        mockMoedaRepository
            .Setup(x => x.GetDefaultAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("MT");

        mockMoedaRepository
            .Setup(x => x.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        mockStockRepository
            .Setup(x => x.GetByRefAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((St?)null);

        var handler = new CreateDossierCommandHandler(
            mockDossierRepository.Object,
            mockClientRepository.Object,
            mockSupplierRepository.Object,
            mockEntityRepository.Object,
            mockContactRepository.Object,
            mockCl2Repository.Object,
            mockStockRepository.Object,
            mockMoedaRepository.Object,
            mockTaxasIvaRepository.Object,
            mockTipoDossierRepository.Object
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DossiersModuleException>(
            () => handler.Handle(command, CancellationToken.None));

        exception.Code.Should().Be(DossiersErrorCatalog.ReferenceNotFound.Code);
    }

    [Fact]
    public async Task Handle_DossierAlreadyExists_ShouldThrowException()
    {
        // Arrange
        var dossierDto = _fixture.CreateValidDossierDto();
        var command = new CreateDossierCommand(dossierDto);

        var client = _fixture.CreateValidClient();
        var stock = _fixture.CreateValidStock();
        var tipo = _fixture.CreateValidTipoDossier();

        var mockDossierRepository = new Mock<IDossierRepository>();
        var mockClientRepository = new Mock<IDossierClientRepository>();
        var mockSupplierRepository = new Mock<ISupplierRepository>();
        var mockEntityRepository = new Mock<IEntityRepository>();
        var mockContactRepository = new Mock<IContactRepository>();
        var mockCl2Repository = new Mock<ICl2Repository>();
        var mockStockRepository = new Mock<IStockRepository>();
        var mockMoedaRepository = new Mock<IMoedaRepository>();
        var mockTaxasIvaRepository = new Mock<ITaxasIvaRepository>();
        var mockTipoDossierRepository = new Mock<ITipoDossierRepository>();

        mockTipoDossierRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tipo);

        mockCl2Repository
            .Setup(x => x.GetByStampAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Cl2());

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

        // Simula que o dossier já existe
        mockDossierRepository
            .Setup(x => x.ExistsByKeyAsync(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new CreateDossierCommandHandler(
            mockDossierRepository.Object,
            mockClientRepository.Object,
            mockSupplierRepository.Object,
            mockEntityRepository.Object,
            mockContactRepository.Object,
            mockCl2Repository.Object,
            mockStockRepository.Object,
            mockMoedaRepository.Object,
            mockTaxasIvaRepository.Object,
            mockTipoDossierRepository.Object
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DossiersModuleException>(
            () => handler.Handle(command, CancellationToken.None));

        exception.Code.Should().Be(DossiersErrorCatalog.DossierAlreadyExistsByKey.Code);
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Setup de mocks para cenários válidos
    /// </summary>
    private static void SetupValidMocks(
        Mock<ITipoDossierRepository> mockTipoDossier,
        Mock<IDossierClientRepository> mockClient,
        Mock<ICl2Repository> mockCl2,
        Mock<IMoedaRepository> mockMoeda,
        Mock<IStockRepository> mockStock,
        Mock<IDossierRepository> mockDossier,
        Mock<ITaxasIvaRepository> mockTaxasIva,
        Ts tipo,
        DossierClient client,
        St stock,
        Taxasiva taxasiva)
    {
        mockTipoDossier
            .Setup(x => x.GetByIdAsync(It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tipo);

        mockCl2
            .Setup(x => x.GetByStampAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Cl2 { Codpais = "MZ", Descpais = "Moçambique" });

        mockMoeda
            .Setup(x => x.GetDefaultAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("MT");

        mockMoeda
            .Setup(x => x.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        mockStock
            .Setup(x => x.GetByRefAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stock);

        mockDossier
            .Setup(x => x.GetNextObranoAsync(It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(123);

        mockDossier
            .Setup(x => x.ExistsByKeyAsync(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        mockTaxasIva
            .Setup(x => x.GetByCodeAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(taxasiva);
    }

    #endregion

    */
}
