namespace cmsUserManagment.Application.Interfaces;

public interface IJwtTokenProvider
{
    string GenerateToken(string email, string id, bool isAdmin);
}
