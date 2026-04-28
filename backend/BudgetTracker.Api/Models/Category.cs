namespace BudgetTracker.Api.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = TransactionTypes.Expense;
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public Budget? Budget { get; set; }
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
