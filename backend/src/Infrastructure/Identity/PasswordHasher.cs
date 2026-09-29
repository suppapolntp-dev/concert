namespace TicketWorld.Infrastructure.Identity;

using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using TicketWorld.Application.Common.Interfaces;

/// <summary>
/// Cryptographically secure password hasher using PBKDF2 with HMAC-SHA512 and salt.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16; // 128 bit
    private const int HashSize = 32; // 256 bit
    private const int IterationCount = 100_000;

    public string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA512,
            iterationCount: IterationCount,
            numBytesRequested: HashSize);

        var outputBytes = new byte[1 + SaltSize + HashSize];
        outputBytes[0] = 0x01; // Format version
        Buffer.BlockCopy(salt, 0, outputBytes, 1, SaltSize);
        Buffer.BlockCopy(hash, 0, outputBytes, 1 + SaltSize, HashSize);

        return Convert.ToBase64String(outputBytes);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        try
        {
            var decoded = Convert.FromBase64String(passwordHash);
            if (decoded.Length != (1 + SaltSize + HashSize) || decoded[0] != 0x01)
                return false;

            var salt = new byte[SaltSize];
            Buffer.BlockCopy(decoded, 1, salt, 0, SaltSize);

            var expectedHash = new byte[HashSize];
            Buffer.BlockCopy(decoded, 1 + SaltSize, expectedHash, 0, HashSize);

            var actualHash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: IterationCount,
                numBytesRequested: HashSize);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch
        {
            return false;
        }
    }
}
