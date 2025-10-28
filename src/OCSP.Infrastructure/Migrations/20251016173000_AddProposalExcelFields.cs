using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCSP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProposalExcelFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add new columns to existing Proposals table for Excel-based proposals
            migrationBuilder.AddColumn<bool>(
                name: "IsFromExcel",
                table: "Proposals",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ExcelFileName",
                table: "Proposals",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            // Add Notes column to ProposalItems table
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "ProposalItems",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove columns from Proposals table
            migrationBuilder.DropColumn(
                name: "IsFromExcel",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "ExcelFileName",
                table: "Proposals");

            // Remove Notes column from ProposalItems table
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "ProposalItems");
        }
    }
}
