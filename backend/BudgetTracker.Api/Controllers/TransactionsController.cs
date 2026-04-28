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
public class TransactionsController : AuthenticatedControllerBase
{
    private readonly BudgetTrackerDbContext _context;

    public TransactionsController(BudgetTrackerDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TransactionResponseDto>>> GetTransactions()
    {
        var transactions = await _context.Transactions
            .Include(transaction => transaction.Category)
            .Where(transaction => transaction.UserId == CurrentUserId)
            .OrderByDescending(transaction => transaction.TransactionDate)
            .ThenByDescending(transaction => transaction.Id)
            .Select(transaction => ToResponseDto(transaction))
            .ToListAsync();

        return Ok(transactions);
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
        var transaction = new Transaction
        {
            Title = dto.Title.Trim(),
            Amount = dto.Amount,
            Type = normalizedType,
            TransactionDate = dto.TransactionDate.Date,
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            CategoryId = dto.CategoryId,
            UserId = CurrentUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

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

        transaction.Title = dto.Title.Trim();
        transaction.Amount = dto.Amount;
        transaction.Type = dto.Type.Trim().ToLowerInvariant();
        transaction.TransactionDate = dto.TransactionDate.Date;
        transaction.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        transaction.CategoryId = dto.CategoryId;
        transaction.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
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
            CreatedAt = transaction.CreatedAt,
            UpdatedAt = transaction.UpdatedAt
        };
    }
}
