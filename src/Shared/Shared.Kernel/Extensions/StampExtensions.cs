using System.Text;
using System.Security.Cryptography;

namespace Shared.Kernel.Extensions;

public static class StampExtensions
{
    // ✅ Thread-safe: Cada thread tem sua própria instância
    [ThreadStatic]
    private static Random? _random;

    private static Random GetRandom()
    {
        if (_random == null)
        {
            // Seed único baseado em thread + timestamp para evitar colisões
            var seed = Environment.CurrentManagedThreadId ^ DateTime.UtcNow.Ticks.GetHashCode();
            _random = new Random(seed);
        }
        return _random;
    }

    public static string GenerateStamp(this int size, bool lowerCase = false)
    {
        var builder = new StringBuilder(size);
        char offset = lowerCase ? 'a' : 'A';
        const int lettersOffset = 26;
        var random = GetRandom();

        for (var i = 0; i < size; i++)
        {
            var @char = (char)random.Next(offset, offset + lettersOffset);
            builder.Append(@char);
        }

        return builder.ToString();
    }

    public static string GenerateStamp()
    {
        return DateTime.UtcNow.ToString("yyyyMMddHHmmss") + GenerateStamp(8, false);
    }

    public static string GenerateStamp(this int randomSize)
    {
        return  GenerateStamp(randomSize, false);
    }

    public static string GenerateRequestId(string prefix = "PHCAPI")
    {
        var uuid = Guid.NewGuid().ToString();
        return $"{prefix}{uuid[..21]}";
    }

    public static string GenerateCustomStamp(string prefix, int randomSize = 8, bool includeTimestamp = true)
    {
        var stamp = new StringBuilder();

        if (!string.IsNullOrEmpty(prefix))
            stamp.Append(prefix);

        if (includeTimestamp)
            stamp.Append(DateTime.UtcNow.ToString("yyyyMMddHHmmss"));

        if (randomSize > 0)
            stamp.Append(GenerateStamp(randomSize, false));

        return stamp.ToString();
    }
}
