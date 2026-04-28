using BudgetTracker.Api.Data;
using BudgetTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Api.Services;

public class RecurringTransactionService
{
    private readonly BudgetTrackerDbContext _context;

    public RecurringTransactionService(BudgetTrackerDbContext context)
    {
        _context = context;
    }

    public async Task GenerateDueOccurrencesAsync(int userId)
    {
        var today = DateTime.UtcNow.Date;
        var templates = await _context.Transactions
            .Where(transaction =>
                transaction.UserId == userId &&
                transaction.IsRecurring &&
                transaction.RecurringParentId == null &&
                transaction.RecurrenceStartDate != null)
            .ToListAsync();

        foreach (var template in templates)
        {
            var startDate = template.RecurrenceStartDate!.Value.Date;
            var endDate = template.RecurrenceEndDate?.Date ?? today;
            var generationLimit = endDate < today ? endDate : today;

            if (generationLimit < startDate)
            {
                continue;
            }

            var monthCount = ((generationLimit.Year - startDate.Year) * 12) + generationLimit.Month - startDate.Month;

            for (var index = 1; index <= monthCount; index += 1)
            {
                var occurrenceDate = GetMonthlyOccurrenceDate(startDate, index);
                if (occurrenceDate > generationLimit)
                {
                    continue;
                }

                var exists = await _context.Transactions.AnyAsync(transaction =>
                    transaction.UserId == userId &&
                    transaction.RecurringParentId == template.Id &&
                    transaction.TransactionDate == occurrenceDate);

                if (exists)
                {
                    continue;
                }

                _context.Transactions.Add(new Transaction
                {
                    Title = template.Title,
                    Amount = template.Amount,
                    Type = template.Type,
                    TransactionDate = occurrenceDate,
                    Description = template.Description,
                    CategoryId = template.CategoryId,
                    UserId = template.UserId,
                    IsRecurring = false,
                    RecurringParentId = template.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();
    }

    private static DateTime GetMonthlyOccurrenceDate(DateTime startDate, int monthOffset)
    {
        var month = new DateTime(startDate.Year, startDate.Month, 1).AddMonths(monthOffset);
        var day = Math.Min(startDate.Day, DateTime.DaysInMonth(month.Year, month.Month));
        return new DateTime(month.Year, month.Month, day);
    }
}
