using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCSP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectInvitations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DetailedRole",
                table: "ProjectParticipants",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ProjectInvitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    InviteeEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    InviteeUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    InvitedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    InvitationToken = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectInvitations_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectInvitations_Users_InvitedBy",
                        column: x => x.InvitedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectInvitations_Users_InviteeUserId",
                        column: x => x.InviteeUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectInvitations_ExpiresAt",
                table: "ProjectInvitations",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectInvitations_InvitationToken",
                table: "ProjectInvitations",
                column: "InvitationToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectInvitations_InvitedBy",
                table: "ProjectInvitations",
                column: "InvitedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectInvitations_InviteeUserId",
                table: "ProjectInvitations",
                column: "InviteeUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectInvitations_ProjectId_InviteeEmail",
                table: "ProjectInvitations",
                columns: new[] { "ProjectId", "InviteeEmail" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectInvitations_ProjectId_Status",
                table: "ProjectInvitations",
                columns: new[] { "ProjectId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectInvitations");

            migrationBuilder.DropColumn(
                name: "DetailedRole",
                table: "ProjectParticipants");
        }
    }
}
