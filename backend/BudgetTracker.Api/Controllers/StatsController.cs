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
    public async Task<ActionResult<SummaryDto>> GetSummary()
    {
        var totalIncome = await _context.Transactions
            .Where(transaction => transaction.UserId == CurrentUserId && transaction.Type == TransactionTypes.Income)
            .SumAsync(transaction => (decimal?)transaction.Amount) ?? 0;

        var totalExpense = await _context.Transactions
            .Where(transaction => transaction.UserId == CurrentUserId && transaction.Type == TransactionTypes.Expense)
            .SumAsync(transaction => (decimal?)transaction.Amount) ?? 0;

        return Ok(new SummaryDto
        {
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            Balance = totalIncome - totalExpense
        });
    }

    [HttpGet("by-category")]
    public async Task<ActionResult<IEnumerable<CategoryStatsDto>>> GetByCategory()
    {
        var stats = await _context.Transactions
            .Include(transaction => transaction.Category)
            .Where(transaction => transaction.UserId == CurrentUserId)
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
}
