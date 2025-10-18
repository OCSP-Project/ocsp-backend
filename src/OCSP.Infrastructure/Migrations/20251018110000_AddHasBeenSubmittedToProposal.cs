using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCSP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHasBeenSubmittedToProposal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasBeenSubmitted",
                table: "Proposals",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasBeenSubmitted",
                table: "Proposals");
        }
    }
}
