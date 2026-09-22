using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Shared.Infrastructure.Tests.Fixtures;

internal static class DocumentFixtures
{
    /// <summary>
    /// Minimal PDF 1.4 with a Helvetica text string (digital text layer).
    /// </summary>
    public static byte[] CreateSimplePdf(string text)
    {
        // Escape PDF string delimiters
        var escaped = text.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
        var stream = $"BT /F1 12 Tf 50 700 Td ({escaped}) Tj ET";
        var streamBytes = System.Text.Encoding.ASCII.GetBytes(stream);

        var objects = new List<string>
        {
            "1 0 obj<< /Type /Catalog /Pages 2 0 R >>endobj\n",
            "2 0 obj<< /Type /Pages /Kids [3 0 R] /Count 1 >>endobj\n",
            "3 0 obj<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources<< /Font<< /F1 5 0 R >> >> >>endobj\n",
            $"4 0 obj<< /Length {streamBytes.Length} >>stream\n{stream}\nendstream\nendobj\n",
            "5 0 obj<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>endobj\n"
        };

        var header = "%PDF-1.4\n";
        var body = string.Concat(objects);
        var offsets = new List<int>();
        var cursor = header.Length;
        foreach (var obj in objects)
        {
            offsets.Add(cursor);
            cursor += obj.Length;
        }

        var xrefPos = cursor;
        var xref = $"xref\n0 {objects.Count + 1}\n0000000000 65535 f \n";
        foreach (var offset in offsets)
        {
            xref += $"{offset:D10} 00000 n \n";
        }

        var trailer =
            $"trailer<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xrefPos}\n%%EOF\n";

        return System.Text.Encoding.ASCII.GetBytes(header + body + xref + trailer);
    }

    public static byte[] CreateSimpleDocx(string text)
    {
        using var ms = new MemoryStream();
        using (var document = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document, true))
        {
            var mainPart = document.AddMainDocumentPart();
            mainPart.Document = new Document(
                new Body(
                    new Paragraph(
                        new Run(
                            new Text(text) { Space = SpaceProcessingModeValues.Preserve }))));
            mainPart.Document.Save();
        }

        return ms.ToArray();
    }

    public static string FixturesDirectory
    {
        get
        {
            var dir = Path.Combine(AppContext.BaseDirectory, "Fixtures");
            Directory.CreateDirectory(dir);
            return dir;
        }
    }

    public static string EnsurePdfFixture(string fileName, string text)
    {
        var path = Path.Combine(FixturesDirectory, fileName);
        File.WriteAllBytes(path, CreateSimplePdf(text));
        return path;
    }

    public static string EnsureDocxFixture(string fileName, string text)
    {
        var path = Path.Combine(FixturesDirectory, fileName);
        File.WriteAllBytes(path, CreateSimpleDocx(text));
        return path;
    }
}
