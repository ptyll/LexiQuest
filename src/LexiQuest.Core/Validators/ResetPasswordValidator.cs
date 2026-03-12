using FluentValidation;
using LexiQuest.Shared.DTOs.Auth;
using Microsoft.Extensions.Localization;

namespace LexiQuest.Core.Validators;

public class ResetPasswordValidator : AbstractValidator<ResetPasswordDto>
{
    public ResetPasswordValidator(IStringLocalizer<ResetPasswordValidator> localizer)
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage(localizer["Validation.Token.Required"]);

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage(localizer["Validation.Password.Required"])
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
            .NotEmpty()
            .WithMessage(localizer["Validation.ConfirmPassword.Required"])
            .Equal(x => x.NewPassword)
            .WithMessage(localizer["Validation.ConfirmPassword.Mismatch"]);
    }
}
