using LexiQuest.Api.Extensions;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Dictionaries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LexiQuest.Api.Controllers;

[ApiController]
[Route("api/dictionaries")]
[Authorize]
public class DictionaryController : ControllerBase
{
    private readonly IDictionaryService _dictionaryService;

    public DictionaryController(IDictionaryService dictionaryService)
    {
        _dictionaryService = dictionaryService;
    }

    [HttpGet("my")]
    public async Task<ActionResult<List<DictionaryDto>>> GetMyDictionaries()
    {
        var userId = User.GetUserId();
        var dictionaries = await _dictionaryService.GetUserDictionariesAsync(userId);
        return Ok(dictionaries);
    }

    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<ActionResult<List<DictionaryDto>>> GetPublicDictionaries()
    {
        var dictionaries = await _dictionaryService.GetPublicDictionariesAsync();
        return Ok(dictionaries);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DictionaryDto>> GetDictionary(Guid id)
    {
        var userId = User.GetUserId();
        var dictionary = await _dictionaryService.GetDictionaryByIdAsync(id, userId);
        
        if (dictionary == null)
            return NotFound();

        return Ok(dictionary);
    }

    [HttpPost]
    public async Task<ActionResult<DictionaryDto>> CreateDictionary(CreateDictionaryRequest request)
    {
        var userId = User.GetUserId();
        var dictionary = await _dictionaryService.CreateDictionaryAsync(userId, request);
        return CreatedAtAction(nameof(GetDictionary), new { id = dictionary.Id }, dictionary);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteDictionary(Guid id)
    {
        var userId = User.GetUserId();
        var success = await _dictionaryService.DeleteDictionaryAsync(id, userId);
        
        if (!success)
            return Forbid();

        return NoContent();
    }

    [HttpPost("{id:guid}/words")]
    public async Task<ActionResult<DictionaryWordDto>> AddWord(Guid id, AddWordRequest request)
    {
        var userId = User.GetUserId();
        
        try
        {
            var word = await _dictionaryService.AddWordAsync(id, userId, request);
            return CreatedAtAction(nameof(GetDictionary), new { id }, word);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost("{id:guid}/import-csv")]
    public async Task<ActionResult<ImportResultDto>> ImportCsv(Guid id, [FromBody] ImportContentRequest request)
    {
        var userId = User.GetUserId();
        
        try
        {
            var result = await _dictionaryService.ImportWordsFromCsvAsync(id, userId, request.Content);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost("{id:guid}/import-txt")]
    public async Task<ActionResult<ImportResultDto>> ImportTxt(Guid id, [FromBody] ImportContentRequest request)
    {
        var userId = User.GetUserId();
        
        try
        {
            var result = await _dictionaryService.ImportWordsFromTxtAsync(id, userId, request.Content);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost("{id:guid}/import-json")]
    public async Task<ActionResult<ImportResultDto>> ImportJson(Guid id, [FromBody] ImportContentRequest request)
    {
        var userId = User.GetUserId();
        
        try
        {
            var result = await _dictionaryService.ImportWordsFromJsonAsync(id, userId, request.Content);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}

public record ImportContentRequest(string Content);
