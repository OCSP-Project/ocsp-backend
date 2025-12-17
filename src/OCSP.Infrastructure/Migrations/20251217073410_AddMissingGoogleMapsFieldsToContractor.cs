using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCSP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingGoogleMapsFieldsToContractor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "GoogleMapsRating",
                table: "Contractors",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GoogleMapsReviewCount",
                table: "Contractors",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoogleMapsRating",
                table: "Contractors");

            migrationBuilder.DropColumn(
                name: "GoogleMapsReviewCount",
                table: "Contractors");
        }
    }
}
