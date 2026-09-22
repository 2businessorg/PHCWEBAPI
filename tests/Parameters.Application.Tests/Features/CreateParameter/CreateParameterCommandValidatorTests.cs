using FluentAssertions;
using FluentValidation.TestHelper;
using Parameters.Application.DTOs.Parameters;
using Parameters.Application.Features.CreateParameter;
using Xunit;

namespace Parameters.Application.Tests.Features.CreateParameter;

/// <summary>
/// ✅ Testes do CreateParameterCommandValidator
/// Demonstra uso de FluentValidation.TestHelper para testar validações
/// </summary>
public class CreateParameterCommandValidatorTests
{
    private readonly CreateParameterCommandValidator _validator;

    public CreateParameterCommandValidatorTests()
    {
        _validator = new CreateParameterCommandValidator();
    }

    #region ✅ EXEMPLO 1: Testes de Campo Obrigatório

    /// <summary>
    /// Descricao é obrigatória
    /// </summary>
    [Fact]
    public void Validate_DescricaoEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new CreateParameterInputDTO
        {
            Descricao = string.Empty,
            Valor = "test",
            Tipo = "T",
            Dec = 0,
            Tam = 0
        };
        var command = new CreateParameterCommand(dto, "test_user");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Dto.Descricao)
            .WithErrorMessage("A Descrição é obrigatória")
            .Only(); // ✅ Garante que é o único erro
    }

    [Fact]
    public void Validate_DescricaoNull_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new CreateParameterInputDTO
        {
            Descricao = null!,
            Valor = "test",
            Tipo = "T"
        };
        var command = new CreateParameterCommand(dto, "test_user");

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Dto.Descricao);
    }

    #endregion

    #region ✅ EXEMPLO 2: Testes de Tamanho Máximo

    /// <summary>
    /// Testa múltiplos tamanhos para Descricao
    /// </summary>
    [Theory]
    [InlineData(50, false)]   // ✅ Válido
    [InlineData(100, false)]  // ✅ Válido (limite)
    [InlineData(101, true)]   // ❌ Inválido (excede)
    [InlineData(200, true)]   // ❌ Inválido
    public void Validate_DescricaoLength_ShouldValidateCorrectly(int length, bool shouldHaveError)
    {
        // Arrange
        var dto = new CreateParameterInputDTO
        {
            Descricao = new string('A', length),
            Valor = "test",
            Tipo = "T"
        };
        var command = new CreateParameterCommand(dto, "test_user");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        if (shouldHaveError)
        {
            result.ShouldHaveValidationErrorFor(c => c.Dto.Descricao)
                .WithErrorMessage("A Descrição não pode exceder 100 caracteres");
        }
        else
        {
            result.ShouldNotHaveValidationErrorFor(c => c.Dto.Descricao);
        }
    }

    #endregion

    #region ✅ EXEMPLO 3: Testes de Valores Numéricos

    /// <summary>
    /// Dec deve estar em range válido
    /// </summary>
    [Theory]
    [InlineData(-1, true)]   // ❌ Negativo inválido
    [InlineData(0, false)]   // ✅ Válido
    [InlineData(5, false)]   // ✅ Válido
    [InlineData(10, false)]  // ✅ Válido (limite)
    [InlineData(11, true)]   // ❌ Excede limite
    public void Validate_DecValue_ShouldValidateRange(int decValue, bool shouldHaveError)
    {
        // Arrange
        var dto = new CreateParameterInputDTO
        {
            Descricao = "Test",
            Valor = "test",
            Tipo = "N", // Número
            Dec = decValue
        };
        var command = new CreateParameterCommand(dto, "test_user");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        if (shouldHaveError)
        {
            result.ShouldHaveValidationErrorFor(c => c.Dto.Dec);
        }
        else
        {
            result.ShouldNotHaveValidationErrorFor(c => c.Dto.Dec);
        }
    }

    #endregion

    #region ✅ EXEMPLO 4: Teste Completo de Comando Válido

    /// <summary>
    /// Comando completamente válido não deve ter erros
    /// </summary>
    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveAnyErrors()
    {
        // Arrange
        var dto = new CreateParameterInputDTO
        {
            Descricao = "Parâmetro de Teste",
            Valor = "Valor de Teste",
            Tipo = "T",
            Dec = 2,
            Tam = 50
        };
        var command = new CreateParameterCommand(dto, "admin");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region ✅ EXEMPLO 5: Múltiplos Erros Simultâneos

    /// <summary>
    /// Testa comando com múltiplos erros
    /// </summary>
    [Fact]
    public void Validate_MultipleErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var dto = new CreateParameterInputDTO
        {
            Descricao = string.Empty,        // ❌ Vazio
            Valor = string.Empty,            // ❌ Vazio
            Tipo = string.Empty,             // ❌ Vazio
            Dec = -5,                        // ❌ Negativo
            Tam = 1000                       // ❌ Muito grande
        };
        var command = new CreateParameterCommand(dto, "test_user");

        // Act
        var result = _validator.TestValidate(command);

        // Assert: Verificar que todos os campos têm erros
        result.ShouldHaveValidationErrorFor(c => c.Dto.Descricao);
        result.ShouldHaveValidationErrorFor(c => c.Dto.Dec);

        // ✅ Contar total de erros
        result.Errors.Should().HaveCountGreaterOrEqualTo(2, 
            "deve ter pelo menos 2 erros de validação");
    }

    #endregion

    #region ✅ EXEMPLO 6: Edge Cases

    /// <summary>
    /// Testa casos extremos (edge cases)
    /// </summary>
    [Theory]
    [InlineData("")]                              // Empty
    [InlineData(" ")]                             // Whitespace
    [InlineData("   ")]                           // Multiple spaces
    [InlineData("\t")]                            // Tab
    [InlineData("\n")]                            // Newline
    public void Validate_DescricaoWhitespace_ShouldHaveError(string descricao)
    {
        // Arrange
        var dto = new CreateParameterInputDTO
        {
            Descricao = descricao,
            Valor = "test",
            Tipo = "T"
        };
        var command = new CreateParameterCommand(dto, "test_user");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Dto.Descricao);
    }

    #endregion

    #region ✅ EXEMPLO 7: Caracteres Especiais

    /// <summary>
    /// Testa se caracteres especiais são aceitos
    /// </summary>
    [Theory]
    [InlineData("Parâmetro com acentuação")]
    [InlineData("Parameter with special chars !@#$%")]
    [InlineData("Parâmetro com números 12345")]
    [InlineData("Parameter_with_underscores")]
    [InlineData("Parameter-with-dashes")]
    public void Validate_DescricaoSpecialCharacters_ShouldBeValid(string descricao)
    {
        // Arrange
        var dto = new CreateParameterInputDTO
        {
            Descricao = descricao,
            Valor = "test",
            Tipo = "T"
        };
        var command = new CreateParameterCommand(dto, "test_user");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.Dto.Descricao);
    }

    #endregion

    #region ✅ EXEMPLO 8: Validação Condicional

    /// <summary>
    /// Testa validação que depende de outro campo
    /// Exemplo: Se Tipo = 'N' (Número), Dec é obrigatório
    /// </summary>
    [Theory]
    [InlineData("N", null, true)]    // ❌ Tipo Número sem Dec
    [InlineData("N", 2, false)]      // ✅ Tipo Número com Dec
    [InlineData("T", null, false)]   // ✅ Tipo Texto sem Dec (ok)
    [InlineData("D", null, false)]   // ✅ Tipo Data sem Dec (ok)
    public void Validate_ConditionalValidation_DecRequiredForNumericType(
        string tipo, 
        int? dec, 
        bool shouldHaveError)
    {
        // Arrange
        var dto = new CreateParameterInputDTO
        {
            Descricao = "Test Parameter",
            Valor = "123",
            Tipo = tipo,
            Dec = dec
        };
        var command = new CreateParameterCommand(dto, "test_user");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        if (shouldHaveError)
        {
            result.ShouldHaveValidationErrorFor(c => c.Dto.Dec)
                .WithErrorMessage("Dec é obrigatório para parâmetros do tipo Numérico");
        }
        else
        {
            result.ShouldNotHaveValidationErrorFor(c => c.Dto.Dec);
        }
    }

    #endregion
}
