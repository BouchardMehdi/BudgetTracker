using BudgetTracker.Api.DTOs;
using BudgetTracker.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CategoriesController : AuthenticatedControllerBase
{
    private readonly CategoryService _categoryService;

    public CategoriesController(CategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetCategories()
    {
        var categories = await _categoryService.GetCategoriesAsync(CurrentUserId);
        return Ok(categories);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryResponseDto>> CreateCategory(CategoryCreateDto dto)
    {
        var result = await _categoryService.CreateCategoryAsync(CurrentUserId, dto);
        return this.ToActionResult(result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoryResponseDto>> UpdateCategory(int id, CategoryUpdateDto dto)
    {
        var result = await _categoryService.UpdateCategoryAsync(CurrentUserId, id, dto);
        return this.ToActionResult(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var result = await _categoryService.DeleteCategoryAsync(CurrentUserId, id);
        return this.ToActionResult(result);
    }
}
