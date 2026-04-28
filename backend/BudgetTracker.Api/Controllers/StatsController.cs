using BudgetTracker.Api.DTOs;
using BudgetTracker.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class StatsController : AuthenticatedControllerBase
{
    private readonly StatsService _statsService;

    public StatsController(StatsService statsService)
    {
        _statsService = statsService;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<SummaryDto>> GetSummary([FromQuery] string? period = null)
    {
        var result = await _statsService.GetSummaryAsync(CurrentUserId, period);
        return this.ToActionResult(result);
    }

    [HttpGet("by-category")]
    public async Task<ActionResult<IEnumerable<CategoryStatsDto>>> GetByCategory(
        [FromQuery] string? period = null,
        [FromQuery] string? type = null)
    {
        var result = await _statsService.GetByCategoryAsync(CurrentUserId, period, type);
        return this.ToActionResult(result);
    }

    [HttpGet("latest-transactions")]
    public async Task<ActionResult<IEnumerable<TransactionResponseDto>>> GetLatestTransactions([FromQuery] int limit = 5)
    {
        var transactions = await _statsService.GetLatestTransactionsAsync(CurrentUserId, limit);
        return Ok(transactions);
    }
}
