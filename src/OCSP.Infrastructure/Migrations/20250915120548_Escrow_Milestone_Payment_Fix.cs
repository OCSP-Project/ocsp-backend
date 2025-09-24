using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCSP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Escrow_Milestone_Payment_Fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename cột Note -> Description
            migrationBuilder.RenameColumn(
                name: "Note",
                table: "PaymentTransactions",
                newName: "Description");

            // --- PaymentTransactions.Provider ---
            // Chuyển dữ liệu text sang số (enum PaymentProvider)
            migrationBuilder.Sql(@"
                UPDATE ""PaymentTransactions""
                SET ""Provider"" = CASE
                    WHEN ""Provider"" ILIKE 'vnpay'   THEN '1'
                    WHEN ""Provider"" ILIKE 'zalopay' THEN '2'
                    WHEN ""Provider"" ~ '^\d+$'       THEN ""Provider""
                    ELSE '0'
                END;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""PaymentTransactions""
                ALTER COLUMN ""Provider"" TYPE integer USING ""Provider""::integer;
            ");

            // --- EscrowAccounts.Provider ---
            migrationBuilder.Sql(@"
                UPDATE ""EscrowAccounts""
                SET ""Provider"" = CASE
                    WHEN ""Provider"" ILIKE 'vnpay'   THEN '1'
                    WHEN ""Provider"" ILIKE 'zalopay' THEN '2'
                    WHEN ""Provider"" ~ '^\d+$'       THEN ""Provider""
                    ELSE '0'
                END;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""EscrowAccounts""
                ALTER COLUMN ""Provider"" TYPE integer USING ""Provider""::integer;
            ");

            // ContractMilestones.Note -> varchar(500)
            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "ContractMilestones",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // revert Description -> Note
            migrationBuilder.RenameColumn(
                name: "Description",
                table: "PaymentTransactions",
                newName: "Note");

            // revert PaymentTransactions.Provider về string
            migrationBuilder.AlterColumn<string>(
                name: "Provider",
                table: "PaymentTransactions",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            // revert EscrowAccounts.Provider về string
            migrationBuilder.AlterColumn<string>(
                name: "Provider",
                table: "EscrowAccounts",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            // revert ContractMilestones.Note về text
            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "ContractMilestones",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
