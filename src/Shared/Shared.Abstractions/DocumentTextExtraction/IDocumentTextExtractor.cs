namespace Shared.Abstractions.DocumentTextExtraction;

/// <summary>
/// Platform facade: extract plain text from PDF, DOCX, or images.
/// Prefer this over calling native extractors or OCR directly.
/// </summary>
public interface IDocumentTextExtractor
{
    /// <summary>
    /// Extracts plain text from <paramref name="content"/>.
    /// The stream position is not required to be at the start; the implementation
    /// buffers as needed. The caller retains ownership of the stream.
    /// </summary>
    Task<DocumentTextExtractionResult> ExtractAsync(
        Stream content,
        string? fileName = null,
        string? contentType = null,
        CancellationToken cancellationToken = default);
}
