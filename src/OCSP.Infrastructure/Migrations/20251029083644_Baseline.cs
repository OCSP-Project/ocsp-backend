using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCSP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Baseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Baseline migration: intentionally left empty to avoid touching existing schema
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Baseline migration: no-op
        }
    }
}
