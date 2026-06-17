-- ============================================================
-- Fix schema mismatches and add sample data for working UI
-- ============================================================

-- 1. Add missing Difficulty column to Exercises
ALTER TABLE public."Exercises"
ADD COLUMN IF NOT EXISTS "Difficulty" character varying(50);

-- 2. Update Exercises with Difficulty values
UPDATE public."Exercises" SET "Difficulty" = 'Easy' WHERE "Difficulty" IS NULL;

-- 3. Insert sample PlanExercises for TherapyPlan 10 (doctor 3, patient 3)
INSERT INTO public."PlanExercises" ("TherapyPlanId", "ExerciseId", "Repetition", "DurationMinutes")
SELECT 10, 1, 3, 15
WHERE NOT EXISTS (SELECT 1 FROM public."PlanExercises" WHERE "TherapyPlanId"=10 AND "ExerciseId"=1);

INSERT INTO public."PlanExercises" ("TherapyPlanId", "ExerciseId", "Repetition", "DurationMinutes")
SELECT 10, 2, 2, 10
WHERE NOT EXISTS (SELECT 1 FROM public."PlanExercises" WHERE "TherapyPlanId"=10 AND "ExerciseId"=2);

-- 4. Verify counts
SELECT 'Schema fix complete' AS status;
SELECT COUNT(*) as exercises FROM public."Exercises";
SELECT COUNT(*) as plan_exercises FROM public."PlanExercises";
