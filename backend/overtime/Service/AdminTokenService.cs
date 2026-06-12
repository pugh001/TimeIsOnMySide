using System.Security.Cryptography;
using System.Text;

namespace Overtime.Service;

public sealed class AdminTokenService : IAdminTokenService
{
    private readonly byte[] _keyBytes;

    public AdminTokenService(string secret)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secret);
        _keyBytes = Encoding.UTF8.GetBytes(secret);
    }

    public string GenerateToken(Guid userId)
    {
        var message = BuildMessage(userId);
        var hash = HMACSHA256.HashData(_keyBytes, Encoding.UTF8.GetBytes(message));
        return Convert.ToBase64String(hash);
    }

    public bool ValidateToken(string token, Guid userId)
    {
        ArgumentNullException.ThrowIfNull(token);
        var expected = GenerateToken(userId);
        var expectedBytes = Convert.FromBase64String(expected);
        var tokenBuffer = new byte[expectedBytes.Length];
        if (!Convert.TryFromBase64String(token, tokenBuffer, out var written) || written != expectedBytes.Length)
            return false;
        return CryptographicOperations.FixedTimeEquals(tokenBuffer, expectedBytes);
    }

    private static string BuildMessage(Guid userId)
        => $"{DateOnly.FromDateTime(DateTime.UtcNow):yyyyMMdd}:{userId}";
}
