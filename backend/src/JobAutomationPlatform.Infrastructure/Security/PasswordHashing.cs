using System.Security.Cryptography;

namespace JobAutomationPlatform.Infrastructure.Security;

public static class PasswordHashing
{
    private const int Iterations = 210_000;
    private const int SaltSize = 16;
    private const int SubkeySize = 32;

    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var subkey = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, SubkeySize);
        return $"PBKDF2$SHA256${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(subkey)}";
    }

    public static bool Verify(string password, string passwordHash)
    {
        var parts = passwordHash.Split('$');
        if (parts.Length != 5 || !string.Equals(parts[0], "PBKDF2", StringComparison.Ordinal) || !string.Equals(parts[1], "SHA256", StringComparison.Ordinal))
        {
            return false;
        }

        if (!int.TryParse(parts[2], out var iterations))
        {
            return false;
        }

        var salt = Convert.FromBase64String(parts[3]);
        var expectedSubkey = Convert.FromBase64String(parts[4]);
        var actualSubkey = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedSubkey.Length);
        return CryptographicOperations.FixedTimeEquals(expectedSubkey, actualSubkey);
    }
}
