using FluentAssertions;
using Shared.Infrastructure.DocumentTextExtraction.Extractors;
using Shared.Infrastructure.Tests.Fixtures;
using Xunit;

namespace Shared.Infrastructure.Tests.DocumentTextExtraction;

public class NativeExtractorTests
{
    [Fact]
    public async Task PdfPig_ExtractsTextFromTinyFixture()
    {
        var path = DocumentFixtures.EnsurePdfFixture(
            "sample-native.pdf",
            "PHC shared document text extraction fixture content.");
        var bytes = await File.ReadAllBytesAsync(path);

        var extractor = new PdfPigNativeTextExtractor();
        await using var stream = new MemoryStream(bytes);
        var text = await extractor.ExtractAsync(stream);

        text.Should().NotBeNullOrWhiteSpace();
        text.Should().Contain("PHC shared document");
    }

    [Fact]
    public async Task Docx_ExtractsTextFromTinyFixture()
    {
        var path = DocumentFixtures.EnsureDocxFixture(
            "sample-native.docx",
            "PHC DOCX fixture for native OpenXml extraction.");
        var bytes = await File.ReadAllBytesAsync(path);

        var extractor = new DocxOpenXmlNativeTextExtractor();
        await using var stream = new MemoryStream(bytes);
        var text = await extractor.ExtractAsync(stream);

        text.Should().NotBeNullOrWhiteSpace();
        text.Should().Contain("DOCX fixture");
    }
}
