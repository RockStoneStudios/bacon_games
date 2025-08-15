// En Challenge/Services/IJwtService.cs
namespace Challenge.Services;

public interface IJwtService
{
    string GenerateToken(string email, string userId);
}