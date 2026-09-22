namespace Shared.Abstractions.DocumentTextExtraction;

/// <summary>
/// How plain text was obtained from a document.
/// </summary>
public enum DocumentTextSource
{
    /// <summary>No usable text was extracted.</summary>
    None = 0,

    /// <summary>Native digital text (PDF text layer, DOCX body, etc.).</summary>
    Native = 1,

    /// <summary>Optical character recognition (images or weak/scanned PDFs).</summary>
    Ocr = 2
}
