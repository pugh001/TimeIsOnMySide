namespace Overtime.Service;

public interface IAdminTokenService
{
    string GenerateToken(Guid userId);
    bool ValidateToken(string token, Guid userId);
}
