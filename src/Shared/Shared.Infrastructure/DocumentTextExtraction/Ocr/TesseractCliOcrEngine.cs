using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Abstractions.DocumentTextExtraction;
using Shared.Infrastructure.DocumentTextExtraction.Options;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace Shared.Infrastructure.DocumentTextExtraction.Ocr;

/// <summary>
/// OCR via the system <c>tesseract</c> CLI (cross-platform when Tesseract is installed).
/// Avoids the Windows-only native binaries shipped with the Tesseract NuGet package.
/// </summary>
public sealed class TesseractCliOcrEngine : IOcrEngine
{
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".tif", ".tiff", ".bmp", ".gif", ".webp"
    };

    private readonly DocumentTextExtractionOptions _options;
    private readonly ILogger<TesseractCliOcrEngine> _logger;
    private readonly Lazy<bool> _available;

    public TesseractCliOcrEngine(
        IOptions<DocumentTextExtractionOptions> options,
        ILogger<TesseractCliOcrEngine> logger)
    {
        _options = options.Value;
        _logger = logger;
        _available = new Lazy<bool>(DetectAvailability);
    }

    public string EngineName => "TesseractCli";

    public bool IsAvailable => _available.Value;

    public async Task<string?> ExtractFromImageAsync(
        Stream imageStream,
        CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
        {
            return null;
        }

        var bytes = await ReadAllBytesAsync(imageStream, cancellationToken).ConfigureAwait(false);
        if (bytes.Length == 0)
        {
            return null;
        }

        var extension = GuessImageExtension(bytes);
        return await RunTesseractOnBytesAsync(bytes, extension, cancellationToken).ConfigureAwait(false);
    }

    public async Task<string?> ExtractFromPdfAsync(
        Stream pdfStream,
        CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
        {
            return null;
        }

        try
        {
            var parts = new List<string>();
            using var document = PdfDocument.Open(pdfStream);

            foreach (Page page in document.GetPages())
            {
                cancellationToken.ThrowIfCancellationRequested();

                foreach (var image in page.GetImages())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    byte[]? raw = null;
                    try
                    {
                        if (image.TryGetPng(out var png))
                        {
                            raw = png;
                        }
                        else
                        {
                            raw = image.RawBytes.ToArray();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Skipping PDF embedded image that could not be decoded.");
                        continue;
                    }

                    if (raw is null || raw.Length == 0)
                    {
                        continue;
                    }

                    var extension = GuessImageExtension(raw);
                    var text = await RunTesseractOnBytesAsync(raw, extension, cancellationToken)
                        .ConfigureAwait(false);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        parts.Add(text.Trim());
                    }
                }
            }

            if (parts.Count == 0)
            {
                return null;
            }

            return string.Join(Environment.NewLine, parts);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "PDF OCR via embedded images failed.");
            return null;
        }
    }

    internal static bool IsLikelyImage(string? fileName, string? contentType)
    {
        if (!string.IsNullOrWhiteSpace(contentType) &&
            contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        var ext = Path.GetExtension(fileName);
        return ImageExtensions.Contains(ext);
    }

    private async Task<string?> RunTesseractOnBytesAsync(
        byte[] bytes,
        string extension,
        CancellationToken cancellationToken)
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"phc-ocr-{Guid.NewGuid():N}{extension}");
        try
        {
            await File.WriteAllBytesAsync(tempFile, bytes, cancellationToken).ConfigureAwait(false);

            var exe = string.IsNullOrWhiteSpace(_options.TesseractExecutablePath)
                ? "tesseract"
                : _options.TesseractExecutablePath!;

            var psi = new ProcessStartInfo
            {
                FileName = exe,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8
            };

            psi.ArgumentList.Add(tempFile);
            psi.ArgumentList.Add("stdout");
            psi.ArgumentList.Add("-l");
            psi.ArgumentList.Add(string.IsNullOrWhiteSpace(_options.OcrLanguage) ? "eng" : _options.OcrLanguage);

            if (!string.IsNullOrWhiteSpace(_options.TessDataPath))
            {
                psi.Environment["TESSDATA_PREFIX"] = _options.TessDataPath;
            }

            using var process = new Process { StartInfo = psi };
            if (!process.Start())
            {
                return null;
            }

            var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Max(5, _options.OcrTimeoutSeconds)));

            try
            {
                await process.WaitForExitAsync(timeoutCts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                TryKill(process);
                _logger.LogWarning("Tesseract timed out after {Seconds}s.", _options.OcrTimeoutSeconds);
                return null;
            }

            var stdout = await stdoutTask.ConfigureAwait(false);
            var stderr = await stderrTask.ConfigureAwait(false);

            if (process.ExitCode != 0)
            {
                _logger.LogWarning(
                    "Tesseract exited with code {Code}. stderr: {Stderr}",
                    process.ExitCode,
                    Truncate(stderr));
                return null;
            }

            var text = stdout?.Trim();
            return string.IsNullOrEmpty(text) ? null : text;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Tesseract CLI OCR failed.");
            return null;
        }
        finally
        {
            TryDelete(tempFile);
        }
    }

    private bool DetectAvailability()
    {
        try
        {
            var exe = string.IsNullOrWhiteSpace(_options.TesseractExecutablePath)
                ? "tesseract"
                : _options.TesseractExecutablePath!;

            var psi = new ProcessStartInfo
            {
                FileName = exe,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            psi.ArgumentList.Add("--version");

            using var process = Process.Start(psi);
            if (process is null)
            {
                return false;
            }

            if (!process.WaitForExit(5000))
            {
                TryKill(process);
                return false;
            }

            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "Tesseract CLI is not available on this host.");
            return false;
        }
    }

    private static async Task<byte[]> ReadAllBytesAsync(Stream stream, CancellationToken cancellationToken)
    {
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
        return ms.ToArray();
    }

    private static string GuessImageExtension(byte[] bytes)
    {
        if (bytes.Length >= 8 &&
            bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
        {
            return ".png";
        }

        if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
        {
            return ".jpg";
        }

        if (bytes.Length >= 4 &&
            ((bytes[0] == 0x49 && bytes[1] == 0x49) || (bytes[0] == 0x4D && bytes[1] == 0x4D)))
        {
            return ".tiff";
        }

        if (bytes.Length >= 2 && bytes[0] == 0x42 && bytes[1] == 0x4D)
        {
            return ".bmp";
        }

        return ".png";
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch
        {
            // ignored
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // ignored
        }
    }

    private static string Truncate(string? value, int max = 400)
        => string.IsNullOrEmpty(value) ? string.Empty
            : value.Length <= max ? value
            : value[..max] + "...";
}
