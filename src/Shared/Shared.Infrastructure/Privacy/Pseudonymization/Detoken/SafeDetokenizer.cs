using System.Text;
using Shared.Abstractions.Privacy.Pseudonymization;

namespace Shared.Infrastructure.Privacy.Pseudonymization.Detoken;

/// <summary>
/// Safe detokenization: lexer + session whitelist + type check. NOT naive string.Replace.
/// </summary>
public sealed class SafeDetokenizer : IDetokenizer
{
    private readonly ITokenMapAccessLogger _accessLogger;

    public SafeDetokenizer(ITokenMapAccessLogger accessLogger)
    {
        _accessLogger = accessLogger;
    }

    public async Task<DetokenizationResult> DetokenizeAsync(
        DetokenizationRequest request,
        ITokenMapStore map,
        Func<TokenMapEntry, string> decryptOriginal,
        PseudonymizationPolicy policy,
        CancellationToken cancellationToken = default)
    {
        var text = request.ModelResponseText ?? string.Empty;
        var rejected = new List<string>();
        var sb = new StringBuilder(text.Length);
        var last = 0;

        foreach (System.Text.RegularExpressions.Match m in TokenGrammar.TokenRegex.Matches(text))
        {
            sb.Append(text, last, m.Index - last);
            var token = m.Value;

            if (!TokenGrammar.TryParse(token, out var typeFromToken, out _))
            {
                rejected.Add(token);
                if (policy.StrictDetoken)
                    return Fail(rejected, $"Invalid token grammar: {token}");
                sb.Append("[REDACTED]");
                last = m.Index + m.Length;
                continue;
            }

            var entry = await map.GetByTokenAsync(
                request.SessionId,
                request.DocumentId,
                token,
                cancellationToken);

            if (entry is null)
            {
                rejected.Add(token);
                if (policy.StrictDetoken)
                    return Fail(rejected, $"Unknown or expired token: {token}");
                sb.Append("[REDACTED]");
                last = m.Index + m.Length;
                continue;
            }

            if (!string.Equals(entry.EntityType, typeFromToken, StringComparison.OrdinalIgnoreCase))
            {
                rejected.Add(token);
                if (policy.StrictDetoken)
                    return Fail(rejected, $"Type mismatch for token: {token}");
                sb.Append("[REDACTED]");
                last = m.Index + m.Length;
                continue;
            }

            await _accessLogger.LogAccessAsync(
                request.SessionId,
                request.DocumentId,
                token,
                "detoken.read",
                cancellationToken);

            string original;
            try
            {
                original = decryptOriginal(entry);
            }
            catch (Exception ex)
            {
                return new DetokenizationResult(
                    string.Empty,
                    rejected,
                    false,
                    $"Decrypt failed for {token}: {ex.Message}");
            }

            sb.Append(original);
            last = m.Index + m.Length;
        }

        sb.Append(text, last, text.Length - last);

        if (rejected.Count > 0 && policy.StrictDetoken)
            return Fail(rejected, "One or more tokens rejected");

        return new DetokenizationResult(sb.ToString(), rejected, true);
    }

    private static DetokenizationResult Fail(List<string> rejected, string reason) =>
        new(string.Empty, rejected, false, reason);
}
