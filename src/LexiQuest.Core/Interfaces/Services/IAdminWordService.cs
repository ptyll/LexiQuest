using LexiQuest.Shared.DTOs.Admin;

namespace LexiQuest.Core.Interfaces.Services;

public interface IAdminWordService
{
    Task<PaginatedResult<AdminWordDto>> GetWordsAsync(AdminWordListRequest request, CancellationToken cancellationToken = default);
    Task<AdminWordDto> CreateWordAsync(AdminWordCreateRequest request, CancellationToken cancellationToken = default);
    Task<AdminWordDto?> UpdateWordAsync(Guid id, AdminWordUpdateRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteWordAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BulkImportResult> BulkImportAsync(string csvContent, CancellationToken cancellationToken = default);
    Task<string> ExportAsync(CancellationToken cancellationToken = default);
    Task<WordStatsDto> GetStatsAsync(CancellationToken cancellationToken = default);
}
