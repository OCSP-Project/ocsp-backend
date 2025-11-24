-- ========================================
-- Add CompletedProjects column to RegistrationRequests table
-- ========================================
-- This script adds the CompletedProjects column to RegistrationRequests table if it doesn't exist
-- This column stores the number of completed projects for contractors
-- Default value: NULL (nullable)

DO $$
BEGIN
    -- Check if RegistrationRequests table exists
    IF EXISTS (
        SELECT 1 FROM pg_catalog.pg_class c
        JOIN pg_catalog.pg_namespace n ON n.oid=c.relnamespace
        WHERE n.nspname='public' AND c.relname='RegistrationRequests'
    ) THEN
        -- Check if CompletedProjects column doesn't exist, then add it
        IF NOT EXISTS (
            SELECT 1 FROM information_schema.columns 
            WHERE table_schema='public' 
            AND table_name='RegistrationRequests' 
            AND column_name='CompletedProjects'
        ) THEN
            ALTER TABLE "RegistrationRequests" 
            ADD COLUMN "CompletedProjects" INTEGER NULL;
            
            RAISE NOTICE 'CompletedProjects column added to RegistrationRequests table';
        ELSE
            RAISE NOTICE 'CompletedProjects column already exists in RegistrationRequests table';
        END IF;
    ELSE
        RAISE NOTICE 'RegistrationRequests table does not exist. Please run 006_create_registration_requests.sql first.';
    END IF;
END $$;

-- Add comment for documentation
COMMENT ON COLUMN "RegistrationRequests"."CompletedProjects" IS 'Number of completed projects (for contractors)';

