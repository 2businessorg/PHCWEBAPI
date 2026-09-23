using System.Text.RegularExpressions;

namespace Shared.Infrastructure.Privacy.Pseudonymization.Detection;

/// <summary>
/// Contact patterns shared by the local detector and the independent leak checker.
/// Keep them in one place so a number the detector redacts is the same number the
/// leak checker would otherwise fail closed on (PT mobile and Mozambique +258).
/// </summary>
internal static class ContactSignals
{
    internal const string EmailPattern = @"\b[A-Z0-9._%+\-]+@[A-Z0-9.\-]+\.[A-Z]{2,}\b";

    /// <summary>
    /// PT mobile (optional +351) or Mozambique mobile 82-87 (optional +258).
    /// </summary>
    internal const string PhonePattern =
        @"(?:(?:\+351[\s\-]?)?9\d{2}(?:[\s\-]?\d{3}){2}|(?:\+258[\s\-]?)?8[2-7](?:[\s\-]?\d){7})";

    internal const string NifPattern = @"\b[123568]\d{8}\b";

    internal static readonly Regex Email = new(
        EmailPattern,
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    internal static readonly Regex Phone = new(
        PhonePattern,
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    internal static readonly Regex Nif = new(
        NifPattern,
        RegexOptions.CultureInvariant | RegexOptions.Compiled);
}
