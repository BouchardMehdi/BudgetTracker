namespace BudgetTracker.Api.Services;

public static class ReportingPeriods
{
    public static bool IsValidStatsPeriod(string? period)
    {
        var normalizedPeriod = Normalize(period);
        return string.IsNullOrWhiteSpace(normalizedPeriod) ||
            normalizedPeriod is "all" or "current-month" or "previous-month" or "current-year";
    }

    public static bool IsValidBudgetPeriod(string? period)
    {
        var normalizedPeriod = Normalize(period);
        return string.IsNullOrWhiteSpace(normalizedPeriod) ||
            normalizedPeriod is "current-month" or "previous-month" or "current-year";
    }

    public static IQueryable<T> ApplyStatsPeriodFilter<T>(
        IQueryable<T> query,
        string? period,
        System.Linq.Expressions.Expression<Func<T, DateTime>> dateSelector)
    {
        var normalizedPeriod = Normalize(period);
        if (string.IsNullOrWhiteSpace(normalizedPeriod) || normalizedPeriod == "all")
        {
            return query;
        }

        var (startDate, endDate) = GetStatsPeriodRange(normalizedPeriod);
        return query.Where(BuildRangeExpression(dateSelector, startDate, endDate));
    }

    public static (DateTime StartDate, DateTime EndDate, int Multiplier) GetBudgetPeriodRange(string? period)
    {
        var today = DateTime.UtcNow.Date;
        var currentMonthStart = new DateTime(today.Year, today.Month, 1);

        return Normalize(period) switch
        {
            "previous-month" => (currentMonthStart.AddMonths(-1), currentMonthStart, 1),
            "current-year" => (new DateTime(today.Year, 1, 1), new DateTime(today.Year + 1, 1, 1), today.Month),
            _ => (currentMonthStart, currentMonthStart.AddMonths(1), 1)
        };
    }

    private static (DateTime StartDate, DateTime EndDate) GetStatsPeriodRange(string period)
    {
        var today = DateTime.UtcNow.Date;
        var currentMonthStart = new DateTime(today.Year, today.Month, 1);

        return period switch
        {
            "current-month" => (currentMonthStart, currentMonthStart.AddMonths(1)),
            "previous-month" => (currentMonthStart.AddMonths(-1), currentMonthStart),
            "current-year" => (new DateTime(today.Year, 1, 1), new DateTime(today.Year + 1, 1, 1)),
            _ => (DateTime.MinValue, DateTime.MaxValue)
        };
    }

    private static string Normalize(string? period)
    {
        return period?.Trim().ToLowerInvariant() ?? string.Empty;
    }

    private static System.Linq.Expressions.Expression<Func<T, bool>> BuildRangeExpression<T>(
        System.Linq.Expressions.Expression<Func<T, DateTime>> dateSelector,
        DateTime startDate,
        DateTime endDate)
    {
        var parameter = dateSelector.Parameters[0];
        var body = System.Linq.Expressions.Expression.AndAlso(
            System.Linq.Expressions.Expression.GreaterThanOrEqual(dateSelector.Body, System.Linq.Expressions.Expression.Constant(startDate)),
            System.Linq.Expressions.Expression.LessThan(dateSelector.Body, System.Linq.Expressions.Expression.Constant(endDate)));

        return System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}
