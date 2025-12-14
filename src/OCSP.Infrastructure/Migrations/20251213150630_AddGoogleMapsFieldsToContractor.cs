using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCSP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleMapsFieldsToContractor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GoogleMapsDataId",
                table: "Contractors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GoogleMapsPlaceUrl",
                table: "Contractors",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoogleMapsDataId",
                table: "Contractors");

            migrationBuilder.DropColumn(
                name: "GoogleMapsPlaceUrl",
                table: "Contractors");
        }
    }
}
