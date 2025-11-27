-- ========================================
-- Add Color column to BuildingElements table
-- ========================================
-- This script adds the Color column to BuildingElements table if it doesn't exist
-- Color is used for 3D visualization (hex color format: #RRGGBB)
-- Default value: #CCCCCC (light gray)

DO $$
BEGIN
    -- Check if BuildingElements table exists
    IF EXISTS (
        SELECT 1 FROM pg_catalog.pg_class c
        JOIN pg_catalog.pg_namespace n ON n.oid=c.relnamespace
        WHERE n.nspname='public' AND c.relname='BuildingElements'
    ) THEN
        -- Check if Color column doesn't exist, then add it
        IF NOT EXISTS (
            SELECT 1 FROM information_schema.columns 
            WHERE table_schema='public' 
            AND table_name='BuildingElements' 
            AND column_name='Color'
        ) THEN
            ALTER TABLE "BuildingElements" 
            ADD COLUMN "Color" VARCHAR(7) NOT NULL DEFAULT '#CCCCCC';
            
            -- Add comment for documentation
            COMMENT ON COLUMN "BuildingElements"."Color" IS 'Hex color code for 3D visualization (format: #RRGGBB, default: #CCCCCC)';
            
            RAISE NOTICE 'Color column added to BuildingElements table';
        ELSE
            RAISE NOTICE 'Color column already exists in BuildingElements table';
        END IF;
    ELSE
        RAISE NOTICE 'BuildingElements table does not exist. Please run migration-3d-model-tracking.sql first.';
    END IF;
END $$;

