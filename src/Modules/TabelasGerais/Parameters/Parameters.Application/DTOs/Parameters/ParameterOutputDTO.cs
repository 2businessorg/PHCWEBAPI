namespace Parameters.Application.DTOs.Parameters;

public record ParameterOutputDTO
{
    public string Para1Stamp { get; init; } = string.Empty;
    public string Descricao { get; init; } = string.Empty;
    public string Valor { get; init; } = string.Empty;
    public string Tipo { get; init; } = string.Empty;
    public int? Dec { get; init; }
    public int? Tam { get; init; }
    public DateTime OUsrData { get; init; }
    public string OUsrHora { get; init; } = string.Empty;
    public string? OUsrInis { get; init; }
    public DateTime? UsrData { get; init; }
    public string? UsrHora { get; init; }
    public string? UsrInis { get; init; }
}
