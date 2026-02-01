-- Add repetition tracking columns to ExerciseProgresses table
ALTER TABLE "ExerciseProgresses"
ADD COLUMN IF NOT EXISTS "CurrentRepetition" integer NOT NULL DEFAULT 1,
ADD COLUMN IF NOT EXISTS "TotalRepetitions" integer NOT NULL DEFAULT 1;

-- Add constraints
ALTER TABLE "ExerciseProgresses"
ADD CONSTRAINT "CK_CurrentRepetition" CHECK ("CurrentRepetition" >= 1 AND "CurrentRepetition" <= 100),
ADD CONSTRAINT "CK_TotalRepetitions" CHECK ("TotalRepetitions" >= 1 AND "TotalRepetitions" <= 100);
