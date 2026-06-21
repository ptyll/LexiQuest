using LexiQuest.Shared.DTOs.Game;

namespace LexiQuest.Core.Interfaces.Services;

public interface IBossGameService
{
    Task<BossSessionDto> StartBossGameAsync(
        Guid userId,
        BossStartRequest request,
        CancellationToken cancellationToken = default);

    Task<BossSessionDto?> GetBossStateAsync(
        Guid userId,
        Guid sessionId,
        CancellationToken cancellationToken = default);

    Task<BossRoundResultDto> SubmitAnswerAsync(
        Guid userId,
        Guid sessionId,
        BossAnswerRequest request,
        CancellationToken cancellationToken = default);

    Task<TwistRevealStateDto?> GetTwistRevealStateAsync(
        Guid userId,
        Guid sessionId,
        CancellationToken cancellationToken = default);
}
