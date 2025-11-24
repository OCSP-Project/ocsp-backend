using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCSP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDelegateApprovalToProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
<<<<<<< HEAD
            // Only add column if table exists and column doesn't exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM pg_catalog.pg_class c
                        JOIN pg_catalog.pg_namespace n ON n.oid=c.relnamespace
                        WHERE n.nspname='public' AND c.relname='Projects'
                    ) THEN
                        IF NOT EXISTS (
                            SELECT 1 FROM information_schema.columns 
                            WHERE table_schema='public' 
                            AND table_name='Projects' 
                            AND column_name='DelegateApprovalToSupervisor'
                        ) THEN
                            ALTER TABLE ""Projects"" 
                            ADD COLUMN ""DelegateApprovalToSupervisor"" BOOLEAN NOT NULL DEFAULT false;
                            
                            COMMENT ON COLUMN ""Projects"".""DelegateApprovalToSupervisor"" IS 'Homeowner delegates material approval authority to Supervisor (default: false)';
                        END IF;
                    END IF;
                END $$;
            ");
=======
            migrationBuilder.AddColumn<bool>(
                name: "DelegateApprovalToSupervisor",
                table: "Projects",
                type: "boolean",
                nullable: false,
                defaultValue: false);
>>>>>>> 4bb8c9d575355129311ea350af4b1d10c01d0f56
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
<<<<<<< HEAD
            // Only drop column if table and column exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM pg_catalog.pg_class c
                        JOIN pg_catalog.pg_namespace n ON n.oid=c.relnamespace
                        WHERE n.nspname='public' AND c.relname='Projects'
                    ) THEN
                        IF EXISTS (
                            SELECT 1 FROM information_schema.columns 
                            WHERE table_schema='public' 
                            AND table_name='Projects' 
                            AND column_name='DelegateApprovalToSupervisor'
                        ) THEN
                            ALTER TABLE ""Projects"" DROP COLUMN ""DelegateApprovalToSupervisor"";
                        END IF;
                    END IF;
                END $$;
            ");
=======
            migrationBuilder.DropColumn(
                name: "DelegateApprovalToSupervisor",
                table: "Projects");
>>>>>>> 4bb8c9d575355129311ea350af4b1d10c01d0f56
        }
    }
}
