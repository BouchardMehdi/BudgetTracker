using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.Api.DTOs;

public class BudgetCreateDto
{
    [Range(1, int.MaxValue, ErrorMessage = "CategoryId is required.")]
    public int CategoryId { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
    public decimal Amount { get; set; }
}

public class BudgetUpdateDto
{
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
    public decimal Amount { get; set; }
}

public class BudgetResponseDto
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class BudgetProgressDto
{
    public int BudgetId { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal BudgetAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal ProgressPercent { get; set; }
}
