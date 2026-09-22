namespace Shared.Infrastructure.Privacy.Pseudonymization.Options;

/// <summary>
/// Bound from configuration section <c>Pseudonymization</c>.
/// Placeholders only in committed appsettings — never real keys.
/// </summary>
public sealed class PseudonymizationOptions
{
    public const string SectionName = "Pseudonymization";

    /// <summary>Master switch for registering/using the shared pipeline.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Product cloud gate (Recruitment maps EnableCloudLlm here). Default false (BR-08).
    /// </summary>
    public bool EnableCloudLlm { get; set; }

    /// <summary>Base URL of the Presidio sidecar (e.g. http://127.0.0.1:5001).</summary>
    public string PresidioBaseUrl { get; set; } = "http://127.0.0.1:5001";

    public int PresidioTimeoutSeconds { get; set; } = 15;

    public string Language { get; set; } = "pt";

    /// <summary>
    /// Base64-encoded 32-byte AES key placeholder. Override via secret store / env in ops.
    /// Empty = ephemeral in-memory key (dev/test only; maps not durable across restarts).
    /// </summary>
    public string? TokenMapEncryptionKeyBase64 { get; set; }

    /// <summary>
    /// Base64-encoded HMAC key placeholder. Empty = derive from encryption key material.
    /// </summary>
    public string? TokenMapHmacKeyBase64 { get; set; }

    public int DefaultMapTtlHours { get; set; } = 24;

    public bool ConservativeOnLowConfidence { get; set; } = true;

    public bool PseudonymizeEducation { get; set; }

    public bool PreserveProfessionalSkills { get; set; } = true;

    public bool StrictDetoken { get; set; } = true;

    /// <summary>v3 stub — quasi-ID generalization off in v1.</summary>
    public bool EnableQuasiIdGeneralization { get; set; }

    /// <summary>v2 stub — GLiNER/spaCy ensemble off in v1.</summary>
    public bool EnableHybridNerV2 { get; set; }

    /// <summary>
    /// When true, CloudEgress requires a healthy Presidio sidecar (fail-closed).
    /// OnPrem / Mode.Off never requires sidecar.
    /// </summary>
    public bool RequireSidecarForCloudEgress { get; set; } = true;

    /// <summary>Store implementation: InMemory (default) or Sql (schema provided; wire later).</summary>
    public string TokenMapStore { get; set; } = "InMemory";
}
