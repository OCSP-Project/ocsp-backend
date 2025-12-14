-- Migration: Add ContractorPosts and ContractorPostImages tables
-- Date: 2025-12-14
-- Description: Create tables for contractor posts feature

-- Create ContractorPosts table
CREATE TABLE IF NOT EXISTS "ContractorPosts" (
    "Id" uuid PRIMARY KEY,
    "ContractorId" uuid NOT NULL,
    "Title" text NOT NULL,
    "Description" text,
    "CreatedAt" timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" text,
    "UpdatedAt" timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy" text,
    CONSTRAINT "FK_ContractorPosts_Contractors"
        FOREIGN KEY ("ContractorId")
        REFERENCES "Contractors"("Id")
        ON DELETE CASCADE
);

-- Create ContractorPostImages table
CREATE TABLE IF NOT EXISTS "ContractorPostImages" (
    "Id" uuid PRIMARY KEY,
    "ContractorPostId" uuid NOT NULL,
    "Url" text NOT NULL,
    "Caption" text,
    "CreatedAt" timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" text,
    "UpdatedAt" timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy" text,
    CONSTRAINT "FK_ContractorPostImages_ContractorPosts"
        FOREIGN KEY ("ContractorPostId")
        REFERENCES "ContractorPosts"("Id")
        ON DELETE CASCADE
);

-- Add DEFAULT values to existing ContractorPosts table if it was created without them
ALTER TABLE "ContractorPosts"
    ALTER COLUMN "CreatedAt" SET DEFAULT CURRENT_TIMESTAMP,
    ALTER COLUMN "UpdatedAt" SET DEFAULT CURRENT_TIMESTAMP;

-- Add missing auditable columns to ContractorPostImages if they don't exist
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'ContractorPostImages' AND column_name = 'CreatedAt'
    ) THEN
        ALTER TABLE "ContractorPostImages"
            ADD COLUMN "CreatedAt" timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
            ADD COLUMN "CreatedBy" text,
            ADD COLUMN "UpdatedAt" timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
            ADD COLUMN "UpdatedBy" text;
    ELSE
        ALTER TABLE "ContractorPostImages"
            ALTER COLUMN "CreatedAt" SET DEFAULT CURRENT_TIMESTAMP,
            ALTER COLUMN "UpdatedAt" SET DEFAULT CURRENT_TIMESTAMP;
    END IF;
END $$;

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS "IX_ContractorPosts_ContractorId"
    ON "ContractorPosts"("ContractorId");

CREATE INDEX IF NOT EXISTS "IX_ContractorPosts_CreatedAt"
    ON "ContractorPosts"("CreatedAt" DESC);

CREATE INDEX IF NOT EXISTS "IX_ContractorPostImages_ContractorPostId"
    ON "ContractorPostImages"("ContractorPostId");

-- Success message
DO $$
BEGIN
    RAISE NOTICE 'Migration 20251214_AddContractorPostsTables completed successfully';
END $$;
