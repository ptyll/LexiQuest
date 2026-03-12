using FluentValidation;
using LexiQuest.Shared.DTOs.Users;
using Microsoft.Extensions.Localization;

namespace LexiQuest.Api.Validators;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithMessage(localizer["Validation.CurrentPassword.Required"]);

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage(localizer["Validation.NewPassword.Required"])
            .MinimumLength(8)
            .WithMessage(localizer["Validation.Password.MinLength"])
            .Matches("[A-Z]")
            .WithMessage(localizer["Validation.Password.Uppercase"])
            .Matches("[a-z]")
            .WithMessage(localizer["Validation.Password.Lowercase"])
            .Matches("[0-9]")
            .WithMessage(localizer["Validation.Password.Digit"])
            .Matches("[^a-zA-Z0-9]")
            .WithMessage(localizer["Validation.Password.Special"]);

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.NewPassword)
            .WithMessage(localizer["Validation.ConfirmPassword.Mismatch"]);
    }
}
