using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRecurringTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_recurring",
                table: "transactions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "recurrence_end_date",
                table: "transactions",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "recurrence_start_date",
                table: "transactions",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "recurring_parent_id",
                table: "transactions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_transactions_recurring_parent_id_transaction_date",
                table: "transactions",
                columns: new[] { "recurring_parent_id", "transaction_date" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_transactions_transactions_recurring_parent_id",
                table: "transactions",
                column: "recurring_parent_id",
                principalTable: "transactions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_transactions_transactions_recurring_parent_id",
                table: "transactions");

            migrationBuilder.DropIndex(
                name: "IX_transactions_recurring_parent_id_transaction_date",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "is_recurring",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "recurrence_end_date",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "recurrence_start_date",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "recurring_parent_id",
                table: "transactions");
        }
    }
}
