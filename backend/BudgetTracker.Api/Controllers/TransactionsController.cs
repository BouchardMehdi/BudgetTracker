using BudgetTracker.Api.DTOs;
using BudgetTracker.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TransactionsController : AuthenticatedControllerBase
{
    private readonly TransactionService _transactionService;

    public TransactionsController(TransactionService transactionService)
    {
        _transactionService = transactionService;
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
        var result = await _transactionService.GetTransactionsAsync(CurrentUserId, page, pageSize, search, type, categoryId, month);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TransactionResponseDto>> GetTransaction(int id)
    {
        var result = await _transactionService.GetTransactionAsync(CurrentUserId, id);
        return this.ToActionResult(result);
    }

    [HttpPost]
    public async Task<ActionResult<TransactionResponseDto>> CreateTransaction(TransactionCreateDto dto)
    {
        var result = await _transactionService.CreateTransactionAsync(CurrentUserId, dto);
        return this.ToActionResult(result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TransactionResponseDto>> UpdateTransaction(int id, TransactionUpdateDto dto)
    {
        var result = await _transactionService.UpdateTransactionAsync(CurrentUserId, id, dto);
        return this.ToActionResult(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTransaction(int id)
    {
        var result = await _transactionService.DeleteTransactionAsync(CurrentUserId, id);
        return this.ToActionResult(result);
    }
}
