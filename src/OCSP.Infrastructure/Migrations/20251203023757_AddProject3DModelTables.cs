using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCSP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProject3DModelTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create Project3DModels table
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""Project3DModels"" (
                    ""Id"" uuid NOT NULL PRIMARY KEY,
                    ""ProjectId"" uuid NOT NULL,
                    ""FileName"" varchar(500) NOT NULL,
                    ""FileUrl"" varchar(2000) NOT NULL,
                    ""FileSizeMB"" numeric(10,2) NOT NULL,
                    ""TotalMeshes"" integer NOT NULL DEFAULT 0,
                    ""AnalysisCompleted"" boolean NOT NULL DEFAULT false,
                    ""AnalyzedAt"" timestamp with time zone,
                    ""AnalysisResultJson"" jsonb,
                    ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""CreatedBy"" uuid,
                    ""UpdatedBy"" uuid,
                    CONSTRAINT ""FK_Project3DModels_Projects_ProjectId"" FOREIGN KEY (""ProjectId"")
                        REFERENCES ""Projects""(""Id"") ON DELETE CASCADE
                );

                CREATE INDEX IF NOT EXISTS ""IX_Project3DModels_ProjectId"" ON ""Project3DModels""(""ProjectId"");
                CREATE INDEX IF NOT EXISTS ""IX_Project3DModels_AnalysisCompleted"" ON ""Project3DModels""(""AnalysisCompleted"");
                CREATE INDEX IF NOT EXISTS ""IX_Project3DModels_AnalyzedAt"" ON ""Project3DModels""(""AnalyzedAt"");
            ");

            // Create BuildingElements table
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""BuildingElements"" (
                    ""Id"" uuid NOT NULL PRIMARY KEY,
                    ""ModelId"" uuid NOT NULL,
                    ""Name"" varchar(200) NOT NULL,
                    ""ElementType"" integer NOT NULL,
                    ""Width"" numeric(10,3) NOT NULL,
                    ""Length"" numeric(10,3) NOT NULL,
                    ""Height"" numeric(10,3) NOT NULL,
                    ""CenterX"" numeric(10,3) NOT NULL,
                    ""CenterY"" numeric(10,3) NOT NULL,
                    ""CenterZ"" numeric(10,3) NOT NULL,
                    ""VolumeM3"" numeric(12,3) NOT NULL,
                    ""FloorLevel"" integer NOT NULL,
                    ""TrackingStatus"" integer NOT NULL DEFAULT 0,
                    ""CompletionPercentage"" numeric(5,2) NOT NULL DEFAULT 0,
                    ""CanTrack"" boolean NOT NULL DEFAULT true,
                    ""Color"" varchar(7) NOT NULL DEFAULT '#CCCCCC',
                    ""MeshIndices"" jsonb NOT NULL,
                    ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""CreatedBy"" uuid,
                    ""UpdatedBy"" uuid,
                    CONSTRAINT ""FK_BuildingElements_Project3DModels_ModelId"" FOREIGN KEY (""ModelId"")
                        REFERENCES ""Project3DModels""(""Id"") ON DELETE CASCADE
                );

                CREATE INDEX IF NOT EXISTS ""IX_BuildingElements_ModelId"" ON ""BuildingElements""(""ModelId"");
                CREATE INDEX IF NOT EXISTS ""IX_BuildingElements_ElementType"" ON ""BuildingElements""(""ElementType"");
                CREATE INDEX IF NOT EXISTS ""IX_BuildingElements_TrackingStatus"" ON ""BuildingElements""(""TrackingStatus"");
                CREATE INDEX IF NOT EXISTS ""IX_BuildingElements_FloorLevel"" ON ""BuildingElements""(""FloorLevel"");
                CREATE INDEX IF NOT EXISTS ""IX_BuildingElements_ModelId_CompletionPercentage"" ON ""BuildingElements""(""ModelId"", ""CompletionPercentage"");
                CREATE INDEX IF NOT EXISTS ""IX_BuildingElements_ModelId_TrackingStatus"" ON ""BuildingElements""(""ModelId"", ""TrackingStatus"");
            ");

            // Create MeshGroups table
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""MeshGroups"" (
                    ""Id"" uuid NOT NULL PRIMARY KEY,
                    ""ModelId"" uuid NOT NULL,
                    ""ComponentType"" integer NOT NULL,
                    ""MeshIndicesJson"" jsonb NOT NULL DEFAULT '[]'::jsonb,
                    ""Color"" varchar(7) NOT NULL DEFAULT '#CCCCCC',
                    ""VolumeM3"" numeric(12,3) NOT NULL,
                    ""Unit"" varchar(10) NOT NULL DEFAULT 'm3',
                    ""IsAutoDetected"" boolean NOT NULL DEFAULT true,
                    ""DetectionAlgorithm"" varchar(100),
                    ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""CreatedBy"" uuid,
                    ""UpdatedBy"" uuid,
                    CONSTRAINT ""FK_MeshGroups_Project3DModels_ModelId"" FOREIGN KEY (""ModelId"")
                        REFERENCES ""Project3DModels""(""Id"") ON DELETE CASCADE
                );

                CREATE INDEX IF NOT EXISTS ""IX_MeshGroups_ModelId"" ON ""MeshGroups""(""ModelId"");
                CREATE INDEX IF NOT EXISTS ""IX_MeshGroups_ComponentType"" ON ""MeshGroups""(""ComponentType"");
            ");

            // Create ElementTrackingHistory table
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""ElementTrackingHistory"" (
                    ""Id"" uuid NOT NULL PRIMARY KEY,
                    ""BuildingElementId"" uuid NOT NULL,
                    ""RecordedById"" uuid NOT NULL,
                    ""TrackingDate"" timestamp with time zone NOT NULL,
                    ""PreviousStatus"" integer NOT NULL,
                    ""NewStatus"" integer NOT NULL,
                    ""PreviousPercentage"" numeric(5,2) NOT NULL DEFAULT 0,
                    ""NewPercentage"" numeric(5,2) NOT NULL DEFAULT 0,
                    ""PlannedQuantity"" numeric(10,2),
                    ""ActualQuantity"" numeric(10,2),
                    ""CementUsed"" numeric(10,2),
                    ""SandUsed"" numeric(10,2),
                    ""AggregateUsed"" numeric(10,2),
                    ""Notes"" varchar(2000),
                    ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""CreatedBy"" uuid,
                    ""UpdatedBy"" uuid,
                    CONSTRAINT ""FK_ElementTrackingHistory_BuildingElements_BuildingElementId"" FOREIGN KEY (""BuildingElementId"")
                        REFERENCES ""BuildingElements""(""Id"") ON DELETE CASCADE,
                    CONSTRAINT ""FK_ElementTrackingHistory_Users_RecordedById"" FOREIGN KEY (""RecordedById"")
                        REFERENCES ""Users""(""Id"") ON DELETE RESTRICT
                );

                CREATE INDEX IF NOT EXISTS ""IX_ElementTrackingHistory_BuildingElementId"" ON ""ElementTrackingHistory""(""BuildingElementId"");
                CREATE INDEX IF NOT EXISTS ""IX_ElementTrackingHistory_TrackingDate"" ON ""ElementTrackingHistory""(""TrackingDate"");
                CREATE INDEX IF NOT EXISTS ""IX_ElementTrackingHistory_RecordedById"" ON ""ElementTrackingHistory""(""RecordedById"");
                CREATE INDEX IF NOT EXISTS ""IX_ElementTrackingHistory_BuildingElementId_TrackingDate"" ON ""ElementTrackingHistory""(""BuildingElementId"", ""TrackingDate"");
                CREATE INDEX IF NOT EXISTS ""IX_ElementTrackingHistory_BuildingElementId_NewStatus"" ON ""ElementTrackingHistory""(""BuildingElementId"", ""NewStatus"");
            ");

            // Create TrackingPhotos table
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""TrackingPhotos"" (
                    ""Id"" uuid NOT NULL PRIMARY KEY,
                    ""TrackingHistoryId"" uuid NOT NULL,
                    ""PhotoUrl"" varchar(2000) NOT NULL,
                    ""Caption"" varchar(500),
                    ""FileSizeMB"" numeric(10,2),
                    ""FileType"" varchar(50),
                    ""UploadedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""CreatedBy"" uuid,
                    ""UpdatedBy"" uuid,
                    CONSTRAINT ""FK_TrackingPhotos_ElementTrackingHistory_TrackingHistoryId"" FOREIGN KEY (""TrackingHistoryId"")
                        REFERENCES ""ElementTrackingHistory""(""Id"") ON DELETE CASCADE
                );

                CREATE INDEX IF NOT EXISTS ""IX_TrackingPhotos_TrackingHistoryId"" ON ""TrackingPhotos""(""TrackingHistoryId"");
                CREATE INDEX IF NOT EXISTS ""IX_TrackingPhotos_UploadedAt"" ON ""TrackingPhotos""(""UploadedAt"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop tables in reverse order to respect foreign key constraints
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"TrackingPhotos\";");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"ElementTrackingHistory\";");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"MeshGroups\";");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"BuildingElements\";");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"Project3DModels\";");
        }
    }
}
