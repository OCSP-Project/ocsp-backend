using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCSP.Infrastructure.Migrations
{
    public partial class UpdateProposalItemsSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop legacy columns only if they still exist
            migrationBuilder.Sql(@"DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' AND table_name = 'ProposalItems' AND column_name = 'Unit'
    ) THEN
        ALTER TABLE ""ProposalItems"" DROP COLUMN ""Unit"";
    END IF;
END $$;");

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
        WHERE table_schema = 'public' AND table_name = 'ProposalItems' AND column_name = 'UnitPrice'
    ) THEN
        ALTER TABLE ""ProposalItems"" DROP COLUMN ""UnitPrice"";
    END IF;
END $$;");

            // Ensure Price column exists
            migrationBuilder.Sql("ALTER TABLE \"ProposalItems\" ADD COLUMN IF NOT EXISTS \"Price\" numeric NOT NULL DEFAULT 0;");

            // Notes column (optional) — keep commented unless needed
            // migrationBuilder.Sql("ALTER TABLE \"ProposalItems\" ADD COLUMN IF NOT EXISTS \"Notes\" text;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Price", table: "ProposalItems");
            // migrationBuilder.DropColumn(name: "Notes", table: "ProposalItems");

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "ProposalItems",
                type: "character varying(50)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Qty",
                table: "ProposalItems",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "ProposalItems",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
