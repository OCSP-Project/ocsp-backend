using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCSP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEscrowBalanceAndPaymentEnums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Raw",
                table: "PaymentTransactions",
                newName: "Note");

            migrationBuilder.AddColumn<decimal>(
                name: "Balance",
                table: "EscrowAccounts",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Balance",
                table: "EscrowAccounts");

            migrationBuilder.RenameColumn(
                name: "Note",
                table: "PaymentTransactions",
                newName: "Raw");
        }
    }
}
