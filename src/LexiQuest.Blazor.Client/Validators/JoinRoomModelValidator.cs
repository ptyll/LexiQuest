using FluentValidation;
using LexiQuest.Blazor.Models;
using Microsoft.Extensions.Localization;

namespace LexiQuest.Blazor.Validators;

/// <summary>
/// Validator for JoinRoomModel.
/// </summary>
public class JoinRoomModelValidator : AbstractValidator<JoinRoomModel>
{
    public JoinRoomModelValidator(IStringLocalizer<Pages.Multiplayer> localizer)
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage(localizer["Validation_RoomCode_Required"]);

        RuleFor(x => x.Code)
            .Matches(@"^LEXIQ-[A-Z0-9]{4}$")
            .WithMessage(localizer["Validation_RoomCode_InvalidFormat"]);
    }
}
