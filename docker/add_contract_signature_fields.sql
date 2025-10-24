-- Add signature and PDF fields to Contracts table
ALTER TABLE "Contracts" 
ADD COLUMN IF NOT EXISTS "HomeownerSignatureBase64" TEXT,
ADD COLUMN IF NOT EXISTS "ContractorSignatureBase64" TEXT,
ADD COLUMN IF NOT EXISTS "TemplatePdfUrl" TEXT,
ADD COLUMN IF NOT EXISTS "SignedPdfUrl" TEXT;

-- Verify the columns were added
SELECT column_name, data_type 
FROM information_schema.columns 
WHERE table_name = 'Contracts' 
AND column_name IN ('HomeownerSignatureBase64', 'ContractorSignatureBase64', 'TemplatePdfUrl', 'SignedPdfUrl');