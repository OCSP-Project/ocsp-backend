using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCSP.Infrastructure.Migrations
{
    public partial class UpdateProposalItemsSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Nếu cũ còn các cột Unit, Qty, UnitPrice -> xoá đi
            migrationBuilder.DropColumn(
                name: "Unit",
                table: "ProposalItems");

            migrationBuilder.DropColumn(
                name: "Qty",
                table: "ProposalItems");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "ProposalItems");

            // Thêm cột mới theo entity hiện tại
            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "ProposalItems",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            // migrationBuilder.AddColumn<string>(
            //     name: "Notes",
            //     table: "ProposalItems",
            //     type: "text",
            //     nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Price", table: "ProposalItems");
            // migrationBuilder.DropColumn(name: "Notes", table: "ProposalItems");

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "ProposalItems",
                type: "character varying(50)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Qty",
                table: "ProposalItems",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "ProposalItems",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
