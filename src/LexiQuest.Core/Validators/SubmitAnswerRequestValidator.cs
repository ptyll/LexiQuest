using FluentValidation;
using LexiQuest.Shared.DTOs.Game;
using Microsoft.Extensions.Localization;

namespace LexiQuest.Core.Validators;

/// <summary>
/// Validator for SubmitAnswerRequest.
/// </summary>
public class SubmitAnswerRequestValidator : AbstractValidator<SubmitAnswerRequest>
{
    public SubmitAnswerRequestValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.SessionId)
            .NotEmpty()
            .WithMessage(localizer["Validation.SessionId.Required"]);

        RuleFor(x => x.Answer)
            .NotEmpty()
            .WithMessage(localizer["Validation.Answer.Required"])
            .MaximumLength(50)
            .WithMessage(localizer["Validation.Answer.MaxLength"]);

        RuleFor(x => x.TimeSpentMs)
            .GreaterThanOrEqualTo(0)
            .WithMessage(localizer["Validation.TimeSpent.NonNegative"]);
    }
}
