-- Normalize column names to match EF Core configuration (lowercase, unquoted)
-- Safe guards with IF EXISTS

DO $$
BEGIN
    -- HomeownerSignatureBase64 -> homeownersignaturebase64
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name='SupervisorContracts' AND column_name='HomeownerSignatureBase64'
    ) THEN
        EXECUTE 'ALTER TABLE "SupervisorContracts" RENAME COLUMN "HomeownerSignatureBase64" TO homeownersignaturebase64';
    END IF;

    -- SupervisorSignatureBase64 -> supervisorsignaturebase64
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name='SupervisorContracts' AND column_name='SupervisorSignatureBase64'
    ) THEN
        EXECUTE 'ALTER TABLE "SupervisorContracts" RENAME COLUMN "SupervisorSignatureBase64" TO supervisorsignaturebase64';
    END IF;

    -- TemplatePdfUrl -> templatepdfurl
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name='SupervisorContracts' AND column_name='TemplatePdfUrl'
    ) THEN
        EXECUTE 'ALTER TABLE "SupervisorContracts" RENAME COLUMN "TemplatePdfUrl" TO templatepdfurl';
    END IF;

    -- SignedPdfUrl -> signedpdfurl
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name='SupervisorContracts' AND column_name='SignedPdfUrl'
    ) THEN
        EXECUTE 'ALTER TABLE "SupervisorContracts" RENAME COLUMN "SignedPdfUrl" TO signedpdfurl';
    END IF;
END $$;





