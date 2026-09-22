using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Shared.Abstractions.DocumentTextExtraction;
using Shared.Infrastructure.DocumentTextExtraction;
using Shared.Infrastructure.DocumentTextExtraction.Extractors;
using Shared.Infrastructure.DocumentTextExtraction.Options;
using Shared.Infrastructure.Tests.Fixtures;
using Xunit;

namespace Shared.Infrastructure.Tests.DocumentTextExtraction;

public class CompositeDocumentTextExtractorTests
{
    [Fact]
    public async Task ExtractAsync_DigitalPdf_UsesNativeWithoutOcr()
    {
        var pdfBytes = DocumentFixtures.CreateSimplePdf(
            "Candidate curriculum vitae sample text for native extraction path.");
        var ocr = new Mock<IOcrEngine>(MockBehavior.Strict);
        ocr.SetupGet(e => e.EngineName).Returns("MockOcr");
        ocr.SetupGet(e => e.IsAvailable).Returns(true);

        var sut = CreateSut(ocr.Object, minNativeCharacters: 20);

        await using var stream = new MemoryStream(pdfBytes);
        var result = await sut.ExtractAsync(stream, "cv.pdf", "application/pdf");

        result.Success.Should().BeTrue();
        result.Source.Should().Be(DocumentTextSource.Native);
        result.EngineName.Should().Be("PdfPig");
        result.Text.Should().Contain("curriculum");
        ocr.Verify(e => e.ExtractFromPdfAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Never);
        ocr.Verify(e => e.ExtractFromImageAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExtractAsync_DigitalDocx_UsesNative()
    {
        var docxBytes = DocumentFixtures.CreateSimpleDocx(
            "Documento DOCX com texto digital suficiente para extracao nativa sem OCR.");
        var ocr = CreateUnavailableOcr();
        var sut = CreateSut(ocr.Object, minNativeCharacters: 20);

        await using var stream = new MemoryStream(docxBytes);
        var result = await sut.ExtractAsync(
            stream,
            "nota.docx",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document");

        result.Success.Should().BeTrue();
        result.Source.Should().Be(DocumentTextSource.Native);
        result.EngineName.Should().Be("OpenXmlDocx");
        result.Text.Should().Contain("DOCX");
    }

    [Fact]
    public async Task ExtractAsync_WeakNativeText_FallsBackToOcr()
    {
        var weakNative = new Mock<INativeDocumentTextExtractor>();
        weakNative.SetupGet(e => e.EngineName).Returns("WeakPdf");
        weakNative.Setup(e => e.CanHandle(It.IsAny<string?>(), It.IsAny<string?>())).Returns(true);
        weakNative
            .Setup(e => e.ExtractAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("hi");

        var ocr = new Mock<IOcrEngine>();
        ocr.SetupGet(e => e.EngineName).Returns("MockOcr");
        ocr.SetupGet(e => e.IsAvailable).Returns(true);
        ocr.Setup(e => e.ExtractFromPdfAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("OCR recovered full page text from scanned PDF.");

        var sut = CreateSut(
            ocr.Object,
            minNativeCharacters: 40,
            nativeExtractors: new[] { weakNative.Object });

        await using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4 });
        var result = await sut.ExtractAsync(stream, "scan.pdf", "application/pdf");

        result.Success.Should().BeTrue();
        result.Source.Should().Be(DocumentTextSource.Ocr);
        result.EngineName.Should().Be("MockOcr");
        result.Text.Should().Contain("OCR recovered");
    }

    [Fact]
    public async Task ExtractAsync_Image_UsesOcrWhenEnabled()
    {
        var ocr = new Mock<IOcrEngine>();
        ocr.SetupGet(e => e.EngineName).Returns("MockOcr");
        ocr.SetupGet(e => e.IsAvailable).Returns(true);
        ocr.Setup(e => e.ExtractFromImageAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Hello from image OCR");

        var sut = CreateSut(ocr.Object);

        await using var stream = new MemoryStream(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
        var result = await sut.ExtractAsync(stream, "scan.png", "image/png");

        result.Source.Should().Be(DocumentTextSource.Ocr);
        result.Text.Should().Be("Hello from image OCR");
    }

    [Fact]
    public async Task ExtractAsync_EmptyStream_ReturnsEmpty()
    {
        var sut = CreateSut(CreateUnavailableOcr().Object);

        await using var stream = new MemoryStream();
        var result = await sut.ExtractAsync(stream, "empty.pdf", "application/pdf");

        result.Source.Should().Be(DocumentTextSource.None);
        result.Text.Should().BeEmpty();
        result.Error.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ExtractAsync_CorruptPdf_DoesNotThrow()
    {
        var ocr = CreateUnavailableOcr();
        var sut = CreateSut(ocr.Object);

        await using var stream = new MemoryStream(new byte[] { 0x00, 0x01, 0x02, 0xFF, 0xFE });
        var result = await sut.ExtractAsync(stream, "broken.pdf", "application/pdf");

        result.Source.Should().Be(DocumentTextSource.None);
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ExtractAsync_OcrDisabled_KeepsWeakNative()
    {
        var weakNative = new Mock<INativeDocumentTextExtractor>();
        weakNative.SetupGet(e => e.EngineName).Returns("WeakPdf");
        weakNative.Setup(e => e.CanHandle(It.IsAny<string?>(), It.IsAny<string?>())).Returns(true);
        weakNative
            .Setup(e => e.ExtractAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("short");

        var ocr = new Mock<IOcrEngine>(MockBehavior.Strict);
        ocr.SetupGet(e => e.EngineName).Returns("MockOcr");
        ocr.SetupGet(e => e.IsAvailable).Returns(true);

        var sut = CreateSut(
            ocr.Object,
            minNativeCharacters: 40,
            ocrEnabled: false,
            nativeExtractors: new[] { weakNative.Object });

        await using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var result = await sut.ExtractAsync(stream, "scan.pdf", "application/pdf");

        result.Source.Should().Be(DocumentTextSource.Native);
        result.Text.Should().Be("short");
    }

    private static CompositeDocumentTextExtractor CreateSut(
        IOcrEngine ocr,
        int minNativeCharacters = 40,
        bool ocrEnabled = true,
        IEnumerable<INativeDocumentTextExtractor>? nativeExtractors = null)
    {
        var options = Options.Create(new DocumentTextExtractionOptions
        {
            MinNativeCharacters = minNativeCharacters,
            OcrEnabled = ocrEnabled,
            OcrEngineName = ocr.EngineName
        });

        return new CompositeDocumentTextExtractor(
            nativeExtractors ?? new INativeDocumentTextExtractor[]
            {
                new PdfPigNativeTextExtractor(),
                new DocxOpenXmlNativeTextExtractor()
            },
            new[] { ocr },
            options,
            NullLogger<CompositeDocumentTextExtractor>.Instance);
    }

    private static Mock<IOcrEngine> CreateUnavailableOcr()
    {
        var ocr = new Mock<IOcrEngine>();
        ocr.SetupGet(e => e.EngineName).Returns("MockOcr");
        ocr.SetupGet(e => e.IsAvailable).Returns(false);
        return ocr;
    }
}
