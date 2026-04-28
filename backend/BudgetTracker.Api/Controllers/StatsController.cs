using BudgetTracker.Api.Data;
using BudgetTracker.Api.DTOs;
using BudgetTracker.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class StatsController : AuthenticatedControllerBase
{
    private readonly BudgetTrackerDbContext _context;

    public StatsController(BudgetTrackerDbContext context)
    {
        _context = context;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<SummaryDto>> GetSummary([FromQuery] string? period = null)
    {
        if (!IsValidPeriod(period))
        {
            return BadRequest("Period must be one of: all, current-month, previous-month, current-year.");
        }

        var transactions = ApplyPeriodFilter(GetUserTransactions(), period);

        var totalIncome = await transactions
            .Where(transaction => transaction.Type == TransactionTypes.Income)
            .SumAsync(transaction => (decimal?)transaction.Amount) ?? 0;

        var totalExpense = await transactions
            .Where(transaction => transaction.Type == TransactionTypes.Expense)
            .SumAsync(transaction => (decimal?)transaction.Amount) ?? 0;

        return Ok(new SummaryDto
        {
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            Balance = totalIncome - totalExpense
        });
    }

    [HttpGet("by-category")]
    public async Task<ActionResult<IEnumerable<CategoryStatsDto>>> GetByCategory(
        [FromQuery] string? period = null,
        [FromQuery] string? type = null)
    {
        if (!IsValidPeriod(period))
        {
            return BadRequest("Period must be one of: all, current-month, previous-month, current-year.");
        }

        var transactions = ApplyPeriodFilter(GetUserTransactions(), period);
        var normalizedType = type?.Trim().ToLowerInvariant();

        if (!string.IsNullOrWhiteSpace(normalizedType))
        {
            if (!TransactionTypes.IsValid(normalizedType))
            {
                return BadRequest("Type must be either 'income' or 'expense'.");
            }

            transactions = transactions.Where(transaction => transaction.Type == normalizedType);
        }

        var stats = await transactions
            .Include(transaction => transaction.Category)
            .GroupBy(transaction => new
            {
                transaction.CategoryId,
                CategoryName = transaction.Category!.Name,
                transaction.Type
            })
            .Select(group => new CategoryStatsDto
            {
                CategoryId = group.Key.CategoryId,
                CategoryName = group.Key.CategoryName,
                Type = group.Key.Type,
                Total = group.Sum(transaction => transaction.Amount)
            })
            .OrderBy(item => item.Type)
            .ThenByDescending(item => item.Total)
            .ToListAsync();

        return Ok(stats);
    }

    [HttpGet("latest-transactions")]
    public async Task<ActionResult<IEnumerable<TransactionResponseDto>>> GetLatestTransactions([FromQuery] int limit = 5)
    {
        var safeLimit = Math.Clamp(limit, 1, 20);

        var transactions = await GetUserTransactions()
            .Include(transaction => transaction.Category)
            .OrderByDescending(transaction => transaction.TransactionDate)
            .ThenByDescending(transaction => transaction.Id)
            .Take(safeLimit)
            .Select(transaction => new TransactionResponseDto
            {
                Id = transaction.Id,
                Title = transaction.Title,
                Amount = transaction.Amount,
                Type = transaction.Type,
                TransactionDate = transaction.TransactionDate,
                Description = transaction.Description,
                CategoryId = transaction.CategoryId,
                CategoryName = transaction.Category!.Name,
                CreatedAt = transaction.CreatedAt,
                UpdatedAt = transaction.UpdatedAt
            })
            .ToListAsync();

        return Ok(transactions);
    }

    private IQueryable<Transaction> GetUserTransactions()
    {
        return _context.Transactions
            .Where(transaction => transaction.UserId == CurrentUserId);
    }

    private static IQueryable<Transaction> ApplyPeriodFilter(IQueryable<Transaction> query, string? period)
    {
        var normalizedPeriod = period?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalizedPeriod) || normalizedPeriod == "all")
        {
            return query;
        }

        var today = DateTime.UtcNow.Date;
        var currentMonthStart = new DateTime(today.Year, today.Month, 1);

        var (startDate, endDate) = normalizedPeriod switch
        {
            "current-month" => (currentMonthStart, currentMonthStart.AddMonths(1)),
            "previous-month" => (currentMonthStart.AddMonths(-1), currentMonthStart),
            "current-year" => (new DateTime(today.Year, 1, 1), new DateTime(today.Year + 1, 1, 1)),
            _ => (DateTime.MinValue, DateTime.MaxValue)
        };

        return query.Where(transaction =>
            transaction.TransactionDate >= startDate &&
            transaction.TransactionDate < endDate);
    }

    private static bool IsValidPeriod(string? period)
    {
        var normalizedPeriod = period?.Trim().ToLowerInvariant();
        return string.IsNullOrWhiteSpace(normalizedPeriod) ||
            normalizedPeriod is "all" or "current-month" or "previous-month" or "current-year";
    }
}
