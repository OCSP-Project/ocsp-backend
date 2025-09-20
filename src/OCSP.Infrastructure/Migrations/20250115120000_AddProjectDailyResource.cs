using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCSP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectDailyResource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectDailyResources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TowerCrane = table.Column<bool>(type: "boolean", nullable: false),
                    ConcreteMixer = table.Column<bool>(type: "boolean", nullable: false),
                    MaterialHoist = table.Column<bool>(type: "boolean", nullable: false),
                    PassengerHoist = table.Column<bool>(type: "boolean", nullable: false),
                    Vibrator = table.Column<bool>(type: "boolean", nullable: false),
                    CementConsumed = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CementRemaining = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SandConsumed = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SandRemaining = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AggregateConsumed = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AggregateRemaining = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectDailyResources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectDailyResources_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDailyResources_ProjectId_ResourceDate",
                table: "ProjectDailyResources",
                columns: new[] { "ProjectId", "ResourceDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDailyResources_ResourceDate",
                table: "ProjectDailyResources",
                column: "ResourceDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectDailyResources");
        }
    }
}
