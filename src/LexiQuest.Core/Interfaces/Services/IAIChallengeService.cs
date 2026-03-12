using LexiQuest.Shared.DTOs.Game;

namespace LexiQuest.Core.Interfaces.Services;

public interface IAIChallengeService
{
    Task<PlayerAnalysisDto> AnalyzePlayerAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<AIChallengeDto> GenerateChallengeAsync(Guid userId, AIChallengeRequest request, CancellationToken cancellationToken = default);
}
