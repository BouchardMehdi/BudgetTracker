namespace BudgetTracker.Api.Models;

public static class TransactionTypes
{
    public const string Income = "income";
    public const string Expense = "expense";

    public static bool IsValid(string? type)
    {
        return Normalize(type) is Income or Expense;
    }

    public static string Normalize(string? type)
    {
        return type?.Trim().ToLowerInvariant() ?? string.Empty;
    }

    public static bool TryNormalize(string? type, out string normalizedType)
    {
        normalizedType = Normalize(type);
        return IsValid(normalizedType);
    }
}
