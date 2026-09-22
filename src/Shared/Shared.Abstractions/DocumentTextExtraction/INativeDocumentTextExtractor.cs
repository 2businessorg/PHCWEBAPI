namespace Shared.Abstractions.DocumentTextExtraction;

/// <summary>
/// Strategy for reading embedded/digital text without OCR (PDF text layer, DOCX, etc.).
/// </summary>
public interface INativeDocumentTextExtractor
{
    /// <summary>Stable name for diagnostics (e.g. "PdfPig", "OpenXmlDocx").</summary>
    string EngineName { get; }

    bool CanHandle(string? fileName, string? contentType);

    /// <summary>
    /// Returns extracted text, or null/empty when the format is wrong or has no text layer.
    /// </summary>
    Task<string?> ExtractAsync(Stream content, CancellationToken cancellationToken = default);
}
