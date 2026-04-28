-- First, ensure DifficultyLevels exist
INSERT INTO "DifficultyLevels" ("Name", "Level") VALUES 
    ('Easy', 'Easy'),
    ('Medium', 'Medium'),
    ('Hard', 'Hard')
ON CONFLICT DO NOTHING;

-- Add sample vocabulary for the "Pronounce one word" exercise (Articulation category)
INSERT INTO "Vocabularies" ("WordArabic", "WordEnglish", "DifficultyLevelId", "Category", "ImageUrl", "SoundUrl", "VideoUrl", "CreatedAt")
SELECT 
    'كتاب',
    'Book',
    dl."Id",
    'Articulation',
    '/images/book.jpg',
    '/sounds/book.mp3',
    '/videos/book.mp4',
    NOW()
FROM "DifficultyLevels" dl WHERE dl."Name" = 'Easy'

UNION ALL

SELECT 
    'قلم',
    'Pen',
    dl."Id",
    'Articulation',
    '/images/pen.jpg',
    '/sounds/pen.mp3',
    '/videos/pen.mp4',
    NOW()
FROM "DifficultyLevels" dl WHERE dl."Name" = 'Easy'

UNION ALL

SELECT 
    'طاولة',
    'Table',
    dl."Id",
    'Articulation',
    '/images/table.jpg',
    '/sounds/table.mp3',
    '/videos/table.mp4',
    NOW()
FROM "DifficultyLevels" dl WHERE dl."Name" = 'Easy'

UNION ALL

SELECT 
    'كرسي',
    'Chair',
    dl."Id",
    'Articulation',
    '/images/chair.jpg',
    '/sounds/chair.mp3',
    '/videos/chair.mp4',
    NOW()
FROM "DifficultyLevels" dl WHERE dl."Name" = 'Easy'

UNION ALL

SELECT 
    'باب',
    'Door',
    dl."Id",
    'Articulation',
    '/images/door.jpg',
    '/sounds/door.mp3',
    '/videos/door.mp4',
    NOW()
FROM "DifficultyLevels" dl WHERE dl."Name" = 'Easy'

ON CONFLICT DO NOTHING;
