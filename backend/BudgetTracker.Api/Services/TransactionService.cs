using BudgetTracker.Api.Data;
using BudgetTracker.Api.DTOs;
using BudgetTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Api.Services;

public class TransactionService
{
    private readonly BudgetTrackerDbContext _context;
    private readonly RecurringTransactionService _recurringTransactionService;

    public TransactionService(BudgetTrackerDbContext context, RecurringTransactionService recurringTransactionService)
    {
        _context = context;
        _recurringTransactionService = recurringTransactionService;
    }

    public async Task<ServiceResult<PaginatedResponseDto<TransactionResponseDto>>> GetTransactionsAsync(
        int userId,
        int page,
        int pageSize,
        string? search,
        string? type,
        int? categoryId,
        string? month)
    {
        await _recurringTransactionService.GenerateDueOccurrencesAsync(userId);

        var safePage = Math.Max(page, 1);
        var safePageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.Transactions
            .Include(transaction => transaction.Category)
            .Where(transaction => transaction.UserId == userId);

        if (!string.IsNullOrWhiteSpace(type))
        {
            if (!TransactionTypes.TryNormalize(type, out var normalizedType))
            {
                return ServiceResult<PaginatedResponseDto<TransactionResponseDto>>.Failure(
                    "invalid_transaction_type",
                    "Type must be either 'income' or 'expense'.",
                    StatusCodes.Status400BadRequest);
            }

            query = query.Where(transaction => transaction.Type == normalizedType);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(transaction => transaction.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLowerInvariant();
            query = query.Where(transaction =>
                transaction.Title.ToLower().Contains(normalizedSearch) ||
                (transaction.Description != null && transaction.Description.ToLower().Contains(normalizedSearch)));
        }

        if (!string.IsNullOrWhiteSpace(month))
        {
            if (!DateTime.TryParse($"{month}-01", out var monthStart))
            {
                return ServiceResult<PaginatedResponseDto<TransactionResponseDto>>.Failure(
                    "invalid_month",
                    "Month must use the YYYY-MM format.",
                    StatusCodes.Status400BadRequest);
            }

            var monthEnd = monthStart.AddMonths(1);
            query = query.Where(transaction =>
                transaction.TransactionDate >= monthStart &&
                transaction.TransactionDate < monthEnd);
        }

        var totalItems = await query.CountAsync();
        var transactions = await query
            .OrderByDescending(transaction => transaction.TransactionDate)
            .ThenByDescending(transaction => transaction.Id)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .Select(transaction => ToResponseDto(transaction))
            .ToListAsync();

        return ServiceResult<PaginatedResponseDto<TransactionResponseDto>>.Success(new PaginatedResponseDto<TransactionResponseDto>
        {
            Items = transactions,
            Page = safePage,
            PageSize = safePageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)safePageSize)
        });
    }

    public async Task<ServiceResult<TransactionResponseDto>> GetTransactionAsync(int userId, int id)
    {
        var transaction = await _context.Transactions
            .Include(item => item.Category)
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId);

        return transaction is null
            ? ServiceResult<TransactionResponseDto>.Failure("transaction_not_found", "Transaction was not found.", StatusCodes.Status404NotFound)
            : ServiceResult<TransactionResponseDto>.Success(ToResponseDto(transaction));
    }

    public async Task<ServiceResult<TransactionResponseDto>> CreateTransactionAsync(int userId, TransactionCreateDto dto)
    {
        var validation = await ValidateTransactionInput(userId, dto.Type, dto.Amount, dto.CategoryId);
        if (!validation.IsSuccess)
        {
            return ServiceResult<TransactionResponseDto>.Failure(validation.Error!.Code, validation.Error.Message, validation.StatusCode);
        }

        var recurrenceValidation = ValidateRecurrence(dto.IsRecurring, dto.TransactionDate, dto.RecurrenceStartDate, dto.RecurrenceEndDate);
        if (!recurrenceValidation.IsSuccess)
        {
            return ServiceResult<TransactionResponseDto>.Failure(
                recurrenceValidation.Error!.Code,
                recurrenceValidation.Error.Message,
                recurrenceValidation.StatusCode);
        }

        var transaction = new Transaction
        {
            Title = dto.Title.Trim(),
            Amount = dto.Amount,
            Type = validation.Value!,
            TransactionDate = dto.TransactionDate.Date,
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            CategoryId = dto.CategoryId,
            UserId = userId,
            IsRecurring = dto.IsRecurring,
            RecurrenceStartDate = dto.IsRecurring ? (dto.RecurrenceStartDate ?? dto.TransactionDate).Date : null,
            RecurrenceEndDate = dto.IsRecurring ? dto.RecurrenceEndDate?.Date : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        await _recurringTransactionService.GenerateDueOccurrencesAsync(userId);
        await _context.Entry(transaction).Reference(item => item.Category).LoadAsync();

        return ServiceResult<TransactionResponseDto>.Success(ToResponseDto(transaction), StatusCodes.Status201Created);
    }

    public async Task<ServiceResult<TransactionResponseDto>> UpdateTransactionAsync(int userId, int id, TransactionUpdateDto dto)
    {
        var transaction = await _context.Transactions
            .Include(item => item.Category)
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId);

        if (transaction is null)
        {
            return ServiceResult<TransactionResponseDto>.Failure("transaction_not_found", "Transaction was not found.", StatusCodes.Status404NotFound);
        }

        var validation = await ValidateTransactionInput(userId, dto.Type, dto.Amount, dto.CategoryId);
        if (!validation.IsSuccess)
        {
            return ServiceResult<TransactionResponseDto>.Failure(validation.Error!.Code, validation.Error.Message, validation.StatusCode);
        }

        var recurrenceValidation = ValidateRecurrence(dto.IsRecurring, dto.TransactionDate, dto.RecurrenceStartDate, dto.RecurrenceEndDate);
        if (!recurrenceValidation.IsSuccess)
        {
            return ServiceResult<TransactionResponseDto>.Failure(
                recurrenceValidation.Error!.Code,
                recurrenceValidation.Error.Message,
                recurrenceValidation.StatusCode);
        }

        transaction.Title = dto.Title.Trim();
        transaction.Amount = dto.Amount;
        transaction.Type = validation.Value!;
        transaction.TransactionDate = dto.TransactionDate.Date;
        transaction.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        transaction.CategoryId = dto.CategoryId;
        transaction.IsRecurring = dto.IsRecurring && transaction.RecurringParentId == null;
        transaction.RecurrenceStartDate = transaction.IsRecurring ? (dto.RecurrenceStartDate ?? dto.TransactionDate).Date : null;
        transaction.RecurrenceEndDate = transaction.IsRecurring ? dto.RecurrenceEndDate?.Date : null;
        transaction.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _recurringTransactionService.GenerateDueOccurrencesAsync(userId);
        await _context.Entry(transaction).Reference(item => item.Category).LoadAsync();

        return ServiceResult<TransactionResponseDto>.Success(ToResponseDto(transaction));
    }

    public async Task<ServiceResult> DeleteTransactionAsync(int userId, int id)
    {
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId);

        if (transaction is null)
        {
            return ServiceResult.Failure("transaction_not_found", "Transaction was not found.", StatusCodes.Status404NotFound);
        }

        SoftDelete(transaction);

        if (transaction.IsRecurring && transaction.RecurringParentId == null)
        {
            var children = await _context.Transactions
                .Where(item => item.RecurringParentId == transaction.Id && item.UserId == userId)
                .ToListAsync();

            foreach (var child in children)
            {
                SoftDelete(child);
            }
        }

        await _context.SaveChangesAsync();
        return ServiceResult.Success();
    }

    public static TransactionResponseDto ToResponseDto(Transaction transaction)
    {
        return new TransactionResponseDto
        {
            Id = transaction.Id,
            Title = transaction.Title,
            Amount = transaction.Amount,
            Type = transaction.Type,
            TransactionDate = transaction.TransactionDate,
            Description = transaction.Description,
            CategoryId = transaction.CategoryId,
            CategoryName = transaction.Category?.Name ?? string.Empty,
            IsRecurring = transaction.IsRecurring,
            RecurrenceStartDate = transaction.RecurrenceStartDate,
            RecurrenceEndDate = transaction.RecurrenceEndDate,
            RecurringParentId = transaction.RecurringParentId,
            CreatedAt = transaction.CreatedAt,
            UpdatedAt = transaction.UpdatedAt
        };
    }

    private async Task<ServiceResult<string>> ValidateTransactionInput(int userId, string type, decimal amount, int categoryId)
    {
        if (!TransactionTypes.TryNormalize(type, out var normalizedType))
        {
            return ServiceResult<string>.Failure(
                "invalid_transaction_type",
                "Type must be either 'income' or 'expense'.",
                StatusCodes.Status400BadRequest);
        }

        if (amount <= 0)
        {
            return ServiceResult<string>.Failure(
                "invalid_amount",
                "Amount must be greater than 0.",
                StatusCodes.Status400BadRequest);
        }

        var category = await _context.Categories
            .FirstOrDefaultAsync(item => item.Id == categoryId && item.UserId == userId);

        if (category is null)
        {
            return ServiceResult<string>.Failure(
                "category_not_found",
                "Category does not exist.",
                StatusCodes.Status400BadRequest);
        }

        if (category.Type != normalizedType)
        {
            return ServiceResult<string>.Failure(
                "category_type_mismatch",
                "Category type must match transaction type.",
                StatusCodes.Status400BadRequest);
        }

        return ServiceResult<string>.Success(normalizedType);
    }

    private static ServiceResult ValidateRecurrence(
        bool isRecurring,
        DateTime transactionDate,
        DateTime? recurrenceStartDate,
        DateTime? recurrenceEndDate)
    {
        if (!isRecurring)
        {
            return ServiceResult.Success();
        }

        var startDate = (recurrenceStartDate ?? transactionDate).Date;
        if (recurrenceEndDate.HasValue && recurrenceEndDate.Value.Date < startDate)
        {
            return ServiceResult.Failure(
                "invalid_recurrence_dates",
                "Recurrence end date must be after the start date.",
                StatusCodes.Status400BadRequest);
        }

        return ServiceResult.Success();
    }

    private static void SoftDelete(Transaction transaction)
    {
        transaction.IsDeleted = true;
        transaction.DeletedAt = DateTime.UtcNow;
        transaction.UpdatedAt = DateTime.UtcNow;
    }
}
