using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCSP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContractSignatureEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "Proposals",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ContractSignatures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    SignatureBase64 = table.Column<string>(type: "text", nullable: false),
                    SignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractSignatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractSignatures_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_ProjectId",
                table: "Proposals",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractSignatures_ContractId",
                table: "ContractSignatures",
                column: "ContractId");

            // Gán tạm ProjectId hợp lệ cho các proposal chưa có ProjectId hoặc có giá trị rỗng
migrationBuilder.Sql(@"
    UPDATE ""Proposals""
    SET ""ProjectId"" = (
        SELECT ""Id"" FROM ""Projects"" LIMIT 1
    )
    WHERE ""ProjectId"" IS NULL
       OR ""ProjectId"" = '00000000-0000-0000-0000-000000000000'
       OR ""ProjectId"" NOT IN (SELECT ""Id"" FROM ""Projects"");
");

    

            migrationBuilder.AddForeignKey(
                name: "FK_Proposals_Projects_ProjectId",
                table: "Proposals",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Proposals_Projects_ProjectId",
                table: "Proposals");

            migrationBuilder.DropTable(
                name: "ContractSignatures");

            migrationBuilder.DropIndex(
                name: "IX_Proposals_ProjectId",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Proposals");
        }
    }
}
