using LexiQuest.Core.Domain.Enums;

namespace LexiQuest.Core.Interfaces.Services;

public interface IPremiumFeatureService
{
    Task<bool> HasFeatureAsync(Guid userId, PremiumFeature feature);
    Task<bool> IsPremiumAsync(Guid userId);
}
