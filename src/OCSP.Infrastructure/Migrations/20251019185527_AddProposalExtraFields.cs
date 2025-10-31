using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCSP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProposalExtraFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Use idempotent guards to avoid duplicate column errors when environments drift
            migrationBuilder.Sql("ALTER TABLE \"Proposals\" ADD COLUMN IF NOT EXISTS \"AverageSalary\" text;");
            migrationBuilder.Sql("ALTER TABLE \"Proposals\" ADD COLUMN IF NOT EXISTS \"ConstructionArea\" text;");
            migrationBuilder.Sql("ALTER TABLE \"Proposals\" ADD COLUMN IF NOT EXISTS \"ConstructionTime\" text;");
            migrationBuilder.Sql("ALTER TABLE \"Proposals\" ADD COLUMN IF NOT EXISTS \"ExcelFileUrl\" text;");
            migrationBuilder.Sql("ALTER TABLE \"Proposals\" ADD COLUMN IF NOT EXISTS \"HasBeenSubmitted\" boolean NOT NULL DEFAULT false;");
            migrationBuilder.Sql("ALTER TABLE \"Proposals\" ADD COLUMN IF NOT EXISTS \"WasRevised\" boolean NOT NULL DEFAULT false;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "AverageSalary", table: "Proposals");
            migrationBuilder.DropColumn(name: "ConstructionArea", table: "Proposals");
            migrationBuilder.DropColumn(name: "ConstructionTime", table: "Proposals");
            migrationBuilder.DropColumn(name: "ExcelFileUrl", table: "Proposals");
            migrationBuilder.DropColumn(name: "HasBeenSubmitted", table: "Proposals");
            migrationBuilder.DropColumn(name: "WasRevised", table: "Proposals");
        }
    }
}
