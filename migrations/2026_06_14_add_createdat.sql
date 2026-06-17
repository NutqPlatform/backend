-- Add CreatedAt columns to Patients and Doctors without deleting existing data
-- Safe: uses IF NOT EXISTS and fills existing NULLs with current UTC time

BEGIN;

ALTER TABLE IF EXISTS "Patients" ADD COLUMN IF NOT EXISTS "CreatedAt" TIMESTAMP WITH TIME ZONE DEFAULT now();
UPDATE "Patients" SET "CreatedAt" = now() WHERE "CreatedAt" IS NULL;
ALTER TABLE "Patients" ALTER COLUMN "CreatedAt" SET NOT NULL;

ALTER TABLE IF EXISTS "Doctors" ADD COLUMN IF NOT EXISTS "CreatedAt" TIMESTAMP WITH TIME ZONE DEFAULT now();
UPDATE "Doctors" SET "CreatedAt" = now() WHERE "CreatedAt" IS NULL;
ALTER TABLE "Doctors" ALTER COLUMN "CreatedAt" SET NOT NULL;

COMMIT;
