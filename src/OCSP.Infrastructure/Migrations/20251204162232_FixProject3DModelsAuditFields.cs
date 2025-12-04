using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCSP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixProject3DModelsAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Convert CreatedBy and UpdatedBy from uuid to text
            migrationBuilder.Sql(@"
                ALTER TABLE ""Project3DModels""
                ALTER COLUMN ""CreatedBy"" TYPE text USING ""CreatedBy""::text;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Project3DModels""
                ALTER COLUMN ""UpdatedBy"" TYPE text USING ""UpdatedBy""::text;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback: Convert back from text to uuid
            migrationBuilder.Sql(@"
                ALTER TABLE ""Project3DModels""
                ALTER COLUMN ""CreatedBy"" TYPE uuid USING ""CreatedBy""::uuid;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Project3DModels""
                ALTER COLUMN ""UpdatedBy"" TYPE uuid USING ""UpdatedBy""::uuid;
            ");
        }
    }
}
