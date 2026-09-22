namespace Recruitment.Domain.Entities;

public sealed class RecruitmentOutboxItem
{
    public long Id { get; init; }

    public required string CveStamp { get; init; }

    public required string RctStamp { get; init; }

    public required string SrtStamp { get; init; }

    public required string AnexoStamp { get; init; }

    public required string Estado { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    public DateTime? StartedAtUtc { get; init; }

    public DateTime? HeartbeatAtUtc { get; init; }

    public DateTime? CompletedAtUtc { get; init; }

    public string? ErrorMessage { get; init; }

    /// <summary>Lab GO Denilson reference when RCTCLB exception applies (BR-01).</summary>
    public string? LabGoRef { get; init; }
}
