-- Migration: Convert CreatedBy/UpdatedBy from TEXT to UUID
-- Date: 2025-12-18
-- Description: Change all AuditableEntity tables' CreatedBy/UpdatedBy columns from TEXT to UUID
-- IMPORTANT: Run on LOCAL first, test thoroughly, then run on EC2

-- =============================================================================
-- STEP 1: Create a helper function to check if a string is a valid UUID
-- =============================================================================
CREATE OR REPLACE FUNCTION is_valid_uuid(text_val TEXT)
RETURNS BOOLEAN AS $$
BEGIN
    -- Try to cast to UUID, return false if it fails
    PERFORM text_val::UUID;
    RETURN TRUE;
EXCEPTION WHEN OTHERS THEN
    RETURN FALSE;
END;
$$ LANGUAGE plpgsql;

-- =============================================================================
-- STEP 2: List of all tables with CreatedBy/UpdatedBy (AuditableEntity)
-- =============================================================================

-- BuildingElements
ALTER TABLE "BuildingElements"
    ALTER COLUMN "CreatedBy" TYPE UUID USING
        CASE
            WHEN is_valid_uuid("CreatedBy") THEN "CreatedBy"::UUID
            ELSE NULL
        END,
    ALTER COLUMN "UpdatedBy" TYPE UUID USING
        CASE
            WHEN is_valid_uuid("UpdatedBy") THEN "UpdatedBy"::UUID
            ELSE NULL
        END;

-- Project3DModels
ALTER TABLE "Project3DModels"
    ALTER COLUMN "CreatedBy" TYPE UUID USING
        CASE
            WHEN is_valid_uuid("CreatedBy") THEN "CreatedBy"::UUID
            ELSE NULL
        END,
    ALTER COLUMN "UpdatedBy" TYPE UUID USING
        CASE
            WHEN is_valid_uuid("UpdatedBy") THEN "UpdatedBy"::UUID
            ELSE NULL
        END;

-- Notifications
ALTER TABLE "Notifications"
    ALTER COLUMN "CreatedBy" TYPE UUID USING
        CASE
            WHEN is_valid_uuid("CreatedBy") THEN "CreatedBy"::UUID
            ELSE NULL
        END,
    ALTER COLUMN "UpdatedBy" TYPE UUID USING
        CASE
            WHEN is_valid_uuid("UpdatedBy") THEN "UpdatedBy"::UUID
            ELSE NULL
        END;

-- BudgetDetails
ALTER TABLE "BudgetDetails"
    ALTER COLUMN "CreatedBy" TYPE UUID USING
        CASE
            WHEN is_valid_uuid("CreatedBy") THEN "CreatedBy"::UUID
            ELSE NULL
        END,
    ALTER COLUMN "UpdatedBy" TYPE UUID USING
        CASE
            WHEN is_valid_uuid("UpdatedBy") THEN "UpdatedBy"::UUID
            ELSE NULL
        END;

-- Materials
ALTER TABLE "Materials"
    ALTER COLUMN "CreatedBy" TYPE UUID USING
        CASE
            WHEN is_valid_uuid("CreatedBy") THEN "CreatedBy"::UUID
            ELSE NULL
        END,
    ALTER COLUMN "UpdatedBy" TYPE UUID USING
        CASE
            WHEN is_valid_uuid("UpdatedBy") THEN "UpdatedBy"::UUID
            ELSE NULL
        END;

-- MaterialPayments
ALTER TABLE "MaterialPayments"
    ALTER COLUMN "CreatedBy" TYPE UUID USING
        CASE
            WHEN is_valid_uuid("CreatedBy") THEN "CreatedBy"::UUID
            ELSE NULL
        END,
    ALTER COLUMN "UpdatedBy" TYPE UUID USING
        CASE
            WHEN is_valid_uuid("UpdatedBy") THEN "UpdatedBy"::UUID
            ELSE NULL
        END;

-- PaymentRequests
ALTER TABLE "PaymentRequests"
    ALTER COLUMN "CreatedBy" TYPE UUID USING
        CASE
            WHEN is_valid_uuid("CreatedBy") THEN "CreatedBy"::UUID
            ELSE NULL
        END,
    ALTER COLUMN "UpdatedBy" TYPE UUID USING
        CASE
            WHEN is_valid_uuid("UpdatedBy") THEN "UpdatedBy"::UUID
            ELSE NULL
        END;

-- WorkItems
ALTER TABLE "WorkItems"
    ALTER COLUMN "CreatedBy" TYPE UUID USING
        CASE
            WHEN is_valid_uuid("CreatedBy") THEN "CreatedBy"::UUID
            ELSE NULL
        END,
    ALTER COLUMN "UpdatedBy" TYPE UUID USING
        CASE
            WHEN is_valid_uuid("UpdatedBy") THEN "UpdatedBy"::UUID
            ELSE NULL
        END;

-- ConstructionDiaries
ALTER TABLE "ConstructionDiaries"
    ALTER COLUMN "CreatedBy" TYPE UUID USING
        CASE
            WHEN is_valid_uuid("CreatedBy") THEN "CreatedBy"::UUID
            ELSE NULL
        END,
    ALTER COLUMN "UpdatedBy" TYPE UUID USING
        CASE
            WHEN is_valid_uuid("UpdatedBy") THEN "UpdatedBy"::UUID
            ELSE NULL
        END;

-- ProjectDailyResources
ALTER TABLE "ProjectDailyResources"
    ALTER COLUMN "CreatedBy" TYPE UUID USING
        CASE
            WHEN is_valid_uuid("CreatedBy") THEN "CreatedBy"::UUID
            ELSE NULL
        END,
    ALTER COLUMN "UpdatedBy" TYPE UUID USING
        CASE
            WHEN is_valid_uuid("UpdatedBy") THEN "UpdatedBy"::UUID
            ELSE NULL
        END;

-- Proposals
ALTER TABLE "Proposals"
    ALTER COLUMN "CreatedBy" TYPE UUID USING
        CASE
            WHEN is_valid_uuid("CreatedBy") THEN "CreatedBy"::UUID
            ELSE NULL
        END,
    ALTER COLUMN "UpdatedBy" TYPE UUID USING
        CASE
            WHEN is_valid_uuid("UpdatedBy") THEN "UpdatedBy"::UUID
            ELSE NULL
        END;

-- Contracts
ALTER TABLE "Contracts"
    ALTER COLUMN "CreatedBy" TYPE UUID USING
        CASE
            WHEN is_valid_uuid("CreatedBy") THEN "CreatedBy"::UUID
            ELSE NULL
        END,
    ALTER COLUMN "UpdatedBy" TYPE UUID USING
        CASE
            WHEN is_valid_uuid("UpdatedBy") THEN "UpdatedBy"::UUID
            ELSE NULL
        END;

-- ContractMilestones
ALTER TABLE "ContractMilestones"
    ALTER COLUMN "CreatedBy" TYPE UUID USING
        CASE
            WHEN is_valid_uuid("CreatedBy") THEN "CreatedBy"::UUID
            ELSE NULL
        END,
    ALTER COLUMN "UpdatedBy" TYPE UUID USING
        CASE
            WHEN is_valid_uuid("UpdatedBy") THEN "UpdatedBy"::UUID
            ELSE NULL
        END;

-- Escrows
ALTER TABLE "Escrows"
    ALTER COLUMN "CreatedBy" TYPE UUID USING
        CASE
            WHEN is_valid_uuid("CreatedBy") THEN "CreatedBy"::UUID
            ELSE NULL
        END,
    ALTER COLUMN "UpdatedBy" TYPE UUID USING
        CASE
            WHEN is_valid_uuid("UpdatedBy") THEN "UpdatedBy"::UUID
            ELSE NULL
        END;

-- Projects (cũng có CreatedBy/UpdatedBy)
ALTER TABLE "Projects"
    ALTER COLUMN "CreatedBy" TYPE UUID USING
        CASE
            WHEN is_valid_uuid("CreatedBy") THEN "CreatedBy"::UUID
            ELSE NULL
        END,
    ALTER COLUMN "UpdatedBy" TYPE UUID USING
        CASE
            WHEN is_valid_uuid("UpdatedBy") THEN "UpdatedBy"::UUID
            ELSE NULL
        END;

-- Add more tables if needed...
-- Check your schema for all tables inheriting from AuditableEntity

-- =============================================================================
-- STEP 3: Drop helper function
-- =============================================================================
DROP FUNCTION is_valid_uuid(TEXT);

-- =============================================================================
-- Migration completed!
-- =============================================================================
PRINT 'Migration completed successfully!';
PRINT 'All CreatedBy/UpdatedBy columns have been converted to UUID.';
PRINT 'Invalid values have been set to NULL.';
