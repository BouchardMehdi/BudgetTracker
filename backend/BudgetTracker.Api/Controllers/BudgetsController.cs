using BudgetTracker.Api.Data;
using BudgetTracker.Api.DTOs;
using BudgetTracker.Api.Models;
using BudgetTracker.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class BudgetsController : AuthenticatedControllerBase
{
    private readonly BudgetTrackerDbContext _context;
    private readonly RecurringTransactionService _recurringTransactionService;

    public BudgetsController(BudgetTrackerDbContext context, RecurringTransactionService recurringTransactionService)
    {
        _context = context;
        _recurringTransactionService = recurringTransactionService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BudgetResponseDto>>> GetBudgets()
    {
        var budgets = await _context.Budgets
            .Include(budget => budget.Category)
            .Where(budget => budget.UserId == CurrentUserId)
            .OrderBy(budget => budget.Category!.Name)
            .Select(budget => ToResponseDto(budget))
            .ToListAsync();

        return Ok(budgets);
    }

    [HttpPost]
    public async Task<ActionResult<BudgetResponseDto>> CreateBudget(BudgetCreateDto dto)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(item => item.Id == dto.CategoryId && item.UserId == CurrentUserId);

        if (category is null)
        {
            return BadRequest("Category does not exist.");
        }

        if (category.Type != TransactionTypes.Expense)
        {
            return BadRequest("Budgets can only be created for expense categories.");
        }

        var alreadyExists = await _context.Budgets
            .AnyAsync(item => item.CategoryId == dto.CategoryId && item.UserId == CurrentUserId);

        if (alreadyExists)
        {
            return Conflict("A budget already exists for this category.");
        }

        var budget = new Budget
        {
            Amount = dto.Amount,
            CategoryId = dto.CategoryId,
            UserId = CurrentUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync();

        budget.Category = category;

        return CreatedAtAction(nameof(GetBudgets), new { id = budget.Id }, ToResponseDto(budget));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<BudgetResponseDto>> UpdateBudget(int id, BudgetUpdateDto dto)
    {
        var budget = await _context.Budgets
            .Include(item => item.Category)
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == CurrentUserId);

        if (budget is null)
        {
            return NotFound();
        }

        budget.Amount = dto.Amount;
        budget.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(ToResponseDto(budget));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteBudget(int id)
    {
        var budget = await _context.Budgets
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == CurrentUserId);

        if (budget is null)
        {
            return NotFound();
        }

        _context.Budgets.Remove(budget);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("progress")]
    public async Task<ActionResult<IEnumerable<BudgetProgressDto>>> GetBudgetProgress([FromQuery] string? period = "current-month")
    {
        await _recurringTransactionService.GenerateDueOccurrencesAsync(CurrentUserId);

        if (!IsValidPeriod(period))
        {
            return BadRequest("Period must be one of: current-month, previous-month, current-year.");
        }

        var (startDate, endDate, multiplier) = GetPeriodRange(period);

        var spentByCategory = await _context.Transactions
            .Where(transaction =>
                transaction.UserId == CurrentUserId &&
                transaction.Type == TransactionTypes.Expense &&
                transaction.TransactionDate >= startDate &&
                transaction.TransactionDate < endDate)
            .GroupBy(transaction => transaction.CategoryId)
            .Select(group => new
            {
                CategoryId = group.Key,
                Total = group.Sum(transaction => transaction.Amount)
            })
            .ToDictionaryAsync(item => item.CategoryId, item => item.Total);

        var budgets = await _context.Budgets
            .Include(budget => budget.Category)
            .Where(budget => budget.UserId == CurrentUserId)
            .OrderBy(budget => budget.Category!.Name)
            .ToListAsync();

        var progress = budgets.Select(budget =>
        {
            var budgetAmount = budget.Amount * multiplier;
            var spentAmount = spentByCategory.GetValueOrDefault(budget.CategoryId, 0);
            var progressPercent = budgetAmount <= 0 ? 0 : Math.Round(spentAmount / budgetAmount * 100, 1);

            return new BudgetProgressDto
            {
                BudgetId = budget.Id,
                CategoryId = budget.CategoryId,
                CategoryName = budget.Category?.Name ?? string.Empty,
                BudgetAmount = budgetAmount,
                SpentAmount = spentAmount,
                RemainingAmount = budgetAmount - spentAmount,
                ProgressPercent = progressPercent
            };
        });

        return Ok(progress);
    }

    private static BudgetResponseDto ToResponseDto(Budget budget)
    {
        return new BudgetResponseDto
        {
            Id = budget.Id,
            CategoryId = budget.CategoryId,
            CategoryName = budget.Category?.Name ?? string.Empty,
            Amount = budget.Amount,
            CreatedAt = budget.CreatedAt,
            UpdatedAt = budget.UpdatedAt
        };
    }

    private static bool IsValidPeriod(string? period)
    {
        var normalizedPeriod = period?.Trim().ToLowerInvariant();
        return string.IsNullOrWhiteSpace(normalizedPeriod) ||
            normalizedPeriod is "current-month" or "previous-month" or "current-year";
    }

    private static (DateTime StartDate, DateTime EndDate, int Multiplier) GetPeriodRange(string? period)
    {
        var today = DateTime.UtcNow.Date;
        var currentMonthStart = new DateTime(today.Year, today.Month, 1);

        return period?.Trim().ToLowerInvariant() switch
        {
            "previous-month" => (currentMonthStart.AddMonths(-1), currentMonthStart, 1),
            "current-year" => (new DateTime(today.Year, 1, 1), new DateTime(today.Year + 1, 1, 1), today.Month),
            _ => (currentMonthStart, currentMonthStart.AddMonths(1), 1)
        };
    }
}
