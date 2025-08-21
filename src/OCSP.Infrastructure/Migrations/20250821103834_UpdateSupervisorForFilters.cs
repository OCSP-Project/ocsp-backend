using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCSP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSupervisorForFilters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvgResponseMinutes",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "IsInsured",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "IsLicensed",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "Languages",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "Specializations",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "YearsExperience",
                table: "Supervisors");

            migrationBuilder.RenameColumn(
                name: "State",
                table: "Supervisors",
                newName: "District");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "District",
                table: "Supervisors",
                newName: "State");

            migrationBuilder.AddColumn<int>(
                name: "AvgResponseMinutes",
                table: "Supervisors",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "Supervisors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Supervisors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsInsured",
                table: "Supervisors",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLicensed",
                table: "Supervisors",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Supervisors",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Languages",
                table: "Supervisors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Supervisors",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Supervisors",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Specializations",
                table: "Supervisors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YearsExperience",
                table: "Supervisors",
                type: "integer",
                nullable: true);
        }
    }
}
