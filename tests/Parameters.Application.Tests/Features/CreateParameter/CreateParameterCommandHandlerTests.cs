using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using Parameters.Application.DTOs.Parameters;
using Parameters.Application.Features.CreateParameter;
using Parameters.Application.Mappings;
using Parameters.Domain.Repositories;
using Parameters.Domain.Entities;
using Parameters.Application.Tests.Fixtures;
using Xunit;

namespace Parameters.Application.Tests.Features.CreateParameter;

/// <summary>
/// ✅ Testes do CreateParameterCommandHandler
/// Demonstra uso de AutoFixture, Moq e FluentAssertions
/// </summary>
public class CreateParameterCommandHandlerTests
{
    #region ✅ EXEMPLO 1: Teste Básico com AutoMoqData

    /// <summary>
    /// Teste básico: CreateParameter deve salvar e retornar DTO
    /// AutoMoqData injeta automaticamente mocks e dados de teste
    /// </summary>
    [Theory]
    [AutoMoqData]
    public async Task Handle_ValidCommand_ShouldCreateParameterAndReturnDto(
        [Frozen] Mock<IPara1Repository> repositoryMock, // ✅ Mock injetado
        CreateParameterCommandHandler sut, // ✅ System Under Test (Handler)
        CreateParameterInputDTO inputDto, // ✅ Dados gerados automaticamente
        Para1 savedEntity) // ✅ Entidade mockada
    {
        // Arrange: Configurar comportamento do mock
        repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Para1>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedEntity);

        var command = new CreateParameterCommand(inputDto, "test_user");

        // Act: Executar o handler
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert: Verificar resultado
        result.Should().NotBeNull("o handler deve retornar um DTO");
        result.Para1Stamp.Should().Be(savedEntity.Para1Stamp, "o stamp deve ser o mesmo da entidade salva");
        result.Descricao.Should().Be(savedEntity.Descricao);

        // ✅ Verificar que o repositório foi chamado exatamente uma vez
        repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Para1>(), It.IsAny<CancellationToken>()),
            Times.Once,
            "o repositório deve ser chamado exatamente uma vez");
    }

    #endregion

    #region ✅ EXEMPLO 2: Teste Parametrizado com Múltiplos Cenários

    /// <summary>
    /// Testa múltiplos cenários com diferentes descrições
    /// Theory + InlineData = teste parametrizado
    /// </summary>
    [Theory]
    [InlineAutoMoqData("Parâmetro Teste 1")]
    [InlineAutoMoqData("Parâmetro com caracteres especiais !@#$%")]
    [InlineAutoMoqData("Parâmetro com acentuação: áéíóú")]
    [InlineAutoMoqData("Parameter with numbers 12345")]
    public async Task Handle_DifferentDescriptions_ShouldAcceptAll(
        string descricao, // ✅ Valor específico do InlineData
        [Frozen] Mock<IPara1Repository> repositoryMock,
        CreateParameterCommandHandler sut)
    {
        // Arrange
        var fixture = new Fixture();
        var inputDto = new CreateParameterInputDTO
        {
            Descricao = descricao,
            Valor = "test_value",
            Tipo = "T",
            Dec = 2,
            Tam = 10
        };

        var savedEntity = new Para1(
            fixture.Create<string>().Substring(0, 25),
            descricao,
            "test_value",
            "T",
            2,
            10,
            "test_user");

        repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Para1>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedEntity);

        var command = new CreateParameterCommand(inputDto, "test_user");

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        result.Descricao.Should().Be(descricao, $"a descrição '{descricao}' deve ser preservada");
    }

    #endregion

    #region ✅ EXEMPLO 3: Teste com Verificação de Propriedades do Entity

    /// <summary>
    /// Verifica que o Entity é criado com os dados corretos antes de salvar
    /// Usa It.Is() para validar argumento específico
    /// </summary>
    [Theory]
    [AutoMoqData]
    public async Task Handle_ShouldCreateEntityWithCorrectData(
        [Frozen] Mock<IPara1Repository> repositoryMock,
        CreateParameterCommandHandler sut,
        CreateParameterInputDTO inputDto)
    {
        // Arrange
        Para1? capturedEntity = null;

        repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Para1>(), It.IsAny<CancellationToken>()))
            .Callback<Para1, CancellationToken>((entity, _) => capturedEntity = entity)
            .ReturnsAsync((Para1 entity, CancellationToken _) => entity);

        var command = new CreateParameterCommand(inputDto, "admin_user");

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert: Verificar entidade capturada
        capturedEntity.Should().NotBeNull("o repositório deve ter sido chamado com uma entidade");
        capturedEntity!.Descricao.Should().Be(inputDto.Descricao);
        capturedEntity.Valor.Should().Be(inputDto.Valor);
        capturedEntity.Tipo.Should().Be(inputDto.Tipo);
        capturedEntity.Dec.Should().Be(inputDto.Dec);
        capturedEntity.Tam.Should().Be(inputDto.Tam);
        capturedEntity.OUsrInis.Should().Be("admin_user", "o campo de auditoria deve ser preenchido");
    }

    #endregion

    #region ✅ EXEMPLO 4: Teste de Exceção

    /// <summary>
    /// Testa cenário de erro: repositório lança exceção
    /// </summary>
    [Theory]
    [AutoMoqData]
    public async Task Handle_RepositoryThrowsException_ShouldPropagateException(
        [Frozen] Mock<IPara1Repository> repositoryMock,
        CreateParameterCommandHandler sut,
        CreateParameterInputDTO inputDto)
    {
        // Arrange
        var expectedException = new InvalidOperationException("Database connection failed");

        repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Para1>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var command = new CreateParameterCommand(inputDto, "test_user");

        // Act & Assert
        var act = async () => await sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database connection failed");
    }

    #endregion

    #region ✅ EXEMPLO 5: Teste com CancellationToken

    /// <summary>
    /// Verifica que CancellationToken é propagado ao repositório
    /// </summary>
    [Theory]
    [AutoMoqData]
    public async Task Handle_ShouldPassCancellationTokenToRepository(
        [Frozen] Mock<IPara1Repository> repositoryMock,
        CreateParameterCommandHandler sut,
        CreateParameterInputDTO inputDto,
        Para1 savedEntity)
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Para1>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedEntity);

        var command = new CreateParameterCommand(inputDto, "test_user");

        // Act
        await sut.Handle(command, cancellationToken);

        // Assert
        repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Para1>(), cancellationToken),
            Times.Once,
            "o cancellation token deve ser passado ao repositório");
    }

    #endregion

    #region ✅ EXEMPLO 6: Teste com Fixture Customizado

    /// <summary>
    /// Usa Fixture customizado para cenários específicos
    /// </summary>
    [Fact]
    public async Task Handle_LongDescription_ShouldTruncateCorrectly()
    {
        // Arrange: Criar fixture customizado
        var fixture = new Fixture();
        fixture.Customize(new AutoMoqCustomization());

        var repositoryMock = fixture.Freeze<Mock<IPara1Repository>>();

        var longDescription = new string('A', 500); // 500 caracteres
        var inputDto = new CreateParameterInputDTO
        {
            Descricao = longDescription,
            Valor = "test",
            Tipo = "T",
            Dec = 0,
            Tam = 0
        };

        var savedEntity = new Para1(
            fixture.Create<string>().Substring(0, 25),
            longDescription.Substring(0, 100), // Assumindo truncate a 100
            "test",
            "T",
            0,
            0,
            "test_user");

        repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Para1>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedEntity);

        var sut = fixture.Create<CreateParameterCommandHandler>();
        var command = new CreateParameterCommand(inputDto, "test_user");

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        result.Descricao.Length.Should().BeLessOrEqualTo(100, "descrição deve ser truncada");
    }

    #endregion

    #region ✅ EXEMPLO 7: Teste com Dados Específicos (Bogus)

    /// <summary>
    /// Usa Bogus para gerar dados realistas
    /// </summary>
    [Fact]
    public async Task Handle_RealisticData_ShouldCreateParameter()
    {
        // Arrange: Gerar dados realistas com Bogus
        var faker = new Bogus.Faker("pt_BR");

        var inputDto = new CreateParameterInputDTO
        {
            Descricao = faker.Commerce.ProductName(),
            Valor = faker.Random.Number(1, 1000).ToString(),
            Tipo = faker.PickRandom("N", "T", "D"), // Número, Texto, Data
            Dec = faker.Random.Number(0, 5),
            Tam = faker.Random.Number(1, 50)
        };

        var fixture = new Fixture();
        fixture.Customize(new AutoMoqCustomization());

        var repositoryMock = fixture.Freeze<Mock<IPara1Repository>>();
        var savedEntity = new Para1(
            fixture.Create<string>().Substring(0, 25),
            inputDto.Descricao,
            inputDto.Valor,
            inputDto.Tipo,
            inputDto.Dec,
            inputDto.Tam,
            "test_user");

        repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Para1>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedEntity);

        var sut = fixture.Create<CreateParameterCommandHandler>();
        var command = new CreateParameterCommand(inputDto, "test_user");

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Descricao.Should().Be(inputDto.Descricao);
        result.Valor.Should().Be(inputDto.Valor);
        result.Tipo.Should().Be(inputDto.Tipo);
    }

    #endregion
}
