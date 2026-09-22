# Document text extraction (Shared)

Platform capability under `src/Shared/` so any module (Recruitment CV scoring, treasury attachments, etc.) can inject `IDocumentTextExtractor` and obtain plain text from PDF, DOCX, and images.

## Pattern

**Strategy + Composite**

1. **Native strategies** (`INativeDocumentTextExtractor`)
   - `PdfPigNativeTextExtractor` — digital PDF text layer via [UglyToad.PdfPig](https://github.com/UglyToad/PdfPig) `1.7.0-custom-5`
   - `DocxOpenXmlNativeTextExtractor` — DOCX body via DocumentFormat.OpenXml
2. **Optional OCR** (`IOcrEngine`)
   - `TesseractCliOcrEngine` — shells out to system `tesseract` (not the Tesseract NuGet natives, which are Windows-only)
3. **Composite** (`CompositeDocumentTextExtractor` implementing `IDocumentTextExtractor`)
   - Try matching native extractors
   - If significant character count &lt; `MinNativeCharacters` (or input is an image / weak PDF), fall back to OCR when enabled and available
   - Result reports `DocumentTextSource.Native` or `DocumentTextSource.Ocr`

## Why Tesseract CLI (v1)

The maintained `Tesseract` NuGet package ships `leptonica`/`tesseract` **Windows `.dll`** runtimes only. For Linux CI and Windows parity we use the **CLI** when `tesseract` is on `PATH` (or `TesseractExecutablePath`).

Install examples:

- Debian/Ubuntu: `sudo apt install tesseract-ocr tesseract-ocr-eng` (add `tesseract-ocr-por` for Portuguese)
- Windows: install from [UB Mannheim builds](https://github.com/UB-Mannheim/tesseract/wiki) or Chocolatey `tesseract`, then ensure `tesseract.exe` is on PATH

If Tesseract is missing, native PDF/DOCX extraction still works; OCR returns unavailable and the composite reports empty/weak native text.

Scanned PDF OCR uses **embedded page images** via PdfPig (no `pdftoppm` required). Fully rasterized PDFs without extractable images may need a future renderer.

## Configuration

`appsettings` section `DocumentTextExtraction`:

```json
{
  "DocumentTextExtraction": {
    "MinNativeCharacters": 40,
    "OcrEnabled": true,
    "OcrEngineName": "TesseractCli",
    "OcrLanguage": "eng",
    "TesseractExecutablePath": "",
    "TessDataPath": "",
    "OcrTimeoutSeconds": 60
  }
}
```

No API keys are required for the v1 engines.

## Host registration

```csharp
builder.Services.AddDocumentTextExtraction(builder.Configuration);
```

## Consumption (e.g. future Recruitment)

Reference `Shared.Abstractions`, inject `IDocumentTextExtractor`, call `ExtractAsync(stream, fileName, contentType)`. Branch on `result.Source` / `result.Success` — do not put CV scoring or Hangfire inside Shared.
