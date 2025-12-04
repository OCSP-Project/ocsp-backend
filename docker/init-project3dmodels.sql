-- ============================================
-- SQL Script to create Project3DModels table
-- Run this in pgAdmin or via psql
-- ============================================

-- Ensure pgcrypto extension for UUID generation
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Create Project3DModels table
CREATE TABLE IF NOT EXISTS "Project3DModels" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "ProjectId" UUID NOT NULL,
    "FileName" VARCHAR(500) NOT NULL,
    "FileUrl" TEXT NOT NULL,
    "FileSizeMB" DECIMAL(10,2) NOT NULL,
    "TotalMeshes" INTEGER NOT NULL DEFAULT 0,
    "AnalysisCompleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "AnalyzedAt" TIMESTAMP WITHOUT TIME ZONE NULL,
    "AnalysisResultJson" JSONB NULL,

    -- Audit fields
    "CreatedAt" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    "CreatedBy" UUID NULL,
    "UpdatedAt" TIMESTAMP WITHOUT TIME ZONE NULL,
    "UpdatedBy" UUID NULL
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS "IX_Project3DModels_ProjectId"
    ON "Project3DModels" ("ProjectId");

CREATE INDEX IF NOT EXISTS "IX_Project3DModels_AnalysisCompleted"
    ON "Project3DModels" ("AnalysisCompleted");

CREATE INDEX IF NOT EXISTS "IX_Project3DModels_CreatedAt"
    ON "Project3DModels" ("CreatedAt" DESC);

-- Create BuildingElements table (related to 3D models)
CREATE TABLE IF NOT EXISTS "BuildingElements" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Project3DModelId" UUID NOT NULL,
    "Name" VARCHAR(255) NOT NULL,
    "ElementType" VARCHAR(100) NOT NULL,
    "MaterialName" VARCHAR(255) NULL,
    "Color" VARCHAR(50) NULL,
    "BoundingBoxJson" JSONB NULL,
    "PropertiesJson" JSONB NULL,

    -- Audit fields
    "CreatedAt" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    "CreatedBy" UUID NULL,
    "UpdatedAt" TIMESTAMP WITHOUT TIME ZONE NULL,
    "UpdatedBy" UUID NULL,

    -- Foreign key
    CONSTRAINT "FK_BuildingElements_Project3DModels_Project3DModelId"
        FOREIGN KEY ("Project3DModelId")
        REFERENCES "Project3DModels"("Id")
        ON DELETE CASCADE
);

-- Create indexes for BuildingElements
CREATE INDEX IF NOT EXISTS "IX_BuildingElements_Project3DModelId"
    ON "BuildingElements" ("Project3DModelId");

CREATE INDEX IF NOT EXISTS "IX_BuildingElements_ElementType"
    ON "BuildingElements" ("ElementType");

-- Create MeshGroups table
CREATE TABLE IF NOT EXISTS "MeshGroups" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Project3DModelId" UUID NOT NULL,
    "GroupName" VARCHAR(255) NOT NULL,
    "MeshIndices" TEXT NOT NULL, -- Comma-separated mesh indices
    "Color" VARCHAR(50) NULL,
    "Opacity" DECIMAL(3,2) NULL,
    "IsVisible" BOOLEAN NOT NULL DEFAULT TRUE,

    -- Audit fields
    "CreatedAt" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    "CreatedBy" UUID NULL,
    "UpdatedAt" TIMESTAMP WITHOUT TIME ZONE NULL,
    "UpdatedBy" UUID NULL,

    -- Foreign key
    CONSTRAINT "FK_MeshGroups_Project3DModels_Project3DModelId"
        FOREIGN KEY ("Project3DModelId")
        REFERENCES "Project3DModels"("Id")
        ON DELETE CASCADE
);

-- Create indexes for MeshGroups
CREATE INDEX IF NOT EXISTS "IX_MeshGroups_Project3DModelId"
    ON "MeshGroups" ("Project3DModelId");

-- Create ElementTracking table
CREATE TABLE IF NOT EXISTS "ElementTracking" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "BuildingElementId" UUID NOT NULL,
    "Status" VARCHAR(50) NOT NULL,
    "Progress" INTEGER NOT NULL DEFAULT 0,
    "Notes" TEXT NULL,
    "TrackedAt" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),

    -- Audit fields
    "CreatedAt" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    "CreatedBy" UUID NULL,
    "UpdatedAt" TIMESTAMP WITHOUT TIME ZONE NULL,
    "UpdatedBy" UUID NULL,

    -- Foreign key
    CONSTRAINT "FK_ElementTracking_BuildingElements_BuildingElementId"
        FOREIGN KEY ("BuildingElementId")
        REFERENCES "BuildingElements"("Id")
        ON DELETE CASCADE
);

-- Create indexes for ElementTracking
CREATE INDEX IF NOT EXISTS "IX_ElementTracking_BuildingElementId"
    ON "ElementTracking" ("BuildingElementId");

CREATE INDEX IF NOT EXISTS "IX_ElementTracking_TrackedAt"
    ON "ElementTracking" ("TrackedAt" DESC);

-- Verify tables were created
SELECT
    tablename,
    schemaname
FROM pg_tables
WHERE schemaname = 'public'
    AND tablename IN ('Project3DModels', 'BuildingElements', 'MeshGroups', 'ElementTracking')
ORDER BY tablename;

-- Show table info
\d "Project3DModels"
