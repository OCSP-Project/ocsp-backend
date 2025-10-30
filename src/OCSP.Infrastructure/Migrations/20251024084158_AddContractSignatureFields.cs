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
            migrationBuilder.Sql("ALTER TABLE \"Contracts\" ADD COLUMN IF NOT EXISTS \"contractorsignaturebase64\" character varying(1000000);");
            migrationBuilder.Sql("ALTER TABLE \"Contracts\" ADD COLUMN IF NOT EXISTS \"homeownersignaturebase64\" character varying(1000000);");
            migrationBuilder.Sql("ALTER TABLE \"Contracts\" ADD COLUMN IF NOT EXISTS \"signedpdfurl\" character varying(1000);");
            migrationBuilder.Sql("ALTER TABLE \"Contracts\" ADD COLUMN IF NOT EXISTS \"templatepdfurl\" character varying(1000);");
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
