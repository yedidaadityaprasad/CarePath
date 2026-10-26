using System.Security.Cryptography;

namespace CarePath.Infrastructure;

/// <summary>
/// Generates short, human-readable reference numbers for clinical and billing records.
/// .NET 10: Uses RandomNumberGenerator.GetInt32 (cryptographically secure, no allocation).
/// </summary>
public static class ReferenceNumbers
{
    // Alphabet excludes visually ambiguous characters (0, O, 1, I)
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static string Next(string prefix, DateTime at)
    {
        Span<char> suffix = stackalloc char[6];
        for (var i = 0; i < suffix.Length; i++)
        {
            suffix[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
        }
        return $"{prefix}-{at:yyMMdd}-{new string(suffix)}";
    }
}
