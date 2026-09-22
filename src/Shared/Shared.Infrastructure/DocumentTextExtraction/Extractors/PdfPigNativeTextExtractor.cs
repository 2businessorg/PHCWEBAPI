using Shared.Abstractions.DocumentTextExtraction;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace Shared.Infrastructure.DocumentTextExtraction.Extractors;

/// <summary>
/// Native PDF text extraction via PdfPig (digital text layer only).
/// </summary>
public sealed class PdfPigNativeTextExtractor : INativeDocumentTextExtractor
{
    public string EngineName => "PdfPig";

    public bool CanHandle(string? fileName, string? contentType)
    {
        if (!string.IsNullOrWhiteSpace(contentType) &&
            contentType.Contains("pdf", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return HasExtension(fileName, ".pdf");
    }

    public Task<string?> ExtractAsync(Stream content, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            using var document = PdfDocument.Open(content);
            var parts = new List<string>();

            foreach (Page page in document.GetPages())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var pageText = page.Text;
                if (!string.IsNullOrWhiteSpace(pageText))
                {
                    parts.Add(pageText);
                }
            }

            var text = string.Join(Environment.NewLine, parts).Trim();
            return Task.FromResult<string?>(string.IsNullOrEmpty(text) ? null : text);
        }
        catch (Exception)
        {
            return Task.FromResult<string?>(null);
        }
    }

    private static bool HasExtension(string? fileName, string extension)
        => !string.IsNullOrWhiteSpace(fileName) &&
           fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase);
}
