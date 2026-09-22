namespace Shared.Abstractions.DocumentTextExtraction;

/// <summary>
/// Outcome of extracting plain text from a document stream.
/// </summary>
public sealed class DocumentTextExtractionResult
{
    public required string Text { get; init; }

    public required DocumentTextSource Source { get; init; }

    /// <summary>Extractor or OCR engine name used for the successful path.</summary>
    public string? EngineName { get; init; }

    public bool Success { get; init; } = true;

    public string? Error { get; init; }

    public static DocumentTextExtractionResult FromNative(string text, string engineName) => new()
    {
        Text = text,
        Source = DocumentTextSource.Native,
        EngineName = engineName,
        Success = true
    };

    public static DocumentTextExtractionResult FromOcr(string text, string engineName) => new()
    {
        Text = text,
        Source = DocumentTextSource.Ocr,
        EngineName = engineName,
        Success = true
    };

    public static DocumentTextExtractionResult Empty(string? error = null) => new()
    {
        Text = string.Empty,
        Source = DocumentTextSource.None,
        Success = true,
        Error = error
    };

    public static DocumentTextExtractionResult Failure(string error) => new()
    {
        Text = string.Empty,
        Source = DocumentTextSource.None,
        Success = false,
        Error = error
    };
}
