using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCSP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorProposalItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop existing columns if they exist
            migrationBuilder.Sql(@"DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' AND table_name = 'ProposalItems' AND column_name = 'Qty'
    ) THEN
        ALTER TABLE ""ProposalItems"" DROP COLUMN ""Qty"";
    END IF;
END $$;");

            migrationBuilder.Sql(@"DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' AND table_name = 'ProposalItems' AND column_name = 'Unit'
    ) THEN
        ALTER TABLE ""ProposalItems"" DROP COLUMN ""Unit"";
    END IF;
END $$;");

            // Rename UnitPrice to Price if UnitPrice exists and Price doesn't
            migrationBuilder.Sql(@"DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' AND table_name = 'ProposalItems' AND column_name = 'UnitPrice'
    ) AND NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' AND table_name = 'ProposalItems' AND column_name = 'Price'
    ) THEN
        ALTER TABLE ""ProposalItems"" RENAME COLUMN ""UnitPrice"" TO ""Price"";
    END IF;
END $$;");

            // Ensure Notes column exists
            migrationBuilder.Sql("ALTER TABLE \"ProposalItems\" ADD COLUMN IF NOT EXISTS \"Notes\" text;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove Notes column
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "ProposalItems");

            // Rename Price back to UnitPrice
            migrationBuilder.RenameColumn(
                name: "Price",
                table: "ProposalItems",
                newName: "UnitPrice");

            // Add back the dropped columns
            migrationBuilder.AddColumn<decimal>(
                name: "Qty",
                table: "ProposalItems",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "ProposalItems",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
