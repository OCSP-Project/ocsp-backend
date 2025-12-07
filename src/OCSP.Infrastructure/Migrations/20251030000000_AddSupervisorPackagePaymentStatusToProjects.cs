using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCSP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSupervisorPackagePaymentStatusToProjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if column exists before adding (idempotent migration)
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'Projects' 
                        AND column_name = 'SupervisorPackagePaymentStatus'
                    ) THEN
                        ALTER TABLE ""Projects"" 
                        ADD COLUMN ""SupervisorPackagePaymentStatus"" integer NOT NULL DEFAULT 0;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SupervisorPackagePaymentStatus",
                table: "Projects");
        }
    }
}

