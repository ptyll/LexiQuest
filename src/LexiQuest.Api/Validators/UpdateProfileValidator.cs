using FluentValidation;
using LexiQuest.Shared.DTOs.Users;
using Microsoft.Extensions.Localization;

namespace LexiQuest.Api.Validators;

public class UpdateProfileValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage(localizer["Validation.Username.Required"])
            .MinimumLength(3)
            .WithMessage(localizer["Validation.Username.MinLength"])
            .MaximumLength(50)
            .WithMessage(localizer["Validation.Username.MaxLength"])
            .Matches("^[a-zA-Z0-9_]+$")
            .WithMessage(localizer["Validation.Username.InvalidCharacters"]);

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(localizer["Validation.Email.Required"])
            .EmailAddress()
            .WithMessage(localizer["Validation.Email.Invalid"]);
    }
}
