using FluentAssertions;
using Overtime.Service;

namespace Overtime.Tests.Unit.Service;

public sealed class AdminTokenServiceTests
{
    private static AdminTokenService Build(string secret = "test-secret-32-chars-long-enough!")
        => new(secret);

    [Fact]
    public void GenerateToken_SameDayAndUserId_ReturnsSameToken()
    {
        var service = Build();
        var userId = Guid.NewGuid();

        var token1 = service.GenerateToken(userId);
        var token2 = service.GenerateToken(userId);

        token1.Should().Be(token2);
    }

    [Fact]
    public void GenerateToken_DifferentUserIds_ReturnsDifferentTokens()
    {
        var service = Build();

        var token1 = service.GenerateToken(Guid.NewGuid());
        var token2 = service.GenerateToken(Guid.NewGuid());

        token1.Should().NotBe(token2);
    }

    [Fact]
    public void ValidateToken_TodaysToken_ReturnsTrue()
    {
        var service = Build();
        var userId = Guid.NewGuid();
        var token = service.GenerateToken(userId);

        var result = service.ValidateToken(token, userId);

        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateToken_WrongToken_ReturnsFalse()
    {
        var service = Build();
        var userId = Guid.NewGuid();

        var result = service.ValidateToken("not-a-real-token", userId);

        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateToken_TokenForDifferentUser_ReturnsFalse()
    {
        var service = Build();
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var token = service.GenerateToken(userId1);

        var result = service.ValidateToken(token, userId2);

        result.Should().BeFalse();
    }

    [Fact]
    public void Constructor_EmptySecret_Throws()
    {
        var act = () => new AdminTokenService(string.Empty);

        act.Should().Throw<ArgumentException>();
    }
}
