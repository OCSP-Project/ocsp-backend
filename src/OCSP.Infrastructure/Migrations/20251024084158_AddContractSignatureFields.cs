using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCSP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContractSignatureFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "contractorsignaturebase64",
                table: "Contracts",
                type: "character varying(1000000)",
                maxLength: 1000000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "homeownersignaturebase64",
                table: "Contracts",
                type: "character varying(1000000)",
                maxLength: 1000000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "signedpdfurl",
                table: "Contracts",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "templatepdfurl",
                table: "Contracts",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "contractorsignaturebase64",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "homeownersignaturebase64",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "signedpdfurl",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "templatepdfurl",
                table: "Contracts");
        }
    }
}
