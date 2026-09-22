namespace Shared.Abstractions.Privacy.Pseudonymization;

/// <summary>
/// Pseudonymization path. Cloud egress requires EnableCloudLlm + GO (BR-08).
/// This is pseudonymization (reversible map), not irreversible anonymization.
/// </summary>
public enum PseudonymizationMode
{
    Off = 0,
    CloudEgress = 1,
    RedactedScoring = 2
}
