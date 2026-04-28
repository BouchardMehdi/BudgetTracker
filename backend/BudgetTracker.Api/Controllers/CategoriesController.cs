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
public class CategoriesController : AuthenticatedControllerBase
{
    private readonly BudgetTrackerDbContext _context;

    public CategoriesController(BudgetTrackerDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetCategories()
    {
        var categories = await _context.Categories
            .Where(category => category.UserId == CurrentUserId)
            .OrderBy(category => category.Type)
            .ThenBy(category => category.Name)
            .Select(category => ToResponseDto(category))
            .ToListAsync();

        return Ok(categories);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryResponseDto>> CreateCategory(CategoryCreateDto dto)
    {
        var normalizedType = dto.Type.Trim().ToLowerInvariant();
        if (!TransactionTypes.IsValid(normalizedType))
        {
            return BadRequest("Type must be either 'income' or 'expense'.");
        }

        var category = new Category
        {
            Name = dto.Name.Trim(),
            Type = normalizedType,
            UserId = CurrentUserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategories), new { id = category.Id }, ToResponseDto(category));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoryResponseDto>> UpdateCategory(int id, CategoryUpdateDto dto)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == CurrentUserId);

        if (category is null)
        {
            return NotFound();
        }

        var normalizedType = dto.Type.Trim().ToLowerInvariant();
        if (!TransactionTypes.IsValid(normalizedType))
        {
            return BadRequest("Type must be either 'income' or 'expense'.");
        }

        category.Name = dto.Name.Trim();
        category.Type = normalizedType;

        await _context.SaveChangesAsync();

        return Ok(ToResponseDto(category));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == CurrentUserId);

        if (category is null)
        {
            return NotFound();
        }

        var hasTransactions = await _context.Transactions
            .AnyAsync(transaction => transaction.CategoryId == id && transaction.UserId == CurrentUserId);

        if (hasTransactions)
        {
            return Conflict("This category is used by at least one transaction.");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static CategoryResponseDto ToResponseDto(Category category)
    {
        return new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            Type = category.Type,
            CreatedAt = category.CreatedAt
        };
    }
}
