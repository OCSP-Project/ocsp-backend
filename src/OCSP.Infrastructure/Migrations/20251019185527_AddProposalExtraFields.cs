using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCSP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProposalExtraFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AverageSalary",
                table: "Proposals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConstructionArea",
                table: "Proposals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConstructionTime",
                table: "Proposals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExcelFileUrl",
                table: "Proposals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasBeenSubmitted",
                table: "Proposals",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WasRevised",
                table: "Proposals",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "AverageSalary", table: "Proposals");
            migrationBuilder.DropColumn(name: "ConstructionArea", table: "Proposals");
            migrationBuilder.DropColumn(name: "ConstructionTime", table: "Proposals");
            migrationBuilder.DropColumn(name: "ExcelFileUrl", table: "Proposals");
            migrationBuilder.DropColumn(name: "HasBeenSubmitted", table: "Proposals");
            migrationBuilder.DropColumn(name: "WasRevised", table: "Proposals");
        }
    }
}
