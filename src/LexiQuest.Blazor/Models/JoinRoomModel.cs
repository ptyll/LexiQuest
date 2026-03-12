using System.ComponentModel.DataAnnotations;

namespace LexiQuest.Blazor.Models;

/// <summary>
/// Model for joining a room.
/// </summary>
public class JoinRoomModel
{
    /// <summary>
    /// Room code in format LEXIQ-XXXX.
    /// </summary>
    [Required(ErrorMessage = "Kód místnosti je povinný")]
    [RegularExpression(@"^LEXIQ-[A-Z0-9]{4}$", ErrorMessage = "Kód musí být ve formátu LEXIQ-XXXX")]
    public string Code { get; set; } = "";
}
