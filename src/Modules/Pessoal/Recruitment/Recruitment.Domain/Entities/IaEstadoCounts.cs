namespace Recruitment.Domain.Entities;

/// <summary>
/// Counts of cve.u_estadoia for the SRT rows of one vacancy.
/// </summary>
public sealed class IaEstadoCounts
{
    public int Pendente { get; init; }

    public int Ok { get; init; }

    public int Erro { get; init; }

    /// <summary>Null, empty, or any value other than pendente/ok/erro.</summary>
    public int SemEstado { get; init; }

    public int Total => Pendente + Ok + Erro + SemEstado;
}
