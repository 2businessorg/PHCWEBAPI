namespace Shared.Infrastructure.DocumentTextExtraction.Options;

/// <summary>
/// Bound from configuration section <c>DocumentTextExtraction</c>.
/// </summary>
public sealed class DocumentTextExtractionOptions
{
    public const string SectionName = "DocumentTextExtraction";

    /// <summary>
    /// Minimum non-whitespace character count for native text to be accepted without OCR.
    /// </summary>
    public int MinNativeCharacters { get; set; } = 40;

    /// <summary>When false, OCR is never attempted even if an engine is registered.</summary>
    public bool OcrEnabled { get; set; } = true;

    /// <summary>
    /// Preferred OCR engine name (must match <c>IOcrEngine.EngineName</c>).
    /// Empty = first available registered engine.
    /// </summary>
    public string OcrEngineName { get; set; } = "TesseractCli";

    /// <summary>
    /// Language codes for Tesseract (e.g. "eng", "por", "eng+por").
    /// </summary>
    public string OcrLanguage { get; set; } = "eng";

    /// <summary>
    /// Optional absolute path to the <c>tesseract</c> executable.
    /// Empty = resolve from PATH.
    /// </summary>
    public string? TesseractExecutablePath { get; set; }

    /// <summary>
    /// Optional TESSDATA_PREFIX / tessdata directory override.
    /// </summary>
    public string? TessDataPath { get; set; }

    /// <summary>Timeout for a single OCR process invocation.</summary>
    public int OcrTimeoutSeconds { get; set; } = 60;
}
