using FluentValidation;
using LexiQuest.Shared.DTOs.Teams;
using Microsoft.Extensions.Localization;

namespace LexiQuest.Core.Validators;

/// <summary>
/// Validator for CreateTeamRequest.
/// </summary>
public class CreateTeamValidator : AbstractValidator<CreateTeamRequest>
{
    public CreateTeamValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(localizer["Validation.Team.Name.Required"])
            .Length(3, 30).WithMessage(localizer["Validation.Team.Name.Length"]);

        RuleFor(x => x.Tag)
            .NotEmpty().WithMessage(localizer["Validation.Team.Tag.Required"])
            .Length(2, 4).WithMessage(localizer["Validation.Team.Tag.Length"])
            .Matches("^[A-Z0-9]+$").WithMessage(localizer["Validation.Team.Tag.Format"]);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(localizer["Validation.Team.Description.MaxLength"])
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
