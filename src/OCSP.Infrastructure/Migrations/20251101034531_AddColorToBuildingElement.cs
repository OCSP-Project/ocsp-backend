using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCSP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddColorToBuildingElement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Only add column if table exists and column doesn't exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM pg_catalog.pg_class c
                        JOIN pg_catalog.pg_namespace n ON n.oid=c.relnamespace
                        WHERE n.nspname='public' AND c.relname='BuildingElements'
                    ) THEN
                        IF NOT EXISTS (
                            SELECT 1 FROM information_schema.columns 
                            WHERE table_schema='public' 
                            AND table_name='BuildingElements' 
                            AND column_name='Color'
                        ) THEN
                            ALTER TABLE ""BuildingElements"" 
                            ADD COLUMN ""Color"" character varying(7) NOT NULL DEFAULT '#CCCCCC';
                            
                            COMMENT ON COLUMN ""BuildingElements"".""Color"" IS 'Hex color code for 3D visualization (format: #RRGGBB, default: #CCCCCC)';
                        END IF;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Only drop column if table and column exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM pg_catalog.pg_class c
                        JOIN pg_catalog.pg_namespace n ON n.oid=c.relnamespace
                        WHERE n.nspname='public' AND c.relname='BuildingElements'
                    ) THEN
                        IF EXISTS (
                            SELECT 1 FROM information_schema.columns 
                            WHERE table_schema='public' 
                            AND table_name='BuildingElements' 
                            AND column_name='Color'
                        ) THEN
                            ALTER TABLE ""BuildingElements"" DROP COLUMN ""Color"";
                        END IF;
                    END IF;
                END $$;
            ");
        }
    }
}
