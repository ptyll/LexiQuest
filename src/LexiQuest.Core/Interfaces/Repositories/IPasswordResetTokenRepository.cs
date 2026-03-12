using LexiQuest.Core.Domain.Entities;

namespace LexiQuest.Core.Interfaces.Repositories;

public interface IPasswordResetTokenRepository
{
    Task<PasswordResetToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default);
    Task<List<PasswordResetToken>> GetExpiredTokensAsync(CancellationToken cancellationToken = default);
    Task DeleteAsync(PasswordResetToken token, CancellationToken cancellationToken = default);
}
