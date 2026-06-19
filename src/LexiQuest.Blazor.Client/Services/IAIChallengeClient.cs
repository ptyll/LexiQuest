using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Blazor.Services;

public interface IAIChallengeClient
{
    Task<PlayerAnalysisDto?> GetAnalysisAsync();
    Task<AIChallengeDto?> GenerateChallengeAsync(AIChallengeType type);
}
