namespace BudgetTracker.Api.DTOs;

public class SummaryDto
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal Balance { get; set; }
}

public class CategoryStatsDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Total { get; set; }
}
