using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCSP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProposalProjectInfoFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProjectTitle",
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
                name: "NumberOfWorkers",
                table: "Proposals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AverageSalary",
                table: "Proposals",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProjectTitle",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "ConstructionArea",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "ConstructionTime",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "NumberOfWorkers",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "AverageSalary",
                table: "Proposals");
        }
    }
}
