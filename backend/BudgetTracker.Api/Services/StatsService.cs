using BudgetTracker.Api.Data;
using BudgetTracker.Api.DTOs;
using BudgetTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Api.Services;

public class StatsService
{
    private readonly BudgetTrackerDbContext _context;
    private readonly RecurringTransactionService _recurringTransactionService;

    public StatsService(BudgetTrackerDbContext context, RecurringTransactionService recurringTransactionService)
    {
        _context = context;
        _recurringTransactionService = recurringTransactionService;
    }

    public async Task<ServiceResult<SummaryDto>> GetSummaryAsync(int userId, string? period)
    {
        await _recurringTransactionService.GenerateDueOccurrencesAsync(userId);

        if (!ReportingPeriods.IsValidStatsPeriod(period))
        {
            return ServiceResult<SummaryDto>.Failure(
                "invalid_period",
                "Period must be one of: all, current-month, previous-month, current-year.",
                StatusCodes.Status400BadRequest);
        }

        var transactions = ApplyPeriodFilter(GetUserTransactions(userId), period);

        var totalIncome = await transactions
            .Where(transaction => transaction.Type == TransactionTypes.Income)
            .SumAsync(transaction => (decimal?)transaction.Amount) ?? 0;

        var totalExpense = await transactions
            .Where(transaction => transaction.Type == TransactionTypes.Expense)
            .SumAsync(transaction => (decimal?)transaction.Amount) ?? 0;

        return ServiceResult<SummaryDto>.Success(new SummaryDto
        {
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            Balance = totalIncome - totalExpense
        });
    }

    public async Task<ServiceResult<IEnumerable<CategoryStatsDto>>> GetByCategoryAsync(int userId, string? period, string? type)
    {
        await _recurringTransactionService.GenerateDueOccurrencesAsync(userId);

        if (!ReportingPeriods.IsValidStatsPeriod(period))
        {
            return ServiceResult<IEnumerable<CategoryStatsDto>>.Failure(
                "invalid_period",
                "Period must be one of: all, current-month, previous-month, current-year.",
                StatusCodes.Status400BadRequest);
        }

        var transactions = ApplyPeriodFilter(GetUserTransactions(userId), period);

        if (!string.IsNullOrWhiteSpace(type))
        {
            if (!TransactionTypes.TryNormalize(type, out var normalizedType))
            {
                return ServiceResult<IEnumerable<CategoryStatsDto>>.Failure(
                    "invalid_transaction_type",
                    "Type must be either 'income' or 'expense'.",
                    StatusCodes.Status400BadRequest);
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

        return ServiceResult<IEnumerable<CategoryStatsDto>>.Success(stats);
    }

    public async Task<IEnumerable<TransactionResponseDto>> GetLatestTransactionsAsync(int userId, int limit)
    {
        await _recurringTransactionService.GenerateDueOccurrencesAsync(userId);

        var safeLimit = Math.Clamp(limit, 1, 20);

        return await GetUserTransactions(userId)
            .Include(transaction => transaction.Category)
            .OrderByDescending(transaction => transaction.TransactionDate)
            .ThenByDescending(transaction => transaction.Id)
            .Take(safeLimit)
            .Select(transaction => TransactionService.ToResponseDto(transaction))
            .ToListAsync();
    }

    private IQueryable<Transaction> GetUserTransactions(int userId)
    {
        return _context.Transactions
            .Where(transaction => transaction.UserId == userId);
    }

    private static IQueryable<Transaction> ApplyPeriodFilter(IQueryable<Transaction> query, string? period)
    {
        return ReportingPeriods.ApplyStatsPeriodFilter(query, period, transaction => transaction.TransactionDate);
    }
}
