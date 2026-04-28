using BudgetTracker.Api.Data;
using BudgetTracker.Api.DTOs;
using BudgetTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Api.Services;

public class CategoryService
{
    private readonly BudgetTrackerDbContext _context;

    public CategoryService(BudgetTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CategoryResponseDto>> GetCategoriesAsync(int userId)
    {
        return await _context.Categories
            .Where(category => category.UserId == userId)
            .OrderBy(category => category.Type)
            .ThenBy(category => category.Name)
            .Select(category => ToResponseDto(category))
            .ToListAsync();
    }

    public async Task<ServiceResult<CategoryResponseDto>> CreateCategoryAsync(int userId, CategoryCreateDto dto)
    {
        if (!TransactionTypes.TryNormalize(dto.Type, out var normalizedType))
        {
            return ServiceResult<CategoryResponseDto>.Failure(
                "invalid_transaction_type",
                "Type must be either 'income' or 'expense'.",
                StatusCodes.Status400BadRequest);
        }

        var category = new Category
        {
            Name = dto.Name.Trim(),
            Type = normalizedType,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return ServiceResult<CategoryResponseDto>.Success(ToResponseDto(category), StatusCodes.Status201Created);
    }

    public async Task<ServiceResult<CategoryResponseDto>> UpdateCategoryAsync(int userId, int id, CategoryUpdateDto dto)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId);

        if (category is null)
        {
            return ServiceResult<CategoryResponseDto>.Failure("category_not_found", "Category was not found.", StatusCodes.Status404NotFound);
        }

        if (!TransactionTypes.TryNormalize(dto.Type, out var normalizedType))
        {
            return ServiceResult<CategoryResponseDto>.Failure(
                "invalid_transaction_type",
                "Type must be either 'income' or 'expense'.",
                StatusCodes.Status400BadRequest);
        }

        category.Name = dto.Name.Trim();
        category.Type = normalizedType;

        await _context.SaveChangesAsync();

        return ServiceResult<CategoryResponseDto>.Success(ToResponseDto(category));
    }

    public async Task<ServiceResult> DeleteCategoryAsync(int userId, int id)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId);

        if (category is null)
        {
            return ServiceResult.Failure("category_not_found", "Category was not found.", StatusCodes.Status404NotFound);
        }

        var hasTransactions = await _context.Transactions
            .AnyAsync(transaction => transaction.CategoryId == id && transaction.UserId == userId);

        if (hasTransactions)
        {
            return ServiceResult.Failure(
                "category_in_use",
                "This category is used by at least one active transaction.",
                StatusCodes.Status409Conflict);
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return ServiceResult.Success();
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
