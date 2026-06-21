using FluentValidation;
using LexiQuest.Shared.DTOs.Multiplayer;

namespace LexiQuest.Core.Validators;

/// <summary>
/// Validator for room settings when creating a private room.
/// </summary>
public class RoomSettingsValidator : AbstractValidator<RoomSettingsDto>
{
    public RoomSettingsValidator()
    {
        RuleFor(x => x.WordCount)
            .Must(BeValidWordCount)
            .WithMessage("Počet slov musí být 10, 15 nebo 20.");

        RuleFor(x => x.TimeLimitMinutes)
            .Must(BeValidTimeLimit)
            .WithMessage("Časový limit musí být 2, 3 nebo 5 minut.");

        RuleFor(x => x.BestOf)
            .Must(BeValidBestOf)
            .WithMessage("Série musí být na 1, 3 nebo 5 her.");

        RuleFor(x => x.Difficulty)
            .IsInEnum()
            .WithMessage("Neplatná obtížnost.");
    }

    private static bool BeValidWordCount(int wordCount) =>
        wordCount is 10 or 15 or 20;

    private static bool BeValidTimeLimit(int timeLimit) =>
        timeLimit is 2 or 3 or 5;

    private static bool BeValidBestOf(int bestOf) =>
        bestOf is 1 or 3 or 5;
}
