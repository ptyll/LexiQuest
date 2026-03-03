using FluentValidation;
using LexiQuest.Blazor.Models;
using Microsoft.Extensions.Localization;

namespace LexiQuest.Blazor.Validators;

public class LoginModelValidator : AbstractValidator<LoginModel>
{
    public LoginModelValidator(IStringLocalizer<LoginModelValidator> localizer)
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
