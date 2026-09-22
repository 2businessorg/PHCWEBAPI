namespace Shared.Abstractions.DocumentTextExtraction;

/// <summary>
/// Optional OCR backend for images and image-heavy / weak-text PDFs.
/// </summary>
public interface IOcrEngine
{
    /// <summary>Stable name for diagnostics (e.g. "TesseractCli").</summary>
    string EngineName { get; }

    /// <summary>Whether this engine can run in the current environment.</summary>
    bool IsAvailable { get; }

    /// <summary>OCR a raster image stream (PNG, JPEG, TIFF, BMP, ...).</summary>
    Task<string?> ExtractFromImageAsync(Stream imageStream, CancellationToken cancellationToken = default);

    /// <summary>
    /// OCR a PDF that has little or no native text (e.g. scanned pages), when supported.
    /// </summary>
    Task<string?> ExtractFromPdfAsync(Stream pdfStream, CancellationToken cancellationToken = default);
}
