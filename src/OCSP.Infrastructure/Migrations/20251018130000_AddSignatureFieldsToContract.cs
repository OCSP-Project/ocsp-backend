using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCSP.Infrastructure.Migrations
{
    public partial class AddSignatureFieldsToContract : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add signature and PDF fields to Contracts table using raw SQL
            migrationBuilder.Sql(@"
                ALTER TABLE ""Contracts"" 
                ADD COLUMN IF NOT EXISTS ""HomeownerSignatureBase64"" text NULL,
                ADD COLUMN IF NOT EXISTS ""ContractorSignatureBase64"" text NULL,
                ADD COLUMN IF NOT EXISTS ""TemplatePdfUrl"" text NULL,
                ADD COLUMN IF NOT EXISTS ""SignedPdfUrl"" text NULL;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove signature and PDF fields from Contracts table
            migrationBuilder.Sql(@"
                ALTER TABLE ""Contracts"" 
                DROP COLUMN IF EXISTS ""HomeownerSignatureBase64"",
                DROP COLUMN IF EXISTS ""ContractorSignatureBase64"",
                DROP COLUMN IF EXISTS ""TemplatePdfUrl"",
                DROP COLUMN IF EXISTS ""SignedPdfUrl"";
            ");
        }
    }
}

