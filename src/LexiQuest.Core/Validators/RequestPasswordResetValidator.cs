using FluentValidation;
using LexiQuest.Shared.DTOs.Auth;
using Microsoft.Extensions.Localization;

namespace LexiQuest.Core.Validators;

public class RequestPasswordResetValidator : AbstractValidator<RequestPasswordResetDto>
{
    public RequestPasswordResetValidator(IStringLocalizer<RequestPasswordResetValidator> localizer)
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(localizer["Validation.Email.Required"])
            .EmailAddress()
            .WithMessage(localizer["Validation.Email.Invalid"]);
    }
}
