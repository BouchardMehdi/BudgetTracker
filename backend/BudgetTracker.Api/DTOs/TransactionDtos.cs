using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.Api.DTOs;

public class TransactionCreateDto
{
    [Required]
    [MaxLength(160)]
    public string Title { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
    public decimal Amount { get; set; }

    [Required]
    public string Type { get; set; } = string.Empty;

    [Required]
    public DateTime TransactionDate { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "CategoryId is required.")]
    public int CategoryId { get; set; }
}

public class TransactionUpdateDto
{
    [Required]
    [MaxLength(160)]
    public string Title { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
    public decimal Amount { get; set; }

    [Required]
    public string Type { get; set; } = string.Empty;

    [Required]
    public DateTime TransactionDate { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "CategoryId is required.")]
    public int CategoryId { get; set; }
}

public class TransactionResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
