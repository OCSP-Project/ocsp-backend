using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCSP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFeaturedImageUrlToContractor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FeaturedImageUrl",
                table: "Contractors",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FeaturedImageUrl",
                table: "Contractors");
        }
    }
}
