using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtemisBank.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLoanColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLate",
                table: "Loans",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PaidInstallments",
                table: "Loans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "PendingAmount",
                table: "Loans",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "TotalInstallments",
                table: "Loans",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLate",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "PaidInstallments",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "PendingAmount",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "TotalInstallments",
                table: "Loans");
        }
    }
}
