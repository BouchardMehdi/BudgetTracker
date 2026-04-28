namespace BudgetTracker.Api.Models;

public class Transaction
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Type { get; set; } = TransactionTypes.Expense;
    public DateTime TransactionDate { get; set; }
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public int UserId { get; set; }
    public bool IsRecurring { get; set; }
    public DateTime? RecurrenceStartDate { get; set; }
    public DateTime? RecurrenceEndDate { get; set; }
    public int? RecurringParentId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Category? Category { get; set; }
    public User? User { get; set; }
    public Transaction? RecurringParent { get; set; }
    public ICollection<Transaction> RecurringChildren { get; set; } = new List<Transaction>();
}
