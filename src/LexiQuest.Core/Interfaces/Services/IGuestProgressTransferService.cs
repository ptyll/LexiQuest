using LexiQuest.Shared.DTOs.Game;

namespace LexiQuest.Core.Interfaces.Services;

public interface IGuestProgressTransferService
{
    string CreateTransferToken(GuestSessionProgress progress);

    GuestSessionProgress? ConsumeTransferToken(string? token);
}
