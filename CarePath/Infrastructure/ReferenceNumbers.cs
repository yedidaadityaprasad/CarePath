using System.Security.Cryptography;

namespace CarePath.Infrastructure;

public static class ReferenceNumbers
{
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
