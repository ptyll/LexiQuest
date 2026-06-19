using LexiQuest.Shared.DTOs.Game;

namespace LexiQuest.Blazor.Services;

public interface IPathService
{
    Task<List<LearningPathDto>> GetPathsAsync();
    Task<PathProgressDto?> GetPathProgressAsync(Guid pathId);
}
