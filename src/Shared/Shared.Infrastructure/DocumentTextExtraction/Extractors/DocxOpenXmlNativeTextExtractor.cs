using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Shared.Abstractions.DocumentTextExtraction;

namespace Shared.Infrastructure.DocumentTextExtraction.Extractors;

/// <summary>
/// Native DOCX text extraction via DocumentFormat.OpenXml.
/// </summary>
public sealed class DocxOpenXmlNativeTextExtractor : INativeDocumentTextExtractor
{
    public string EngineName => "OpenXmlDocx";

    public bool CanHandle(string? fileName, string? contentType)
    {
        if (!string.IsNullOrWhiteSpace(contentType))
        {
            if (contentType.Contains("wordprocessingml", StringComparison.OrdinalIgnoreCase) ||
                contentType.Contains("msword", StringComparison.OrdinalIgnoreCase) ||
                contentType.Equals("application/vnd.openxmlformats-officedocument.wordprocessingml.document", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return HasExtension(fileName, ".docx");
    }

    public Task<string?> ExtractAsync(Stream content, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            using var document = WordprocessingDocument.Open(content, isEditable: false);
            var body = document.MainDocumentPart?.Document?.Body;
            if (body is null)
            {
                return Task.FromResult<string?>(null);
            }

            var texts = body.Descendants<Text>().Select(t => t.Text);
            var combined = string.Join(' ', texts).Trim();
            return Task.FromResult<string?>(string.IsNullOrEmpty(combined) ? null : combined);
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
