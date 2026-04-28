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
public class TransactionsController : AuthenticatedControllerBase
{
    private readonly BudgetTrackerDbContext _context;
    private readonly RecurringTransactionService _recurringTransactionService;

    public TransactionsController(BudgetTrackerDbContext context, RecurringTransactionService recurringTransactionService)
    {
        _context = context;
        _recurringTransactionService = recurringTransactionService;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponseDto<TransactionResponseDto>>> GetTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? type = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? month = null)
    {
        await _recurringTransactionService.GenerateDueOccurrencesAsync(CurrentUserId);

        var safePage = Math.Max(page, 1);
        var safePageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.Transactions
            .Include(transaction => transaction.Category)
            .Where(transaction => transaction.UserId == CurrentUserId);

        var normalizedType = type?.Trim().ToLowerInvariant();
        if (!string.IsNullOrWhiteSpace(normalizedType))
        {
            if (!TransactionTypes.IsValid(normalizedType))
            {
                return BadRequest("Type must be either 'income' or 'expense'.");
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
                return BadRequest("Month must use the YYYY-MM format.");
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

        return Ok(new PaginatedResponseDto<TransactionResponseDto>
        {
            Items = transactions,
            Page = safePage,
            PageSize = safePageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)safePageSize)
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TransactionResponseDto>> GetTransaction(int id)
    {
        var transaction = await _context.Transactions
            .Include(item => item.Category)
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == CurrentUserId);

        if (transaction is null)
        {
            return NotFound();
        }

        return Ok(ToResponseDto(transaction));
    }

    [HttpPost]
    public async Task<ActionResult<TransactionResponseDto>> CreateTransaction(TransactionCreateDto dto)
    {
        var validationError = await ValidateTransactionInput(dto.Type, dto.Amount, dto.CategoryId);
        if (validationError is not null)
        {
            return validationError;
        }

        var normalizedType = dto.Type.Trim().ToLowerInvariant();
        var recurrenceValidation = ValidateRecurrence(dto.IsRecurring, dto.TransactionDate, dto.RecurrenceStartDate, dto.RecurrenceEndDate);
        if (recurrenceValidation is not null)
        {
            return recurrenceValidation;
        }

        var recurrenceStartDate = dto.IsRecurring
            ? (dto.RecurrenceStartDate ?? dto.TransactionDate).Date
            : null as DateTime?;

        var transaction = new Transaction
        {
            Title = dto.Title.Trim(),
            Amount = dto.Amount,
            Type = normalizedType,
            TransactionDate = dto.TransactionDate.Date,
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            CategoryId = dto.CategoryId,
            UserId = CurrentUserId,
            IsRecurring = dto.IsRecurring,
            RecurrenceStartDate = recurrenceStartDate,
            RecurrenceEndDate = dto.IsRecurring ? dto.RecurrenceEndDate?.Date : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        await _recurringTransactionService.GenerateDueOccurrencesAsync(CurrentUserId);

        await _context.Entry(transaction).Reference(item => item.Category).LoadAsync();

        return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, ToResponseDto(transaction));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TransactionResponseDto>> UpdateTransaction(int id, TransactionUpdateDto dto)
    {
        var transaction = await _context.Transactions
            .Include(item => item.Category)
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == CurrentUserId);

        if (transaction is null)
        {
            return NotFound();
        }

        var validationError = await ValidateTransactionInput(dto.Type, dto.Amount, dto.CategoryId);
        if (validationError is not null)
        {
            return validationError;
        }

        var recurrenceValidation = ValidateRecurrence(dto.IsRecurring, dto.TransactionDate, dto.RecurrenceStartDate, dto.RecurrenceEndDate);
        if (recurrenceValidation is not null)
        {
            return recurrenceValidation;
        }

        transaction.Title = dto.Title.Trim();
        transaction.Amount = dto.Amount;
        transaction.Type = dto.Type.Trim().ToLowerInvariant();
        transaction.TransactionDate = dto.TransactionDate.Date;
        transaction.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        transaction.CategoryId = dto.CategoryId;
        transaction.IsRecurring = dto.IsRecurring && transaction.RecurringParentId == null;
        transaction.RecurrenceStartDate = transaction.IsRecurring
            ? (dto.RecurrenceStartDate ?? dto.TransactionDate).Date
            : null;
        transaction.RecurrenceEndDate = transaction.IsRecurring ? dto.RecurrenceEndDate?.Date : null;
        transaction.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _recurringTransactionService.GenerateDueOccurrencesAsync(CurrentUserId);
        await _context.Entry(transaction).Reference(item => item.Category).LoadAsync();

        return Ok(ToResponseDto(transaction));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTransaction(int id)
    {
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == CurrentUserId);

        if (transaction is null)
        {
            return NotFound();
        }

        if (transaction.IsRecurring && transaction.RecurringParentId == null)
        {
            var children = await _context.Transactions
                .Where(item => item.RecurringParentId == transaction.Id && item.UserId == CurrentUserId)
                .ToListAsync();
            _context.Transactions.RemoveRange(children);
        }

        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<ActionResult?> ValidateTransactionInput(string type, decimal amount, int categoryId)
    {
        var normalizedType = type.Trim().ToLowerInvariant();
        if (!TransactionTypes.IsValid(normalizedType))
        {
            return BadRequest("Type must be either 'income' or 'expense'.");
        }

        if (amount <= 0)
        {
            return BadRequest("Amount must be greater than 0.");
        }

        var category = await _context.Categories
            .FirstOrDefaultAsync(item => item.Id == categoryId && item.UserId == CurrentUserId);

        if (category is null)
        {
            return BadRequest("Category does not exist.");
        }

        if (category.Type != normalizedType)
        {
            return BadRequest("Category type must match transaction type.");
        }

        return null;
    }

    private ActionResult? ValidateRecurrence(
        bool isRecurring,
        DateTime transactionDate,
        DateTime? recurrenceStartDate,
        DateTime? recurrenceEndDate)
    {
        if (!isRecurring)
        {
            return null;
        }

        var startDate = (recurrenceStartDate ?? transactionDate).Date;
        if (recurrenceEndDate.HasValue && recurrenceEndDate.Value.Date < startDate)
        {
            return BadRequest("Recurrence end date must be after the start date.");
        }

        return null;
    }

    private static TransactionResponseDto ToResponseDto(Transaction transaction)
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
}
