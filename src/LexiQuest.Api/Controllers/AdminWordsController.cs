using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LexiQuest.Api.Controllers;

[ApiController]
[Route("api/v1/admin/words")]
[Authorize(Roles = "Admin,ContentManager")]
public class AdminWordsController : ControllerBase
{
    private readonly IAdminWordService _adminWordService;

    public AdminWordsController(IAdminWordService adminWordService)
    {
        _adminWordService = adminWordService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<AdminWordDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<AdminWordDto>>> GetWords(
        [FromQuery] string? search,
        [FromQuery] string? difficulty,
        [FromQuery] string? category,
        [FromQuery] int? minLength,
        [FromQuery] int? maxLength,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var request = new AdminWordListRequest(search, difficulty, category, minLength, maxLength, page, pageSize);
        var result = await _adminWordService.GetWordsAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AdminWordDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AdminWordDto>> CreateWord(AdminWordCreateRequest request, CancellationToken cancellationToken)
    {
        var word = await _adminWordService.CreateWordAsync(request, cancellationToken);
        return Created($"/api/v1/admin/words/{word.Id}", word);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AdminWordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminWordDto>> UpdateWord(Guid id, AdminWordUpdateRequest request, CancellationToken cancellationToken)
    {
        var word = await _adminWordService.UpdateWordAsync(id, request, cancellationToken);
        if (word == null) return NotFound();
        return Ok(word);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWord(Guid id, CancellationToken cancellationToken)
    {
        var success = await _adminWordService.DeleteWordAsync(id, cancellationToken);
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpPost("import")]
    [ProducesResponseType(typeof(BulkImportResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<BulkImportResult>> ImportWords([FromBody] ImportRequest request, CancellationToken cancellationToken)
    {
        var result = await _adminWordService.BulkImportAsync(request.CsvContent, cancellationToken);
        return Ok(result);
    }

    [HttpGet("export")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportWords(CancellationToken cancellationToken)
    {
        var csv = await _adminWordService.ExportAsync(cancellationToken);
        return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "words-export.csv");
    }

    [HttpGet("stats")]
    [ProducesResponseType(typeof(WordStatsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<WordStatsDto>> GetStats(CancellationToken cancellationToken)
    {
        var stats = await _adminWordService.GetStatsAsync(cancellationToken);
        return Ok(stats);
    }
}

public record ImportRequest(string CsvContent);
