using FluentValidation;
using LexiQuest.Shared.DTOs.Auth;
using Microsoft.Extensions.Localization;

namespace LexiQuest.Core.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator(IStringLocalizer<LoginRequestValidator> localizer)
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(localizer["Validation.Email.Required"])
            .EmailAddress()
            .WithMessage(localizer["Validation.Email.Invalid"]);

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage(localizer["Validation.Password.Required"]);
    }
}
