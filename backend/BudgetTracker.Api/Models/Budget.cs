namespace BudgetTracker.Api.Models;

public class Budget
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public int CategoryId { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Category? Category { get; set; }
    public User? User { get; set; }
}
