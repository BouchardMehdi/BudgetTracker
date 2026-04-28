using BudgetTracker.Api.Data;
using BudgetTracker.Api.DTOs;
using BudgetTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Api.Services;

public class BudgetService
{
    private readonly BudgetTrackerDbContext _context;
    private readonly RecurringTransactionService _recurringTransactionService;

    public BudgetService(BudgetTrackerDbContext context, RecurringTransactionService recurringTransactionService)
    {
        _context = context;
        _recurringTransactionService = recurringTransactionService;
    }

    public async Task<IEnumerable<BudgetResponseDto>> GetBudgetsAsync(int userId)
    {
        return await _context.Budgets
            .Include(budget => budget.Category)
            .Where(budget => budget.UserId == userId)
            .OrderBy(budget => budget.Category!.Name)
            .Select(budget => ToResponseDto(budget))
            .ToListAsync();
    }

    public async Task<ServiceResult<BudgetResponseDto>> CreateBudgetAsync(int userId, BudgetCreateDto dto)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(item => item.Id == dto.CategoryId && item.UserId == userId);

        if (category is null)
        {
            return ServiceResult<BudgetResponseDto>.Failure("category_not_found", "Category does not exist.", StatusCodes.Status400BadRequest);
        }

        if (category.Type != TransactionTypes.Expense)
        {
            return ServiceResult<BudgetResponseDto>.Failure(
                "budget_requires_expense_category",
                "Budgets can only be created for expense categories.",
                StatusCodes.Status400BadRequest);
        }

        var alreadyExists = await _context.Budgets
            .AnyAsync(item => item.CategoryId == dto.CategoryId && item.UserId == userId);

        if (alreadyExists)
        {
            return ServiceResult<BudgetResponseDto>.Failure(
                "budget_already_exists",
                "A budget already exists for this category.",
                StatusCodes.Status409Conflict);
        }

        var budget = new Budget
        {
            Amount = dto.Amount,
            CategoryId = dto.CategoryId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync();

        budget.Category = category;

        return ServiceResult<BudgetResponseDto>.Success(ToResponseDto(budget), StatusCodes.Status201Created);
    }

    public async Task<ServiceResult<BudgetResponseDto>> UpdateBudgetAsync(int userId, int id, BudgetUpdateDto dto)
    {
        var budget = await _context.Budgets
            .Include(item => item.Category)
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId);

        if (budget is null)
        {
            return ServiceResult<BudgetResponseDto>.Failure("budget_not_found", "Budget was not found.", StatusCodes.Status404NotFound);
        }

        budget.Amount = dto.Amount;
        budget.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ServiceResult<BudgetResponseDto>.Success(ToResponseDto(budget));
    }

    public async Task<ServiceResult> DeleteBudgetAsync(int userId, int id)
    {
        var budget = await _context.Budgets
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId);

        if (budget is null)
        {
            return ServiceResult.Failure("budget_not_found", "Budget was not found.", StatusCodes.Status404NotFound);
        }

        _context.Budgets.Remove(budget);
        await _context.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult<IEnumerable<BudgetProgressDto>>> GetBudgetProgressAsync(int userId, string? period)
    {
        await _recurringTransactionService.GenerateDueOccurrencesAsync(userId);

        if (!ReportingPeriods.IsValidBudgetPeriod(period))
        {
            return ServiceResult<IEnumerable<BudgetProgressDto>>.Failure(
                "invalid_period",
                "Period must be one of: current-month, previous-month, current-year.",
                StatusCodes.Status400BadRequest);
        }

        var (startDate, endDate, multiplier) = ReportingPeriods.GetBudgetPeriodRange(period);

        var spentByCategory = await _context.Transactions
            .Where(transaction =>
                transaction.UserId == userId &&
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
            .Where(budget => budget.UserId == userId)
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

        return ServiceResult<IEnumerable<BudgetProgressDto>>.Success(progress);
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
}

