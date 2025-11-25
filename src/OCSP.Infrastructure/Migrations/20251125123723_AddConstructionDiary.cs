using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCSP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConstructionDiary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConstructionDiaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    DiaryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConstructionTeam = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SafetyRating = table.Column<int>(type: "integer", nullable: false),
                    QualityRating = table.Column<int>(type: "integer", nullable: false),
                    ProgressRating = table.Column<int>(type: "integer", nullable: false),
                    CleanlinessRating = table.Column<int>(type: "integer", nullable: false),
                    IncidentReport = table.Column<string>(type: "text", nullable: true),
                    Recommendations = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    SupervisorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SupervisorPosition = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ContractorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SupervisorUnitName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConstructionDiaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConstructionDiaries_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiaryImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConstructionDiaryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiaryImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiaryImages_ConstructionDiaries_ConstructionDiaryId",
                        column: x => x.ConstructionDiaryId,
                        principalTable: "ConstructionDiaries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiaryWeatherPeriods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConstructionDiaryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Period = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Condition = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Temperature = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiaryWeatherPeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiaryWeatherPeriods_ConstructionDiaries_ConstructionDiaryId",
                        column: x => x.ConstructionDiaryId,
                        principalTable: "ConstructionDiaries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiaryWorkItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConstructionDiaryId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkItemName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ConstructionArea = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PlannedQuantity = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ConstructedQuantity = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    RemainingQuantity = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiaryWorkItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiaryWorkItems_ConstructionDiaries_ConstructionDiaryId",
                        column: x => x.ConstructionDiaryId,
                        principalTable: "ConstructionDiaries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiaryWorkItems_WorkItems_WorkItemId",
                        column: x => x.WorkItemId,
                        principalTable: "WorkItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DiaryEquipments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DiaryWorkItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Specifications = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    HoursUsed = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EquipmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiaryEquipments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiaryEquipments_DiaryWorkItems_DiaryWorkItemId",
                        column: x => x.DiaryWorkItemId,
                        principalTable: "DiaryWorkItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiaryLabors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DiaryWorkItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    LaborName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Position = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    WorkHours = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Team = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Shift = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LaborId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiaryLabors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiaryLabors_DiaryWorkItems_DiaryWorkItemId",
                        column: x => x.DiaryWorkItemId,
                        principalTable: "DiaryWorkItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConstructionDiaries_DiaryDate",
                table: "ConstructionDiaries",
                column: "DiaryDate");

            migrationBuilder.CreateIndex(
                name: "IX_ConstructionDiaries_ProjectId_DiaryDate",
                table: "ConstructionDiaries",
                columns: new[] { "ProjectId", "DiaryDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiaryEquipments_DiaryWorkItemId",
                table: "DiaryEquipments",
                column: "DiaryWorkItemId");

            migrationBuilder.CreateIndex(
                name: "IX_DiaryImages_Category",
                table: "DiaryImages",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_DiaryImages_ConstructionDiaryId",
                table: "DiaryImages",
                column: "ConstructionDiaryId");

            migrationBuilder.CreateIndex(
                name: "IX_DiaryImages_UploadedAt",
                table: "DiaryImages",
                column: "UploadedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DiaryLabors_DiaryWorkItemId",
                table: "DiaryLabors",
                column: "DiaryWorkItemId");

            migrationBuilder.CreateIndex(
                name: "IX_DiaryWeatherPeriods_ConstructionDiaryId",
                table: "DiaryWeatherPeriods",
                column: "ConstructionDiaryId");

            migrationBuilder.CreateIndex(
                name: "IX_DiaryWorkItems_ConstructionDiaryId",
                table: "DiaryWorkItems",
                column: "ConstructionDiaryId");

            migrationBuilder.CreateIndex(
                name: "IX_DiaryWorkItems_WorkItemId",
                table: "DiaryWorkItems",
                column: "WorkItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiaryEquipments");

            migrationBuilder.DropTable(
                name: "DiaryImages");

            migrationBuilder.DropTable(
                name: "DiaryLabors");

            migrationBuilder.DropTable(
                name: "DiaryWeatherPeriods");

            migrationBuilder.DropTable(
                name: "DiaryWorkItems");

            migrationBuilder.DropTable(
                name: "ConstructionDiaries");
        }
    }
}
