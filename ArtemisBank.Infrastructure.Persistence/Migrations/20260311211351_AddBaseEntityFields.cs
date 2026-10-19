using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtemisBank.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBaseEntityFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Transactions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "SavingsAccounts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Loans",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "LoanInstallments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "LoanInstallments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "CreditCards",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "CreditCardConsumptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Beneficiaries",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "SavingsAccounts");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "LoanInstallments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "LoanInstallments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CreditCards");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CreditCardConsumptions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Beneficiaries");
        }
    }
}
