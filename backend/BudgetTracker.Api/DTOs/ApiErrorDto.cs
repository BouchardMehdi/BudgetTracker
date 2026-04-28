namespace BudgetTracker.Api.DTOs;

public class ApiErrorDto
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public IDictionary<string, string[]>? Details { get; set; }

    public static ApiErrorDto Create(string code, string message)
    {
        return new ApiErrorDto
        {
            Code = code,
            Message = message
        };
    }
}
