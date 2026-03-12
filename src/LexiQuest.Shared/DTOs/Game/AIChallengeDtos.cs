using LexiQuest.Shared.Enums;

namespace LexiQuest.Shared.DTOs.Game;

public record AIChallengeRequest(AIChallengeType Type);

public record AIChallengeDto(
    AIChallengeType Type,
    string Title,
    string Description,
    List<AIChallengeWordDto> Words,
    double PredictedDifficulty);

public record AIChallengeWordDto(
    string Word,
    double PredictedDifficulty,
    string Reason);

public record PlayerAnalysisDto(
    List<WeakLetterDto> WeakLetters,
    List<CategoryPerformanceDto> CategoryPerformance,
    List<string> Tips);

public record WeakLetterDto(char Letter, double ErrorRate);

public record CategoryPerformanceDto(
    string Category,
    double SuccessRate,
    double AvgTimeSeconds);
