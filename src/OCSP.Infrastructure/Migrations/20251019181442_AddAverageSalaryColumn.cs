using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCSP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAverageSalaryColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_Projects_ProjectId",
                table: "Conversations");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentTransactions_ContractMilestones_MilestoneId",
                table: "PaymentTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Contractors_ContractorId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_PaymentTransactions_ContractId_MilestoneId_Type",
                table: "PaymentTransactions");

            migrationBuilder.DropIndex(
                name: "IX_PaymentTransactions_Provider_ProviderTxnId",
                table: "PaymentTransactions");

            migrationBuilder.DropIndex(
                name: "IX_ContractMilestones_ContractId_Status",
                table: "ContractMilestones");

            // migrationBuilder.RenameTable(
            //     name: "Milestone",
            //     newName: "Milestones");

            migrationBuilder.AlterColumn<string>(
                name: "ExcelFileName",
                table: "Proposals",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExcelFileUrl",
                table: "Proposals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasBeenSubmitted",
                table: "Proposals",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WasRevised",
                table: "Proposals",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "ProviderTxnId",
                table: "PaymentTransactions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "PaymentTransactions",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "ExternalAccountId",
                table: "EscrowAccounts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Balance",
                table: "EscrowAccounts",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "ContractMilestones",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ContractMilestones",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "ContractMilestones",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            // migrationBuilder.AddColumn<Guid>(
            //     name: "Id",
            //     table: "Milestones",
            //     type: "uuid",
            //     nullable: false,
            //     defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // migrationBuilder.AddColumn<DateTime>(
            //     name: "ActualEndDate",
            //     table: "Milestones",
            //     type: "timestamp with time zone",
            //     nullable: true);

            // migrationBuilder.AddColumn<DateTime>(
            //     name: "ActualStartDate",
            //     table: "Milestones",
            //     type: "timestamp with time zone",
            //     nullable: true);

            // migrationBuilder.AddColumn<DateTime>(
            //     name: "CreatedAt",
            //     table: "Milestones",
            //     type: "timestamp with time zone",
            //     nullable: false,
            //     defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            // migrationBuilder.AddColumn<string>(
            //     name: "CreatedBy",
            //     table: "Milestones",
            //     type: "text",
            //     nullable: true);

            // migrationBuilder.AddColumn<string>(
            //     name: "Description",
            //     table: "Milestones",
            //     type: "character varying(1000)",
            //     maxLength: 1000,
            //     nullable: true);

            // migrationBuilder.AddColumn<string>(
            //     name: "Name",
            //     table: "Milestones",
            //     type: "character varying(200)",
            //     maxLength: 200,
            //     nullable: false,
            //     defaultValue: "");

            // migrationBuilder.AddColumn<DateTime>(
            //     name: "PlannedEndDate",
            //     table: "Milestones",
            //     type: "timestamp with time zone",
            //     nullable: false,
            //     defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            // migrationBuilder.AddColumn<DateTime>(
            //     name: "PlannedStartDate",
            //     table: "Milestones",
            //     type: "timestamp with time zone",
            //     nullable: false,
            //     defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            // migrationBuilder.AddColumn<decimal>(
            //     name: "ProgressPercentage",
            //     table: "Milestones",
            //     type: "numeric(5,2)",
            //     nullable: false,
            //     defaultValue: 0m);

            // migrationBuilder.AddColumn<Guid>(
            //     name: "ProjectTimelineId",
            //     table: "Milestones",
            //     type: "uuid",
            //     nullable: false,
            //     defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Milestones",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Milestones",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Milestones",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Milestones",
                table: "Milestones",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ContractorPosts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractorPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractorPosts_Contractors_ContractorId",
                        column: x => x.ContractorId,
                        principalTable: "Contractors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Deliverables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MilestoneId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PlannedDueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActualCompletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ProgressPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deliverables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Deliverables_Milestones_MilestoneId",
                        column: x => x.MilestoneId,
                        principalTable: "Milestones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgressMedias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProgressUpdateId = table.Column<Guid>(type: "uuid", nullable: true),
                    Url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Caption = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressMedias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgressMedias_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProgressMedias_Users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContractorPostImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractorPostId = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Caption = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractorPostImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractorPostImages_ContractorPosts_ContractorPostId",
                        column: x => x.ContractorPostId,
                        principalTable: "ContractorPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_ContractId",
                table: "PaymentTransactions",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractMilestones_ContractId",
                table: "ContractMilestones",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_Milestones_ProjectTimelineId",
                table: "Milestones",
                column: "ProjectTimelineId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractorPostImages_ContractorPostId",
                table: "ContractorPostImages",
                column: "ContractorPostId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractorPosts_ContractorId_CreatedAt",
                table: "ContractorPosts",
                columns: new[] { "ContractorId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Deliverables_MilestoneId",
                table: "Deliverables",
                column: "MilestoneId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressMedias_CreatorId",
                table: "ProgressMedias",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressMedias_ProjectId_CreatedAt",
                table: "ProgressMedias",
                columns: new[] { "ProjectId", "CreatedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_Projects_ProjectId",
                table: "Conversations",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Milestones_ProjectTimelines_ProjectTimelineId",
                table: "Milestones",
                column: "ProjectTimelineId",
                principalTable: "ProjectTimelines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentTransactions_ContractMilestones_MilestoneId",
                table: "PaymentTransactions",
                column: "MilestoneId",
                principalTable: "ContractMilestones",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Contractors_ContractorId",
                table: "Projects",
                column: "ContractorId",
                principalTable: "Contractors",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_Projects_ProjectId",
                table: "Conversations");

            migrationBuilder.DropForeignKey(
                name: "FK_Milestones_ProjectTimelines_ProjectTimelineId",
                table: "Milestones");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentTransactions_ContractMilestones_MilestoneId",
                table: "PaymentTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Contractors_ContractorId",
                table: "Projects");

            migrationBuilder.DropTable(
                name: "ContractorPostImages");

            migrationBuilder.DropTable(
                name: "Deliverables");

            migrationBuilder.DropTable(
                name: "ProgressMedias");

            migrationBuilder.DropTable(
                name: "ContractorPosts");

            migrationBuilder.DropIndex(
                name: "IX_PaymentTransactions_ContractId",
                table: "PaymentTransactions");

            migrationBuilder.DropIndex(
                name: "IX_ContractMilestones_ContractId",
                table: "ContractMilestones");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Milestones",
                table: "Milestones");

            migrationBuilder.DropIndex(
                name: "IX_Milestones_ProjectTimelineId",
                table: "Milestones");

            migrationBuilder.DropColumn(
                name: "ExcelFileUrl",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "HasBeenSubmitted",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "WasRevised",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Milestones");

            migrationBuilder.DropColumn(
                name: "ActualEndDate",
                table: "Milestones");

            migrationBuilder.DropColumn(
                name: "ActualStartDate",
                table: "Milestones");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Milestones");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Milestones");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Milestones");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Milestones");

            migrationBuilder.DropColumn(
                name: "PlannedEndDate",
                table: "Milestones");

            migrationBuilder.DropColumn(
                name: "PlannedStartDate",
                table: "Milestones");

            migrationBuilder.DropColumn(
                name: "ProgressPercentage",
                table: "Milestones");

            migrationBuilder.DropColumn(
                name: "ProjectTimelineId",
                table: "Milestones");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Milestones");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Milestones");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Milestones");

            // migrationBuilder.RenameTable(
            //     name: "Milestones",
            //     newName: "Milestone");

            migrationBuilder.AlterColumn<string>(
                name: "ExcelFileName",
                table: "Proposals",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProviderTxnId",
                table: "PaymentTransactions",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "PaymentTransactions",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "ExternalAccountId",
                table: "EscrowAccounts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Balance",
                table: "EscrowAccounts",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "ContractMilestones",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ContractMilestones",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "ContractMilestones",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_ContractId_MilestoneId_Type",
                table: "PaymentTransactions",
                columns: new[] { "ContractId", "MilestoneId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_Provider_ProviderTxnId",
                table: "PaymentTransactions",
                columns: new[] { "Provider", "ProviderTxnId" });

            migrationBuilder.CreateIndex(
                name: "IX_ContractMilestones_ContractId_Status",
                table: "ContractMilestones",
                columns: new[] { "ContractId", "Status" });

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_Projects_ProjectId",
                table: "Conversations",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentTransactions_ContractMilestones_MilestoneId",
                table: "PaymentTransactions",
                column: "MilestoneId",
                principalTable: "ContractMilestones",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Contractors_ContractorId",
                table: "Projects",
                column: "ContractorId",
                principalTable: "Contractors",
                principalColumn: "Id");
        }
    }
}
