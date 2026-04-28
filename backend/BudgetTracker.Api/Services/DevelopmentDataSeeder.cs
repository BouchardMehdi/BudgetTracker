using BudgetTracker.Api.Data;
using BudgetTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Api.Services;

public class DevelopmentDataSeeder
{
    private readonly BudgetTrackerDbContext _context;
    private readonly PasswordHasher _passwordHasher;

    public DevelopmentDataSeeder(BudgetTrackerDbContext context, PasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync()
    {
        await _context.Database.MigrateAsync();

        const string demoEmail = "demo@budgettracker.local";
        if (await _context.Users.AnyAsync(user => user.Email == demoEmail))
        {
            return;
        }

        var user = new User
        {
            Username = "demo",
            Email = demoEmail,
            PasswordHash = _passwordHasher.Hash("Password123!"),
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var categories = new[]
        {
            new Category { Name = "Salaire", Type = TransactionTypes.Income, UserId = user.Id },
            new Category { Name = "Freelance", Type = TransactionTypes.Income, UserId = user.Id },
            new Category { Name = "Courses", Type = TransactionTypes.Expense, UserId = user.Id },
            new Category { Name = "Logement", Type = TransactionTypes.Expense, UserId = user.Id },
            new Category { Name = "Transport", Type = TransactionTypes.Expense, UserId = user.Id },
            new Category { Name = "Abonnements", Type = TransactionTypes.Expense, UserId = user.Id }
        };

        _context.Categories.AddRange(categories);
        await _context.SaveChangesAsync();

        var salaire = categories.First(category => category.Name == "Salaire");
        var freelance = categories.First(category => category.Name == "Freelance");
        var courses = categories.First(category => category.Name == "Courses");
        var logement = categories.First(category => category.Name == "Logement");
        var transport = categories.First(category => category.Name == "Transport");
        var abonnements = categories.First(category => category.Name == "Abonnements");

        _context.Budgets.AddRange(
            new Budget { Amount = 400, CategoryId = courses.Id, UserId = user.Id },
            new Budget { Amount = 920, CategoryId = logement.Id, UserId = user.Id },
            new Budget { Amount = 120, CategoryId = transport.Id, UserId = user.Id },
            new Budget { Amount = 80, CategoryId = abonnements.Id, UserId = user.Id });

        _context.Transactions.AddRange(
            new Transaction
            {
                Title = "Salaire du mois",
                Amount = 2850,
                Type = TransactionTypes.Income,
                TransactionDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1),
                CategoryId = salaire.Id,
                UserId = user.Id
            },
            new Transaction
            {
                Title = "Mission freelance",
                Amount = 450,
                Type = TransactionTypes.Income,
                TransactionDate = DateTime.UtcNow.Date.AddDays(-6),
                CategoryId = freelance.Id,
                UserId = user.Id
            },
            new Transaction
            {
                Title = "Courses",
                Amount = 86.45m,
                Type = TransactionTypes.Expense,
                TransactionDate = DateTime.UtcNow.Date.AddDays(-4),
                CategoryId = courses.Id,
                UserId = user.Id
            },
            new Transaction
            {
                Title = "Loyer",
                Amount = 920,
                Type = TransactionTypes.Expense,
                TransactionDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 3),
                CategoryId = logement.Id,
                UserId = user.Id,
                IsRecurring = true,
                RecurrenceStartDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 3)
            });

        await _context.SaveChangesAsync();
    }
}
