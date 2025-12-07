using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCSP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistrationRequestsTable_Production : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create table only if it doesn't exist
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""RegistrationRequests"" (
                    ""Id"" uuid NOT NULL PRIMARY KEY,
                    ""Username"" text NOT NULL,
                    ""Email"" text NOT NULL,
                    ""Phone"" text NOT NULL,
                    ""RequestedRole"" integer NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""RejectionReason"" text,
                    ""ReviewedAt"" timestamp with time zone,
                    ""ReviewedByUserId"" uuid,
                    ""Department"" text,
                    ""Position"" text,
                    ""District"" text,
                    ""MinRate"" numeric(18,2),
                    ""MaxRate"" numeric(18,2),
                    ""CompanyName"" text,
                    ""BusinessLicense"" text,
                    ""TaxCode"" text,
                    ""Description"" text,
                    ""Website"" text,
                    ""Address"" text,
                    ""City"" text,
                    ""Province"" text,
                    ""YearsOfExperience"" integer,
                    ""TeamSize"" integer,
                    ""CompletedProjects"" integer,
                    ""MinProjectBudget"" numeric(18,2),
                    ""MaxProjectBudget"" numeric(18,2),
                    ""CreatedUserId"" uuid,
                    ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""UpdatedAt"" timestamp with time zone,
                    ""CreatedBy"" text,
                    ""UpdatedBy"" text,
                    CONSTRAINT ""FK_RegistrationRequests_Users_CreatedUserId"" FOREIGN KEY (""CreatedUserId"")
                        REFERENCES ""Users""(""Id"") ON DELETE RESTRICT,
                    CONSTRAINT ""FK_RegistrationRequests_Users_ReviewedByUserId"" FOREIGN KEY (""ReviewedByUserId"")
                        REFERENCES ""Users""(""Id"") ON DELETE RESTRICT
                );
            ");

            // Create indexes only if they don't exist
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_RegistrationRequests_CreatedUserId"" 
                ON ""RegistrationRequests""(""CreatedUserId"");
                
                CREATE INDEX IF NOT EXISTS ""IX_RegistrationRequests_ReviewedByUserId"" 
                ON ""RegistrationRequests""(""ReviewedByUserId"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrationRequests");
        }
    }
}
