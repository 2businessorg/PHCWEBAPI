using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Abstractions.DocumentTextExtraction;
using Shared.Infrastructure.DocumentTextExtraction.Ocr;
using Shared.Infrastructure.DocumentTextExtraction.Options;

namespace Shared.Infrastructure.DocumentTextExtraction;

/// <summary>
/// Composite: try native extractors first; fall back to OCR when text quality is weak
/// or the input is an image / image-only PDF.
/// </summary>
public sealed class CompositeDocumentTextExtractor : IDocumentTextExtractor
{
    private readonly IReadOnlyList<INativeDocumentTextExtractor> _nativeExtractors;
    private readonly IOcrEngine? _ocrEngine;
    private readonly DocumentTextExtractionOptions _options;
    private readonly ILogger<CompositeDocumentTextExtractor> _logger;

    public CompositeDocumentTextExtractor(
        IEnumerable<INativeDocumentTextExtractor> nativeExtractors,
        IEnumerable<IOcrEngine> ocrEngines,
        IOptions<DocumentTextExtractionOptions> options,
        ILogger<CompositeDocumentTextExtractor> logger)
    {
        _nativeExtractors = nativeExtractors.ToList();
        _options = options.Value;
        _logger = logger;
        _ocrEngine = SelectOcrEngine(ocrEngines, _options);
    }

    public async Task<DocumentTextExtractionResult> ExtractAsync(
        Stream content,
        string? fileName = null,
        string? contentType = null,
        CancellationToken cancellationToken = default)
    {
        if (content is null)
        {
            return DocumentTextExtractionResult.Failure("Content stream is null.");
        }

        byte[] buffer;
        try
        {
            buffer = await BufferAsync(content, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read document stream.");
            return DocumentTextExtractionResult.Failure("Failed to read document stream.");
        }

        if (buffer.Length == 0)
        {
            return DocumentTextExtractionResult.Empty("Empty content stream.");
        }

        string? nativeText = null;
        string? nativeEngine = null;

        foreach (var extractor in _nativeExtractors.Where(e => e.CanHandle(fileName, contentType)))
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await using var slice = new MemoryStream(buffer, writable: false);
                var text = await extractor.ExtractAsync(slice, cancellationToken).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    nativeText = text.Trim();
                    nativeEngine = extractor.EngineName;
                    break;
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogDebug(ex, "Native extractor {Engine} failed for {File}.", extractor.EngineName, fileName);
            }
        }

        if (IsNativeTextAcceptable(nativeText))
        {
            return DocumentTextExtractionResult.FromNative(nativeText!, nativeEngine!);
        }

        var shouldOcr = _options.OcrEnabled &&
                        _ocrEngine is { IsAvailable: true } &&
                        (TesseractCliOcrEngine.IsLikelyImage(fileName, contentType) ||
                         IsPdf(fileName, contentType) ||
                         !IsNativeTextAcceptable(nativeText));

        if (shouldOcr)
        {
            try
            {
                await using var slice = new MemoryStream(buffer, writable: false);
                string? ocrText;

                if (TesseractCliOcrEngine.IsLikelyImage(fileName, contentType))
                {
                    ocrText = await _ocrEngine!.ExtractFromImageAsync(slice, cancellationToken)
                        .ConfigureAwait(false);
                }
                else if (IsPdf(fileName, contentType))
                {
                    ocrText = await _ocrEngine!.ExtractFromPdfAsync(slice, cancellationToken)
                        .ConfigureAwait(false);
                }
                else
                {
                    // Unknown type with weak/no native text: try image OCR as last resort.
                    ocrText = await _ocrEngine!.ExtractFromImageAsync(slice, cancellationToken)
                        .ConfigureAwait(false);
                }

                if (!string.IsNullOrWhiteSpace(ocrText))
                {
                    return DocumentTextExtractionResult.FromOcr(ocrText.Trim(), _ocrEngine.EngineName);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "OCR fallback failed for {File}.", fileName);
            }
        }

        if (!string.IsNullOrWhiteSpace(nativeText) && nativeEngine is not null)
        {
            // Weak native text but still better than nothing (OCR off or failed).
            return DocumentTextExtractionResult.FromNative(nativeText, nativeEngine);
        }

        return DocumentTextExtractionResult.Empty(
            "No usable text extracted (native empty/weak and OCR unavailable or failed).");
    }

    private bool IsNativeTextAcceptable(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var significant = text.Count(c => !char.IsWhiteSpace(c));
        return significant >= Math.Max(1, _options.MinNativeCharacters);
    }

    private static IOcrEngine? SelectOcrEngine(
        IEnumerable<IOcrEngine> engines,
        DocumentTextExtractionOptions options)
    {
        var list = engines.ToList();
        if (list.Count == 0)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(options.OcrEngineName))
        {
            var named = list.FirstOrDefault(e =>
                string.Equals(e.EngineName, options.OcrEngineName, StringComparison.OrdinalIgnoreCase));
            if (named is not null)
            {
                return named;
            }
        }

        return list.FirstOrDefault(e => e.IsAvailable) ?? list[0];
    }

    private static async Task<byte[]> BufferAsync(Stream content, CancellationToken cancellationToken)
    {
        if (content.CanSeek)
        {
            content.Position = 0;
        }

        using var ms = new MemoryStream();
        await content.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
        return ms.ToArray();
    }

    private static bool IsPdf(string? fileName, string? contentType)
    {
        if (!string.IsNullOrWhiteSpace(contentType) &&
            contentType.Contains("pdf", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(fileName) &&
               fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
    }
}
