using BudgetTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Api.Data;

public class BudgetTrackerDbContext : DbContext
{
    public BudgetTrackerDbContext(DbContextOptions<BudgetTrackerDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Budget> Budgets => Set<Budget>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(user => user.Id);

            entity.Property(user => user.Id).HasColumnName("id");
            entity.Property(user => user.Username).HasColumnName("username").HasMaxLength(80).IsRequired();
            entity.Property(user => user.Email).HasColumnName("email").HasMaxLength(180).IsRequired();
            entity.Property(user => user.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
            entity.Property(user => user.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(user => user.Email).IsUnique();
            entity.HasIndex(user => user.Username).IsUnique();
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories", table =>
                table.HasCheckConstraint("ck_categories_type", "type IN ('income', 'expense')"));
            entity.HasKey(category => category.Id);

            entity.Property(category => category.Id).HasColumnName("id");
            entity.Property(category => category.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
            entity.Property(category => category.Type).HasColumnName("type").HasMaxLength(20).IsRequired();
            entity.Property(category => category.UserId).HasColumnName("user_id");
            entity.Property(category => category.CreatedAt).HasColumnName("created_at");

            entity.HasOne(category => category.User)
                .WithMany(user => user.Categories)
                .HasForeignKey(category => category.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(category => category.Transactions)
                .WithOne(transaction => transaction.Category)
                .HasForeignKey(transaction => transaction.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(category => category.Budget)
                .WithOne(budget => budget.Category)
                .HasForeignKey<Budget>(budget => budget.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(category => new { category.UserId, category.Name, category.Type }).IsUnique();
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.ToTable("transactions", table =>
            {
                table.HasCheckConstraint("ck_transactions_amount", "amount > 0");
                table.HasCheckConstraint("ck_transactions_type", "type IN ('income', 'expense')");
            });
            entity.HasKey(transaction => transaction.Id);

            entity.Property(transaction => transaction.Id).HasColumnName("id");
            entity.Property(transaction => transaction.Title).HasColumnName("title").HasMaxLength(160).IsRequired();
            entity.Property(transaction => transaction.Amount).HasColumnName("amount").HasColumnType("decimal(18,2)");
            entity.Property(transaction => transaction.Type).HasColumnName("type").HasMaxLength(20).IsRequired();
            entity.Property(transaction => transaction.TransactionDate).HasColumnName("transaction_date").HasColumnType("date");
            entity.Property(transaction => transaction.Description).HasColumnName("description").HasMaxLength(1000);
            entity.Property(transaction => transaction.CategoryId).HasColumnName("category_id");
            entity.Property(transaction => transaction.UserId).HasColumnName("user_id");
            entity.Property(transaction => transaction.IsRecurring).HasColumnName("is_recurring");
            entity.Property(transaction => transaction.RecurrenceStartDate).HasColumnName("recurrence_start_date").HasColumnType("date");
            entity.Property(transaction => transaction.RecurrenceEndDate).HasColumnName("recurrence_end_date").HasColumnType("date");
            entity.Property(transaction => transaction.RecurringParentId).HasColumnName("recurring_parent_id");
            entity.Property(transaction => transaction.CreatedAt).HasColumnName("created_at");
            entity.Property(transaction => transaction.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(transaction => transaction.User)
                .WithMany(user => user.Transactions)
                .HasForeignKey(transaction => transaction.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(transaction => transaction.Category)
                .WithMany(category => category.Transactions)
                .HasForeignKey(transaction => transaction.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(transaction => transaction.RecurringParent)
                .WithMany(transaction => transaction.RecurringChildren)
                .HasForeignKey(transaction => transaction.RecurringParentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(transaction => transaction.TransactionDate);
            entity.HasIndex(transaction => new { transaction.RecurringParentId, transaction.TransactionDate }).IsUnique();
        });

        modelBuilder.Entity<Budget>(entity =>
        {
            entity.ToTable("budgets", table =>
                table.HasCheckConstraint("ck_budgets_amount", "amount > 0"));
            entity.HasKey(budget => budget.Id);

            entity.Property(budget => budget.Id).HasColumnName("id");
            entity.Property(budget => budget.Amount).HasColumnName("amount").HasColumnType("decimal(18,2)");
            entity.Property(budget => budget.CategoryId).HasColumnName("category_id");
            entity.Property(budget => budget.UserId).HasColumnName("user_id");
            entity.Property(budget => budget.CreatedAt).HasColumnName("created_at");
            entity.Property(budget => budget.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(budget => budget.User)
                .WithMany(user => user.Budgets)
                .HasForeignKey(budget => budget.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(budget => new { budget.UserId, budget.CategoryId }).IsUnique();
        });
    }
}
