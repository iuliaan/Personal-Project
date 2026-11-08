using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using ProjectLogistiXX.Domain.Interfaces;

namespace ProjectLogistiXX.Infrastructure.Security;

public sealed class PasswordHashOptions
{
    public int Iterations { get; init; } = 600_000;
    public HashAlgorithmName Algorithm { get; init; } = HashAlgorithmName.SHA512;
}
internal sealed class PasswordHash : IPasswordHash
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const char SegmentDelimiter = '.';
    private const int MinIterations = 100_000;

    private readonly PasswordHashOptions _options;

    public PasswordHash(IOptions<PasswordHashOptions> options)
    {
        _options = options.Value;
    }

    public string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            _options.Iterations,
            _options.Algorithm,
            KeySize);

        return string.Join(
            SegmentDelimiter,
            _options.Iterations,
            _options.Algorithm.Name,
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        if (!IsValidInput(hashedPassword, providedPassword))
        {
            return false;
        }

        if (!TryParseHash(hashedPassword, out var hashData))
        {
            return false;
        }

        var providedHash = GenerateHash(
            providedPassword,
            hashData!.Salt,
            hashData.Iterations,
            hashData.Algorithm,
            hashData.Hash.Length);


        return CryptographicOperations.FixedTimeEquals(providedHash, hashData.Hash);
    }

    private static bool IsValidInput(string hashedPassword, string providedPassword)
    {
        return !string.IsNullOrEmpty(hashedPassword) && !string.IsNullOrEmpty(providedPassword);
    }

    private static bool TryParseHash(string hashedPassword, out PasswordHashData? hashData)
    {
        hashData = null;
        var segments = hashedPassword.Split(SegmentDelimiter);
        if (segments.Length != 4)
        {
            return false;
        }
        if (!int.TryParse(segments[0], out var iterations))
        {
            return false;
        }

        var algorithm = new HashAlgorithmName(segments[1]);
        try
        {
            var salt = Convert.FromBase64String(segments[2]);
            var hash = Convert.FromBase64String(segments[3]);

            if (salt.Length == 0 || hash.Length == 0)
            {
                return false;
            }

            hashData = new PasswordHashData(iterations, algorithm, salt, hash);

            return true;
        }

        catch (FormatException)
        {
            return false;
        }
    }

    private static byte[] GenerateHash(
        string password,
        byte[] salt,
        int iterations,
        HashAlgorithmName algorithm,
        int hashLength)
    {
        return Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            algorithm,
            hashLength);
    }

    private sealed record PasswordHashData(
        int Iterations,
        HashAlgorithmName Algorithm,
        byte[] Salt,
        byte[] Hash);
}
