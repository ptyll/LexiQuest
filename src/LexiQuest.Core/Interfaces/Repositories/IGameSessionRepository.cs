using LexiQuest.Core.Domain.Entities;

namespace LexiQuest.Core.Interfaces.Repositories;

public interface IGameSessionRepository
{
    Task<IReadOnlyList<GameSession>> GetByUserIdWithRoundsAsync(
        Guid userId, int limit = 50, CancellationToken cancellationToken = default);
}
