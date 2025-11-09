-- Create SupervisorContracts table and indexes

CREATE TABLE IF NOT EXISTS "SupervisorContracts" (
  "Id" uuid NOT NULL PRIMARY KEY,
  "ProjectId" uuid NOT NULL,
  "SupervisorId" uuid NOT NULL,
  "HomeownerUserId" uuid NOT NULL,
  "SupervisorUserId" uuid NOT NULL,

  "MonthlyPrice" numeric(18,2) NOT NULL,
  "Terms" text NOT NULL DEFAULT '',
  "Status" integer NOT NULL DEFAULT 0,

  "SignedByHomeownerAt" timestamptz NULL,
  "SignedBySupervisorAt" timestamptz NULL,

  "HomeownerSignatureBase64" text NULL,
  "SupervisorSignatureBase64" text NULL,

  "TemplatePdfUrl" varchar(1000) NULL,
  "SignedPdfUrl" varchar(1000) NULL,

  "CreatedAt" timestamptz NOT NULL DEFAULT now(),
  "UpdatedAt" timestamptz NOT NULL DEFAULT now(),
  "CreatedBy" uuid NULL,
  "UpdatedBy" uuid NULL,

  CONSTRAINT "FK_SupervisorContracts_Projects_ProjectId"
    FOREIGN KEY ("ProjectId") REFERENCES "Projects"("Id") ON DELETE RESTRICT,
  CONSTRAINT "FK_SupervisorContracts_Supervisors_SupervisorId"
    FOREIGN KEY ("SupervisorId") REFERENCES "Supervisors"("Id") ON DELETE RESTRICT,
  CONSTRAINT "FK_SupervisorContracts_Users_HomeownerUserId"
    FOREIGN KEY ("HomeownerUserId") REFERENCES "Users"("Id") ON DELETE RESTRICT,
  CONSTRAINT "FK_SupervisorContracts_Users_SupervisorUserId"
    FOREIGN KEY ("SupervisorUserId") REFERENCES "Users"("Id") ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS "IX_SupervisorContracts_ProjectId" ON "SupervisorContracts" ("ProjectId");
CREATE INDEX IF NOT EXISTS "IX_SupervisorContracts_SupervisorId" ON "SupervisorContracts" ("SupervisorId");
CREATE INDEX IF NOT EXISTS "IX_SupervisorContracts_Status" ON "SupervisorContracts" ("Status");
CREATE INDEX IF NOT EXISTS "IX_SupervisorContracts_HomeownerUserId" ON "SupervisorContracts" ("HomeownerUserId");
CREATE INDEX IF NOT EXISTS "IX_SupervisorContracts_SupervisorUserId" ON "SupervisorContracts" ("SupervisorUserId");





