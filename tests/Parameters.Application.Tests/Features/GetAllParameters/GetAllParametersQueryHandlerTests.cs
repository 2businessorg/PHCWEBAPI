using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using Parameters.Application.Features.GetAllParameters;
using Parameters.Domain.Repositories;
using Parameters.Domain.Entities;
using Parameters.Application.Tests.Fixtures;
using Xunit;

namespace Parameters.Application.Tests.Features.GetAllParameters;

/// <summary>
/// ✅ Testes do GetAllParametersQueryHandler
/// Demonstra testes de queries e coleções
/// </summary>
public class GetAllParametersQueryHandlerTests
{
    #region ✅ EXEMPLO 1: Query retorna lista vazia

    [Theory]
    [AutoMoqData]
    public async Task Handle_NoParameters_ShouldReturnEmptyList(
        [Frozen] Mock<IPara1Repository> repositoryMock,
        GetAllParametersQueryHandler sut)
    {
        // Arrange
        repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Para1>());

        var query = new GetAllParametersQuery(IncludeInactive: false);

        // Act
        var result = await sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull("o handler deve sempre retornar uma coleção");
        result.Should().BeEmpty("não há parâmetros no repositório");
    }

    #endregion

    #region ✅ EXEMPLO 2: Query retorna múltiplos itens

    [Theory]
    [AutoMoqData]
    public async Task Handle_MultipleParameters_ShouldReturnAll(
        [Frozen] Mock<IPara1Repository> repositoryMock,
        GetAllParametersQueryHandler sut,
        List<Para1> parameters)
    {
        // Arrange: Garantir que temos 5 parâmetros
        var fixture = new Fixture();
        parameters = Enumerable.Range(1, 5)
            .Select(i => new Para1(
                fixture.Create<string>().Substring(0, 25),
                $"Parâmetro {i}",
                $"Valor {i}",
                "T",
                null,
                null,
                "test_user"))
            .ToList();

        repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(parameters);

        var query = new GetAllParametersQuery(IncludeInactive: false);

        // Act
        var result = await sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(5, "há 5 parâmetros no repositório");
        result.Should().OnlyContain(dto => !string.IsNullOrEmpty(dto.Descricao));
    }

    #endregion

    #region ✅ EXEMPLO 3: Verificar DTOs são mapeados corretamente

    [Theory]
    [AutoMoqData]
    public async Task Handle_ShouldMapEntitiesToDtos(
        [Frozen] Mock<IPara1Repository> repositoryMock,
        GetAllParametersQueryHandler sut)
    {
        // Arrange
        var fixture = new Fixture();
        var entity1 = new Para1(
            "STAMP001",
            "Descrição 1",
            "Valor 1",
            "T",
            2,
            50,
            "user1");

        var entity2 = new Para1(
            "STAMP002",
            "Descrição 2",
            "Valor 2",
            "N",
            3,
            100,
            "user2");

        repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Para1> { entity1, entity2 });

        var query = new GetAllParametersQuery(IncludeInactive: false);

        // Act
        var result = await sut.Handle(query, CancellationToken.None);

        // Assert
        var resultList = result.ToList();

        resultList[0].Para1Stamp.Should().Be("STAMP001");
        resultList[0].Descricao.Should().Be("Descrição 1");
        resultList[0].Valor.Should().Be("Valor 1");

        resultList[1].Para1Stamp.Should().Be("STAMP002");
        resultList[1].Descricao.Should().Be("Descrição 2");
        resultList[1].Valor.Should().Be("Valor 2");
    }

    #endregion

    #region ✅ EXEMPLO 4: Parâmetros includeInactive

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Handle_ShouldPassIncludeInactiveToRepository(bool includeInactive)
    {
        // Arrange
        var fixture = new Fixture();
        fixture.Customize(new AutoFixture.AutoMoq.AutoMoqCustomization());

        var repositoryMock = fixture.Freeze<Mock<IPara1Repository>>();
        
        repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Para1>());

        var sut = fixture.Create<GetAllParametersQueryHandler>();
        var query = new GetAllParametersQuery(includeInactive);

        // Act
        await sut.Handle(query, CancellationToken.None);

        // Assert
        repositoryMock.Verify(
            r => r.GetAllAsync(includeInactive, It.IsAny<CancellationToken>()),
            Times.Once,
            $"o repositório deve ser chamado com includeInactive={includeInactive}");
    }

    #endregion

    #region ✅ EXEMPLO 5: Performance - Grande quantidade de dados

    [Fact]
    public async Task Handle_LargeDataset_ShouldHandleEfficiently()
    {
        // Arrange
        var fixture = new Fixture();
        fixture.Customize(new AutoFixture.AutoMoq.AutoMoqCustomization());

        var repositoryMock = fixture.Freeze<Mock<IPara1Repository>>();

        // Criar 1000 parâmetros
        var largeList = Enumerable.Range(1, 1000)
            .Select(i => new Para1(
                $"STAMP{i:D6}",
                $"Parâmetro {i}",
                $"Valor {i}",
                "T",
                null,
                null,
                "test_user"))
            .ToList();

        repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(largeList);

        var sut = fixture.Create<GetAllParametersQueryHandler>();
        var query = new GetAllParametersQuery(IncludeInactive: false);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await sut.Handle(query, CancellationToken.None);
        stopwatch.Stop();

        // Assert
        result.Should().HaveCount(1000, "deve retornar todos os 1000 itens");
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500, 
            "mapeamento de 1000 itens deve ser rápido (<500ms)");
    }

    #endregion

    #region ✅ EXEMPLO 6: Verificar ordem dos resultados

    [Theory]
    [AutoMoqData]
    public async Task Handle_ShouldReturnResultsInCorrectOrder(
        [Frozen] Mock<IPara1Repository> repositoryMock,
        GetAllParametersQueryHandler sut)
    {
        // Arrange
        var fixture = new Fixture();
        var parameters = new List<Para1>
        {
            new Para1("STAMP003", "Zebra", "C", "T", null, null, "user"),
            new Para1("STAMP001", "Alpha", "A", "T", null, null, "user"),
            new Para1("STAMP002", "Beta", "B", "T", null, null, "user")
        };

        repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(parameters);

        var query = new GetAllParametersQuery(IncludeInactive: false);

        // Act
        var result = await sut.Handle(query, CancellationToken.None);

        // Assert
        var resultList = result.ToList();
        
        // Verificar que mantém a ordem retornada pelo repositório
        resultList[0].Descricao.Should().Be("Zebra");
        resultList[1].Descricao.Should().Be("Alpha");
        resultList[2].Descricao.Should().Be("Beta");
    }

    #endregion

    #region ✅ EXEMPLO 7: Teste com exceção do repositório

    [Theory]
    [AutoMoqData]
    public async Task Handle_RepositoryThrows_ShouldPropagateException(
        [Frozen] Mock<IPara1Repository> repositoryMock,
        GetAllParametersQueryHandler sut)
    {
        // Arrange
        repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        var query = new GetAllParametersQuery(IncludeInactive: false);

        // Act
        var act = async () => await sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database error");
    }

    #endregion
}
