-- ========================================
-- 3D MODEL TRACKING TABLES MIGRATION
-- ========================================

-- 1. Project3DModels table
CREATE TABLE "Project3DModels" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "ProjectId" UUID NOT NULL,
    "FileName" VARCHAR(500) NOT NULL,
    "FileUrl" VARCHAR(2000) NOT NULL,
    "FileSizeMB" DECIMAL(10,2) NOT NULL,
    "TotalMeshes" INTEGER NOT NULL DEFAULT 0,
    "AnalysisCompleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "AnalyzedAt" TIMESTAMP NULL,
    "AnalysisResultJson" JSONB NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" VARCHAR(255) NULL,
    "UpdatedBy" VARCHAR(255) NULL,
    
    CONSTRAINT "FK_Project3DModels_Projects_ProjectId" 
        FOREIGN KEY ("ProjectId") REFERENCES "Projects"("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_Project3DModels_ProjectId" ON "Project3DModels"("ProjectId");
CREATE INDEX "IX_Project3DModels_AnalysisCompleted" ON "Project3DModels"("AnalysisCompleted");

-- 2. BuildingElements table
CREATE TABLE "BuildingElements" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "ModelId" UUID NOT NULL,
    "Name" VARCHAR(200) NOT NULL,
    "ElementType" INTEGER NOT NULL,
    "Width" DECIMAL(10,3) NOT NULL,
    "Length" DECIMAL(10,3) NOT NULL,
    "Height" DECIMAL(10,3) NOT NULL,
    "CenterX" DECIMAL(10,3) NOT NULL,
    "CenterY" DECIMAL(10,3) NOT NULL,
    "CenterZ" DECIMAL(10,3) NOT NULL,
    "VolumeM3" DECIMAL(12,3) NOT NULL,
    "FloorLevel" INTEGER NOT NULL,
    "TrackingStatus" INTEGER NOT NULL DEFAULT 0,
    "CompletionPercentage" DECIMAL(5,2) NOT NULL DEFAULT 0,
    "CanTrack" BOOLEAN NOT NULL DEFAULT TRUE,
    "MeshIndices" JSONB NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" VARCHAR(255) NULL,
    "UpdatedBy" VARCHAR(255) NULL,
    
    CONSTRAINT "FK_BuildingElements_Project3DModels_ModelId" 
        FOREIGN KEY ("ModelId") REFERENCES "Project3DModels"("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_BuildingElements_ModelId" ON "BuildingElements"("ModelId");
CREATE INDEX "IX_BuildingElements_ElementType" ON "BuildingElements"("ElementType");
CREATE INDEX "IX_BuildingElements_TrackingStatus" ON "BuildingElements"("TrackingStatus");
CREATE INDEX "IX_BuildingElements_FloorLevel" ON "BuildingElements"("FloorLevel");
CREATE INDEX "IX_BuildingElements_ModelId_CompletionPercentage" 
    ON "BuildingElements"("ModelId", "CompletionPercentage");

-- 3. MeshGroups table
CREATE TABLE "MeshGroups" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "ModelId" UUID NOT NULL,
    "ComponentType" INTEGER NOT NULL,
    "MeshIndicesJson" JSONB NOT NULL DEFAULT '[]',
    "Color" VARCHAR(7) NOT NULL DEFAULT '#CCCCCC',
    "VolumeM3" DECIMAL(12,3) NOT NULL DEFAULT 0,
    "Unit" VARCHAR(10) NOT NULL DEFAULT 'm3',
    "IsAutoDetected" BOOLEAN NOT NULL DEFAULT TRUE,
    "DetectionAlgorithm" VARCHAR(100) NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" VARCHAR(255) NULL,
    "UpdatedBy" VARCHAR(255) NULL,
    
    CONSTRAINT "FK_MeshGroups_Project3DModels_ModelId" 
        FOREIGN KEY ("ModelId") REFERENCES "Project3DModels"("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_MeshGroups_ModelId" ON "MeshGroups"("ModelId");
CREATE INDEX "IX_MeshGroups_ComponentType" ON "MeshGroups"("ComponentType");

-- 4. ElementTrackingHistory table
CREATE TABLE "ElementTrackingHistory" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "BuildingElementId" UUID NOT NULL,
    "TrackingDate" TIMESTAMP NOT NULL,
    "PreviousPercentage" DECIMAL(5,2) NOT NULL DEFAULT 0,
    "NewPercentage" DECIMAL(5,2) NOT NULL,
    "Status" INTEGER NOT NULL,
    "PlannedQuantity" DECIMAL(10,2) NULL,
    "ActualQuantity" DECIMAL(10,2) NULL,
    "CementUsed" DECIMAL(10,2) NULL,
    "SandUsed" DECIMAL(10,2) NULL,
    "AggregateUsed" DECIMAL(10,2) NULL,
    "Notes" VARCHAR(2000) NULL,
    "RecordedById" UUID NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" VARCHAR(255) NULL,
    "UpdatedBy" VARCHAR(255) NULL,
    
    CONSTRAINT "FK_ElementTrackingHistory_BuildingElements_BuildingElementId" 
        FOREIGN KEY ("BuildingElementId") REFERENCES "BuildingElements"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ElementTrackingHistory_Users_RecordedById" 
        FOREIGN KEY ("RecordedById") REFERENCES "Users"("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_ElementTrackingHistory_BuildingElementId" ON "ElementTrackingHistory"("BuildingElementId");
CREATE INDEX "IX_ElementTrackingHistory_TrackingDate" ON "ElementTrackingHistory"("TrackingDate");
CREATE INDEX "IX_ElementTrackingHistory_RecordedById" ON "ElementTrackingHistory"("RecordedById");
CREATE INDEX "IX_ElementTrackingHistory_BuildingElementId_TrackingDate" 
    ON "ElementTrackingHistory"("BuildingElementId", "TrackingDate");
CREATE INDEX "IX_ElementTrackingHistory_BuildingElementId_Status" 
    ON "ElementTrackingHistory"("BuildingElementId", "Status");

-- 5. TrackingPhotos table
CREATE TABLE "TrackingPhotos" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "TrackingHistoryId" UUID NOT NULL,
    "PhotoUrl" VARCHAR(2000) NOT NULL,
    "Caption" VARCHAR(500) NULL,
    "FileSizeMB" DECIMAL(10,2) NOT NULL,
    "FileType" VARCHAR(50) NULL,
    "Width" INTEGER NULL,
    "Height" INTEGER NULL,
    "UploadedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" VARCHAR(255) NULL,
    "UpdatedBy" VARCHAR(255) NULL,
    
    CONSTRAINT "FK_TrackingPhotos_ElementTrackingHistory_TrackingHistoryId" 
        FOREIGN KEY ("TrackingHistoryId") REFERENCES "ElementTrackingHistory"("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_TrackingPhotos_TrackingHistoryId" ON "TrackingPhotos"("TrackingHistoryId");
CREATE INDEX "IX_TrackingPhotos_UploadedAt" ON "TrackingPhotos"("UploadedAt");

-- ========================================
-- COMMENTS FOR DOCUMENTATION
-- ========================================

COMMENT ON TABLE "Project3DModels" IS 'Stores 3D model files (GLB format) for projects';
COMMENT ON TABLE "BuildingElements" IS 'Individual building components parsed from 3D model';
COMMENT ON TABLE "MeshGroups" IS 'Groups of meshes categorized by component type';
COMMENT ON TABLE "ElementTrackingHistory" IS 'Daily tracking records for building elements';
COMMENT ON TABLE "TrackingPhotos" IS 'Photo evidence for tracking records';

COMMENT ON COLUMN "BuildingElements"."ElementType" IS '1=Wall, 2=Column, 3=Slab, 4=Beam, 5=Foundation, 6=Roof, 7=Stair, 8=Window, 9=Door, 10=Other';
COMMENT ON COLUMN "BuildingElements"."TrackingStatus" IS '0=NotStarted, 1=InProgress, 2=Completed, 3=OnHold, 4=Delayed';
COMMENT ON COLUMN "BuildingElements"."CompletionPercentage" IS 'Percentage of completion (0-100)';

-- ========================================
-- ROLLBACK SCRIPT (if needed)
-- ========================================

-- DROP TABLE IF EXISTS "TrackingPhotos" CASCADE;
-- DROP TABLE IF EXISTS "ElementTrackingHistory" CASCADE;
-- DROP TABLE IF EXISTS "MeshGroups" CASCADE;
-- DROP TABLE IF EXISTS "BuildingElements" CASCADE;
-- DROP TABLE IF EXISTS "Project3DModels" CASCADE;

