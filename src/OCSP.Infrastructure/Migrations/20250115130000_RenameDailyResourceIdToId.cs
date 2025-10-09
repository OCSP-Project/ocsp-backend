using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCSP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameDailyResourceIdToId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectDailyResources",
                table: "ProjectDailyResources");

            migrationBuilder.RenameColumn(
                name: "DailyResourceId",
                table: "ProjectDailyResources",
                newName: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectDailyResources",
                table: "ProjectDailyResources",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectDailyResources",
                table: "ProjectDailyResources");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ProjectDailyResources",
                newName: "DailyResourceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectDailyResources",
                table: "ProjectDailyResources",
                column: "DailyResourceId");
        }
    }
}
