using BudgetTracker.Api.DTOs;
using BudgetTracker.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class BudgetsController : AuthenticatedControllerBase
{
    private readonly BudgetService _budgetService;

    public BudgetsController(BudgetService budgetService)
    {
        _budgetService = budgetService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BudgetResponseDto>>> GetBudgets()
    {
        var budgets = await _budgetService.GetBudgetsAsync(CurrentUserId);
        return Ok(budgets);
    }

    [HttpPost]
    public async Task<ActionResult<BudgetResponseDto>> CreateBudget(BudgetCreateDto dto)
    {
        var result = await _budgetService.CreateBudgetAsync(CurrentUserId, dto);
        return this.ToActionResult(result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<BudgetResponseDto>> UpdateBudget(int id, BudgetUpdateDto dto)
    {
        var result = await _budgetService.UpdateBudgetAsync(CurrentUserId, id, dto);
        return this.ToActionResult(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteBudget(int id)
    {
        var result = await _budgetService.DeleteBudgetAsync(CurrentUserId, id);
        return this.ToActionResult(result);
    }

    [HttpGet("progress")]
    public async Task<ActionResult<IEnumerable<BudgetProgressDto>>> GetBudgetProgress([FromQuery] string? period = "current-month")
    {
        var result = await _budgetService.GetBudgetProgressAsync(CurrentUserId, period);
        return this.ToActionResult(result);
    }
}
