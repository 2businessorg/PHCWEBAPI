using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Abstractions.DocumentTextExtraction;
using Shared.Infrastructure.DocumentTextExtraction.Extractors;
using Shared.Infrastructure.DocumentTextExtraction.Ocr;
using Shared.Infrastructure.DocumentTextExtraction.Options;

namespace Shared.Infrastructure.DocumentTextExtraction;

/// <summary>
/// Registers shared document text extraction (native PDF/DOCX + optional OCR).
/// </summary>
public static class DocumentTextExtractionDependencyInjection
{
    public static IServiceCollection AddDocumentTextExtraction(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<DocumentTextExtractionOptions>(
            configuration.GetSection(DocumentTextExtractionOptions.SectionName));

        services.AddSingleton<INativeDocumentTextExtractor, PdfPigNativeTextExtractor>();
        services.AddSingleton<INativeDocumentTextExtractor, DocxOpenXmlNativeTextExtractor>();
        services.AddSingleton<IOcrEngine, TesseractCliOcrEngine>();
        services.AddSingleton<IDocumentTextExtractor, CompositeDocumentTextExtractor>();

        return services;
    }
}
