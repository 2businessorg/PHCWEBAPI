namespace Recruitment.Domain.Entities;

/// <summary>
/// CV attachment from anexos where oritable='cve' and tipo=1 (bytes in bdados).
/// </summary>
public sealed class CvAnexo
{
    public required string AnexoStamp { get; init; }

    public required string CveStamp { get; init; }

    public required byte[] Bytes { get; init; }

    public string? FileName { get; init; }

    public string? ContentType { get; init; }

    public string? Texto { get; init; }
}
