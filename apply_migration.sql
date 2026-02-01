-- SQL Script to Add Repetition Tracking Columns
-- Execute this script in PostgreSQL for NutqDB database

-- Step 1: Connect to the database
-- psql -U postgres -d NutqDB

-- Step 2: Add the new columns
ALTER TABLE "ExerciseProgresses"
ADD COLUMN IF NOT EXISTS "CurrentRepetition" integer NOT NULL DEFAULT 1;

ALTER TABLE "ExerciseProgresses"
ADD COLUMN IF NOT EXISTS "TotalRepetitions" integer NOT NULL DEFAULT 1;

-- Step 3: Verify the columns were added
SELECT column_name, data_type, column_default, is_nullable
FROM information_schema.columns
WHERE table_name = 'ExerciseProgresses'
ORDER BY ordinal_position;

-- Expected output should include:
-- CurrentRepetition  | integer | 1 | NO
-- TotalRepetitions   | integer | 1 | NO

-- Step 4: Check existing ExerciseProgress records
SELECT "Id", "PatientId", "PlanExerciseId", "CurrentRepetition", "TotalRepetitions", "Completed"
FROM "ExerciseProgresses"
LIMIT 10;

-- All existing records should have CurrentRepetition=1 and TotalRepetitions=1 (defaults)

-- Step 5: Verify the table structure
\d "ExerciseProgresses"

-- Expected to show both new columns in the table definition
