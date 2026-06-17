-- ============================================================
-- Seed script: basics from old DB into new DB
-- Run this after applying your migrations on the new database
-- ============================================================

-- ── 1. Difficulty Level needed by both exercises (Easy / Level 1) ──────────

INSERT INTO public."DifficultyLevels" ("Id", "Level", "Name", "Description")
VALUES (1, 'Easy', 'Level 1', 'Basic exercises for beginners')
ON CONFLICT ("Id") DO NOTHING;


-- ── 2. Admin ───────────────────────────────────────────────────────────────
-- Note: password is stored plain-text in the old DB ("123456").
-- Replace with a proper hash before going to production.

INSERT INTO public."Admins" ("Name", "Email", "Password", "IsActive")
VALUES ('Admin', 'admin@example.com', '123456', true)
ON CONFLICT ("Email") DO NOTHING;


-- ── 3. Exercises ───────────────────────────────────────────────────────────
-- Exercise 1 (Id=1): Articulation / pronounce-one-word   → uses Fruits vocab
-- Exercise 2 (Id=2): Tools Sound Match                   → uses Tools vocab

INSERT INTO public."Exercises" ("Id", "DifficultyId", "Name", "Description", "Category", "Tags", "AssetUrl", "ImageUrl")
VALUES
    (1, 1, 'Pronounce one word',
     'Listen to the word and try to pronounce it',
     'Articulation', 'pronunciation,single-word',
     NULL, 'assets/images/fruits/Fruits.png'),

    (2, 1, 'Tools Sound Match',
     'Listen to the sound and choose the correct tool card.',
     'tools', NULL,
     NULL, '/sounds/images/New folder/tools.png')
ON CONFLICT ("Id") DO NOTHING;


-- ── 4. Vocabulary ──────────────────────────────────────────────────────────
-- Fruits (used by Exercise 1 / Articulation)

INSERT INTO public."Vocabularies"
    ("Id", "WordArabic", "WordEnglish", "DifficultyLevelId",
     "Category", "ImageUrl", "SoundUrl", "VideoUrl",
     "ImageDescriptions", "Tags", "CreatedAt")
VALUES
    ( 9, 'تفاح',    'Apple',        1, 'Fruits', '/assets/images/fruits/apple.png',       '/assets/sounds/fruits/apple.mp3',       NULL, NULL, 'fruit', NOW()),
    (10, 'موز',     'Banana',       1, 'Fruits', '/assets/images/fruits/banana.png',      '/assets/sounds/fruits/banana.mp3',      NULL, NULL, 'fruit', NOW()),
    (11, 'برتقال',  'Orange',       1, 'Fruits', '/assets/images/fruits/orange.png',      '/assets/sounds/fruits/orange.mp3',      NULL, NULL, 'fruit', NOW()),
    (12, 'فراولة',  'Strawberry',   1, 'Fruits', '/assets/images/fruits/strawberry.png',  '/assets/sounds/fruits/strawberry.mp3',  NULL, NULL, 'fruit', NOW()),
    (13, 'عنب',     'Grapes',       1, 'Fruits', '/assets/images/fruits/grapes.png',      '/assets/sounds/fruits/grapes.mp3',      NULL, NULL, 'fruit', NOW()),
    (14, 'كيوي',    'Kiwi',         1, 'Fruits', '/assets/images/fruits/kiwi.png',        '/assets/sounds/fruits/kiwi.mp3',        NULL, NULL, 'fruit', NOW()),
    (15, 'بطيخ',    'Watermelon',   1, 'Fruits', '/assets/images/fruits/watermelon.png',  '/assets/sounds/fruits/watermelon.mp3',  NULL, NULL, 'fruit', NOW()),
    (16, 'رمان',    'Pomegranate',  1, 'Fruits', '/assets/images/fruits/pomegranate.png', '/assets/sounds/fruits/pomegranate.mp3', NULL, NULL, 'fruit', NOW())
ON CONFLICT ("Id") DO NOTHING;

-- Tools (used by Exercise 2 / Tools Sound Match)

INSERT INTO public."Vocabularies"
    ("Id", "WordArabic", "WordEnglish", "DifficultyLevelId",
     "Category", "ImageUrl", "SoundUrl", "VideoUrl",
     "ImageDescriptions", "Tags", "CreatedAt")
VALUES
    (17, 'موز',           'Banana',     1, 'tools', '/assets/images/fruits/banana.png',      '/assets/sounds/fruits/banana.mp3',           NULL, NULL, NULL, NOW()),
    (18, 'تفاح',          'Apple',      1, 'tools', '/assets/images/fruits/apple.png',        '/assets/sounds/fruits/apple.mp3',            NULL, NULL, NULL, NOW()),
    (19, 'عنب',           'Grapes',     1, 'furniture', '/assets/images/fruits/grapes.png',   '/assets/sounds/fruits/grapes.mp3',           NULL, NULL, NULL, NOW()),
    (20, 'فرشاة اسنان',   'Toothbrush', 1, 'tools', '/assets/images/game2/toothbrush.png',    '/assets/images/game2/toothbrush.mp3',        NULL, NULL, NULL, NOW())
ON CONFLICT ("Id") DO NOTHING;


-- ── 5. VocabularyExercises links ───────────────────────────────────────────
-- Exercise 1 ↔ Fruits vocab

INSERT INTO public."VocabularyExercises" ("VocabularyId", "ExerciseId", "DifficultyLevelId")
VALUES
    ( 9, 1, 1),
    (10, 1, 1),
    (11, 1, 1),
    (12, 1, 1),
    (13, 1, 1),
    (14, 1, 1),
    (15, 1, 1),
    (16, 1, 1)
ON CONFLICT ("VocabularyId", "ExerciseId", "DifficultyLevelId") DO NOTHING;

-- Exercise 2 ↔ Tools vocab

INSERT INTO public."VocabularyExercises" ("VocabularyId", "ExerciseId", "DifficultyLevelId")
VALUES
    (17, 2, 1),
    (18, 2, 1),
    (19, 2, 1),
    (20, 2, 1)
ON CONFLICT ("VocabularyId", "ExerciseId", "DifficultyLevelId") DO NOTHING;


-- ── 6. Reset sequences so new inserts don't collide ───────────────────────

SELECT setval('public."DifficultyLevels_Id_seq"', (SELECT MAX("Id") FROM public."DifficultyLevels"));
SELECT setval('public."Admins_Id_seq"',           (SELECT MAX("Id") FROM public."Admins"));
SELECT setval('public."Exercises_Id_seq"',        (SELECT MAX("Id") FROM public."Exercises"));
SELECT setval('public."Vocabularies_Id_seq"',     (SELECT MAX("Id") FROM public."Vocabularies"));
SELECT setval('public."VocabularyExercises_Id_seq"', (SELECT MAX("Id") FROM public."VocabularyExercises"));
