-- Fix audit columns types to match EF (string)
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name='SupervisorContracts' AND column_name='CreatedBy'
    ) THEN
        EXECUTE 'ALTER TABLE "SupervisorContracts" ALTER COLUMN "CreatedBy" TYPE text USING "CreatedBy"::text';
    END IF;

    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name='SupervisorContracts' AND column_name='UpdatedBy'
    ) THEN
        EXECUTE 'ALTER TABLE "SupervisorContracts" ALTER COLUMN "UpdatedBy" TYPE text USING "UpdatedBy"::text';
    END IF;
END $$;





