using LexiQuest.Core.Domain.Entities;

namespace LexiQuest.Core.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    Guid? ValidateAccessToken(string token);
}
