namespace BudgetTracker.Api.Models;

public static class TransactionTypes
{
    public const string Income = "income";
    public const string Expense = "expense";

    public static bool IsValid(string? type)
    {
        return type is Income or Expense;
    }
}
