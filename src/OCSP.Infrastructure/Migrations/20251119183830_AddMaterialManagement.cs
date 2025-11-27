using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCSP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MaterialRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractorId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ApprovedByHomeowner = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovedByHomeownerId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedByHomeownerAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedBySupervisor = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovedBySupervisorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedBySupervisorAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    FileName = table.Column<string>(type: "text", nullable: true),
                    FileUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialRequests_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaterialRequests_Users_ContractorId",
                        column: x => x.ContractorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });


            // Create SupervisorContracts table only if it doesn't exist
            // (Table may have been created via SQL script: 001_add_supervisor_contracts.sql)
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_catalog.pg_class c
                        JOIN pg_catalog.pg_namespace n ON n.oid=c.relnamespace
                        WHERE n.nspname='public' AND c.relname='SupervisorContracts'
                    ) THEN
                        CREATE TABLE ""SupervisorContracts"" (
                            ""Id"" uuid NOT NULL PRIMARY KEY,
                            ""ProjectId"" uuid NOT NULL,
                            ""SupervisorId"" uuid NOT NULL,
                            ""HomeownerUserId"" uuid NOT NULL,
                            ""SupervisorUserId"" uuid NOT NULL,
                            ""MonthlyPrice"" numeric(18,2) NOT NULL,
                            ""Terms"" character varying(10000) NOT NULL DEFAULT '',
                            ""Status"" integer NOT NULL,
                            ""SignedByHomeownerAt"" timestamp with time zone NULL,
                            ""SignedBySupervisorAt"" timestamp with time zone NULL,
                            ""homeownersignaturebase64"" character varying(1000000) NULL,
                            ""supervisorsignaturebase64"" character varying(1000000) NULL,
                            ""templatepdfurl"" character varying(1000) NULL,
                            ""signedpdfurl"" character varying(1000) NULL,
                            ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT now(),
                            ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT now(),
                            ""CreatedBy"" text NULL,
                            ""UpdatedBy"" text NULL,
                            CONSTRAINT ""FK_SupervisorContracts_Projects_ProjectId""
                                FOREIGN KEY (""ProjectId"") REFERENCES ""Projects""(""Id"") ON DELETE RESTRICT,
                            CONSTRAINT ""FK_SupervisorContracts_Supervisors_SupervisorId""
                                FOREIGN KEY (""SupervisorId"") REFERENCES ""Supervisors""(""Id"") ON DELETE RESTRICT
                        );
                    END IF;
                END $$;
            ");


            migrationBuilder.CreateTable(
                name: "MaterialApprovalHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedById = table.Column<Guid>(type: "uuid", nullable: false),
                    ApproverRole = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    ActionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Comments = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialApprovalHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialApprovalHistories_MaterialRequests_MaterialRequestId",
                        column: x => x.MaterialRequestId,
                        principalTable: "MaterialRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaterialApprovalHistories_Users_ApprovedById",
                        column: x => x.ApprovedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Materials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ContractQuantity = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    EstimatedQuantity = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ActualQuantity = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ContractAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    EstimatedAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ActualAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Supplier = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Materials_MaterialRequests_MaterialRequestId",
                        column: x => x.MaterialRequestId,
                        principalTable: "MaterialRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Materials_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Materials_WorkItems_WorkItemId",
                        column: x => x.WorkItemId,
                        principalTable: "WorkItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MaterialPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PaidQuantity = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    RemainingQuantity = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    RemainingAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    InvoiceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TransactionReference = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    ApprovedBy = table.Column<string>(type: "text", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialPayments_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaterialPayments_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaterialApprovalHistories_ActionDate",
                table: "MaterialApprovalHistories",
                column: "ActionDate");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialApprovalHistories_ApprovedById",
                table: "MaterialApprovalHistories",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialApprovalHistories_MaterialRequestId",
                table: "MaterialApprovalHistories",
                column: "MaterialRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialPayments_MaterialId",
                table: "MaterialPayments",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialPayments_ProjectId_PaymentDate",
                table: "MaterialPayments",
                columns: new[] { "ProjectId", "PaymentDate" });

            migrationBuilder.CreateIndex(
                name: "IX_MaterialRequests_ContractorId",
                table: "MaterialRequests",
                column: "ContractorId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialRequests_ProjectId_Status",
                table: "MaterialRequests",
                columns: new[] { "ProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_MaterialRequests_RequestDate",
                table: "MaterialRequests",
                column: "RequestDate");

            migrationBuilder.CreateIndex(
                name: "IX_Materials_MaterialRequestId",
                table: "Materials",
                column: "MaterialRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Materials_ProjectId_Code",
                table: "Materials",
                columns: new[] { "ProjectId", "Code" });

            migrationBuilder.CreateIndex(
                name: "IX_Materials_WorkItemId",
                table: "Materials",
                column: "WorkItemId");


            // Create indexes for SupervisorContracts only if table exists and indexes don't exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM pg_catalog.pg_class c
                        JOIN pg_catalog.pg_namespace n ON n.oid=c.relnamespace
                        WHERE n.nspname='public' AND c.relname='SupervisorContracts'
                    ) THEN
                        IF NOT EXISTS (
                            SELECT 1 FROM pg_indexes 
                            WHERE tablename='SupervisorContracts' AND indexname='IX_SupervisorContracts_HomeownerUserId'
                        ) THEN
                            CREATE INDEX ""IX_SupervisorContracts_HomeownerUserId"" ON ""SupervisorContracts"" (""HomeownerUserId"");
                        END IF;

                        IF NOT EXISTS (
                            SELECT 1 FROM pg_indexes 
                            WHERE tablename='SupervisorContracts' AND indexname='IX_SupervisorContracts_ProjectId'
                        ) THEN
                            CREATE INDEX ""IX_SupervisorContracts_ProjectId"" ON ""SupervisorContracts"" (""ProjectId"");
                        END IF;

                        IF NOT EXISTS (
                            SELECT 1 FROM pg_indexes 
                            WHERE tablename='SupervisorContracts' AND indexname='IX_SupervisorContracts_Status'
                        ) THEN
                            CREATE INDEX ""IX_SupervisorContracts_Status"" ON ""SupervisorContracts"" (""Status"");
                        END IF;

                        IF NOT EXISTS (
                            SELECT 1 FROM pg_indexes 
                            WHERE tablename='SupervisorContracts' AND indexname='IX_SupervisorContracts_SupervisorId'
                        ) THEN
                            CREATE INDEX ""IX_SupervisorContracts_SupervisorId"" ON ""SupervisorContracts"" (""SupervisorId"");
                        END IF;

                        IF NOT EXISTS (
                            SELECT 1 FROM pg_indexes 
                            WHERE tablename='SupervisorContracts' AND indexname='IX_SupervisorContracts_SupervisorUserId'
                        ) THEN
                            CREATE INDEX ""IX_SupervisorContracts_SupervisorUserId"" ON ""SupervisorContracts"" (""SupervisorUserId"");
                        END IF;
                    END IF;
                END $$;
            ");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaterialApprovalHistories");

            migrationBuilder.DropTable(
                name: "MaterialPayments");


            // Only drop SupervisorContracts if it exists (may have been created via SQL script)
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM pg_catalog.pg_class c
                        JOIN pg_catalog.pg_namespace n ON n.oid=c.relnamespace
                        WHERE n.nspname='public' AND c.relname='SupervisorContracts'
                    ) THEN
                        DROP TABLE IF EXISTS ""SupervisorContracts"" CASCADE;
                    END IF;
                END $$;
            ");


            migrationBuilder.DropTable(
                name: "Materials");

            migrationBuilder.DropTable(
                name: "MaterialRequests");
        }
    }
}
