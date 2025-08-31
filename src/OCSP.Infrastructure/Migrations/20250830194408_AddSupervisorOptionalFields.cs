using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCSP.Infrastructure.Migrations
{
    public partial class AddSupervisorOptionalFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "Supervisors",
                type: "varchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Rating",
                table: "Supervisors",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewsCount",
                table: "Supervisors",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinRate",
                table: "Supervisors",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxRate",
                table: "Supervisors",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AvailableNow",
                table: "Supervisors",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableNow",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "MaxRate",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "MinRate",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "ReviewsCount",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "District",
                table: "Supervisors");
        }
    }
}
