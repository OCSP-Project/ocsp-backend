using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCSP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingGoogleMapsFieldsToContractor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Use IF NOT EXISTS to avoid errors if columns already exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'Contractors' AND column_name = 'GoogleMapsRating'
                    ) THEN
                        ALTER TABLE ""Contractors"" ADD ""GoogleMapsRating"" numeric;
                    END IF;

                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'Contractors' AND column_name = 'GoogleMapsReviewCount'
                    ) THEN
                        ALTER TABLE ""Contractors"" ADD ""GoogleMapsReviewCount"" integer;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Use IF EXISTS to avoid errors during rollback
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'Contractors' AND column_name = 'GoogleMapsRating'
                    ) THEN
                        ALTER TABLE ""Contractors"" DROP COLUMN ""GoogleMapsRating"";
                    END IF;

                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'Contractors' AND column_name = 'GoogleMapsReviewCount'
                    ) THEN
                        ALTER TABLE ""Contractors"" DROP COLUMN ""GoogleMapsReviewCount"";
                    END IF;
                END $$;
            ");
        }
    }
}
