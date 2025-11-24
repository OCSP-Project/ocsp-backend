-- ========================================
-- Add DelegateApprovalToSupervisor column to Projects table
-- ========================================
-- This script adds the DelegateApprovalToSupervisor column to Projects table if it doesn't exist
-- This column allows homeowner to delegate material approval authority to Supervisor
-- Default value: false

DO $$
BEGIN
    -- Check if Projects table exists
    IF EXISTS (
        SELECT 1 FROM pg_catalog.pg_class c
        JOIN pg_catalog.pg_namespace n ON n.oid=c.relnamespace
        WHERE n.nspname='public' AND c.relname='Projects'
    ) THEN
        -- Check if DelegateApprovalToSupervisor column doesn't exist, then add it
        IF NOT EXISTS (
            SELECT 1 FROM information_schema.columns 
            WHERE table_schema='public' 
            AND table_name='Projects' 
            AND column_name='DelegateApprovalToSupervisor'
        ) THEN
            ALTER TABLE "Projects" 
            ADD COLUMN "DelegateApprovalToSupervisor" BOOLEAN NOT NULL DEFAULT false;
            
            RAISE NOTICE 'DelegateApprovalToSupervisor column added to Projects table';
        ELSE
            RAISE NOTICE 'DelegateApprovalToSupervisor column already exists in Projects table';
        END IF;
    ELSE
        RAISE NOTICE 'Projects table does not exist.';
    END IF;
END $$;

-- Add comment for documentation
COMMENT ON COLUMN "Projects"."DelegateApprovalToSupervisor" IS 'Homeowner delegates material approval authority to Supervisor (default: false)';


