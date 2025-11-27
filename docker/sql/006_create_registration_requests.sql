-- ========================================
-- Create RegistrationRequests table
-- ========================================
-- This script creates the RegistrationRequests table if it doesn't exist
-- Used for managing user registration requests (Supervisor or Contractor)

CREATE TABLE IF NOT EXISTS "RegistrationRequests" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    
    -- User information
    "Username" VARCHAR(50) NOT NULL,
    "Email" VARCHAR(255) NOT NULL,
    "Phone" VARCHAR(20) NOT NULL,
    "RequestedRole" INTEGER NOT NULL, -- 1=Supervisor, 2=Contractor
    
    -- Status
    "Status" INTEGER NOT NULL DEFAULT 0, -- 0=Pending, 1=Approved, 2=Rejected
    "RejectionReason" VARCHAR(1000) NULL,
    "ReviewedAt" TIMESTAMP WITH TIME ZONE NULL,
    "ReviewedByUserId" UUID NULL,
    
    -- Supervisor specific fields
    "Department" VARCHAR(200) NULL,
    "Position" VARCHAR(200) NULL,
    "District" VARCHAR(100) NULL,
    "MinRate" NUMERIC(18,2) NULL,
    "MaxRate" NUMERIC(18,2) NULL,
    
    -- Contractor specific fields
    "CompanyName" VARCHAR(200) NULL,
    "BusinessLicense" VARCHAR(50) NULL,
    "TaxCode" VARCHAR(50) NULL,
    "Description" VARCHAR(2000) NULL,
    "Website" VARCHAR(500) NULL,
    "Address" VARCHAR(500) NULL,
    "City" VARCHAR(100) NULL,
    "Province" VARCHAR(100) NULL,
    "YearsOfExperience" INTEGER NULL,
    "TeamSize" INTEGER NULL,
    "CompletedProjects" INTEGER NULL,
    "MinProjectBudget" NUMERIC(18,2) NULL,
    "MaxProjectBudget" NUMERIC(18,2) NULL,
    
    -- Created user (after approval)
    "CreatedUserId" UUID NULL,
    
    -- Audit fields (from AuditableEntity)
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" TEXT NULL,
    "UpdatedBy" TEXT NULL,
    
    -- Foreign keys
    CONSTRAINT "FK_RegistrationRequests_Users_ReviewedByUserId"
        FOREIGN KEY ("ReviewedByUserId") REFERENCES "Users"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_RegistrationRequests_Users_CreatedUserId"
        FOREIGN KEY ("CreatedUserId") REFERENCES "Users"("Id") ON DELETE SET NULL
);

-- Create indexes
CREATE INDEX IF NOT EXISTS "IX_RegistrationRequests_Email" ON "RegistrationRequests"("Email");
CREATE INDEX IF NOT EXISTS "IX_RegistrationRequests_Status" ON "RegistrationRequests"("Status");
CREATE INDEX IF NOT EXISTS "IX_RegistrationRequests_RequestedRole" ON "RegistrationRequests"("RequestedRole");
CREATE INDEX IF NOT EXISTS "IX_RegistrationRequests_CreatedAt" ON "RegistrationRequests"("CreatedAt");
CREATE INDEX IF NOT EXISTS "IX_RegistrationRequests_ReviewedByUserId" ON "RegistrationRequests"("ReviewedByUserId");
CREATE INDEX IF NOT EXISTS "IX_RegistrationRequests_CreatedUserId" ON "RegistrationRequests"("CreatedUserId");

-- Add comments for documentation
COMMENT ON TABLE "RegistrationRequests" IS 'Stores user registration requests for Supervisor or Contractor roles';
COMMENT ON COLUMN "RegistrationRequests"."RequestedRole" IS '1=Supervisor, 2=Contractor';
COMMENT ON COLUMN "RegistrationRequests"."Status" IS '0=Pending, 1=Approved, 2=Rejected';
COMMENT ON COLUMN "RegistrationRequests"."CompletedProjects" IS 'Number of completed projects (for contractors)';

