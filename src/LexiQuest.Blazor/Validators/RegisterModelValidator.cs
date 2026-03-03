using FluentValidation;
using LexiQuest.Blazor.Models;
using Microsoft.Extensions.Localization;

namespace LexiQuest.Blazor.Validators;

public class RegisterModelValidator : AbstractValidator<RegisterModel>
{
    public RegisterModelValidator(IStringLocalizer<RegisterModelValidator> localizer)
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(localizer["Validation.Email.Required"])
            .EmailAddress()
            .WithMessage(localizer["Validation.Email.Invalid"]);

        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage(localizer["Validation.Username.Required"])
            .MinimumLength(3)
            .WithMessage(localizer["Validation.Username.MinLength"])
            .MaximumLength(30)
            .WithMessage(localizer["Validation.Username.MaxLength"])
            .Matches("^[a-zA-Z0-9_]+$")
            .WithMessage(localizer["Validation.Username.InvalidChars"]);

        RuleFor(x => x.Password)
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
            .Equal(x => x.Password)
            .WithMessage(localizer["Validation.Password.Mismatch"]);

        RuleFor(x => x.AcceptTerms)
            .Equal(true)
            .WithMessage(localizer["Validation.Terms.Required"]);
    }
}
