using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCSP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorProposalItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop existing columns
            migrationBuilder.DropColumn(
                name: "Qty",
                table: "ProposalItems");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "ProposalItems");

            // Rename UnitPrice to Price
            migrationBuilder.RenameColumn(
                name: "UnitPrice",
                table: "ProposalItems",
                newName: "Price");

            // Add Notes column
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "ProposalItems",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove Notes column
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "ProposalItems");

            // Rename Price back to UnitPrice
            migrationBuilder.RenameColumn(
                name: "Price",
                table: "ProposalItems",
                newName: "UnitPrice");

            // Add back the dropped columns
            migrationBuilder.AddColumn<decimal>(
                name: "Qty",
                table: "ProposalItems",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "ProposalItems",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
