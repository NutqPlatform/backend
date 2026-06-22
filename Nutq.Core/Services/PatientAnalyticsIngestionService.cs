using System.Text.Json;
using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using Nutq.Core.Models;

namespace Nutq.Core.Services
{
    public class PatientAnalyticsIngestionService : IPatientAnalyticsIngestionService
    {
        private readonly ITrainingSessionRepository _sessionRepo;
        private readonly ISpeechAttemptRepository _attemptRepo;
        private readonly IProgressSnapshotRepository _snapshotRepo;
        private readonly ICategoryPerformanceSnapshotRepository _categoryRepo;
        private readonly ISessionClinicalReportRepository _reportRepo;
        private readonly IPronunciationPatternRepository _patternRepo;
        private readonly IVocabularyRepository _vocabularyRepo;
        private readonly ClinicalReportGenerator _reportGenerator = new();

        public PatientAnalyticsIngestionService(
            ITrainingSessionRepository sessionRepo,
            ISpeechAttemptRepository attemptRepo,
            IProgressSnapshotRepository snapshotRepo,
            ICategoryPerformanceSnapshotRepository categoryRepo,
            ISessionClinicalReportRepository reportRepo,
            IPronunciationPatternRepository patternRepo,
            IVocabularyRepository vocabularyRepo)
        {
            _sessionRepo = sessionRepo;
            _attemptRepo = attemptRepo;
            _snapshotRepo = snapshotRepo;
            _categoryRepo = categoryRepo;
            _reportRepo = reportRepo;
            _patternRepo = patternRepo;
            _vocabularyRepo = vocabularyRepo;
        }

        public async Task IngestCompletedSessionAsync(ExerciseProgress progress)
        {
            if (!progress.Completed) return;

            var existing = await _sessionRepo.GetByExerciseProgressIdAsync(progress.Id);
            if (existing != null) return;

            var endTime = progress.EndTime ?? DateTime.UtcNow;
            var exercise = progress.PlanExercise?.Exercise;
            var exerciseId = progress.PlanExercise?.ExerciseId ?? exercise?.Id ?? 0;
            if (exerciseId == 0) return;

            var sessionData = SessionDataParser.Parse(progress.SessionData);
            var defaultCategory = exercise?.Category;

            var parsedAttempts = SessionDataParser.ExtractAttempts(
                sessionData,
                progress.StartTime,
                endTime,
                defaultCategory);

            await EnrichCategoriesFromVocabularyAsync(parsedAttempts);

            var durationSeconds = sessionData?.TotalDurationSeconds > 0
                ? sessionData.TotalDurationSeconds
                : Math.Max(0, (int)(endTime - progress.StartTime).TotalSeconds);

            var metrics = PatientAnalyticsEngine.CalculateSessionMetrics(parsedAttempts);

            var trainingSession = await _sessionRepo.AddAsync(new TrainingSession
            {
                PatientId = progress.PatientId,
                ExerciseProgressId = progress.Id,
                ExerciseId = exerciseId,
                PlanExerciseId = progress.PlanExerciseId,
                StartTime = progress.StartTime,
                EndTime = endTime,
                TotalDurationSeconds = durationSeconds,
                WordsCompleted = metrics.WordsCompleted,
                FirstAttemptCorrectCount = metrics.FirstAttemptCorrectCount,
                AverageSimilarityScore = metrics.AverageSimilarityScore,
                AccuracyPercent = metrics.AccuracyPercent,
                CreatedAt = DateTime.UtcNow
            });

            if (parsedAttempts.Count > 0)
            {
                var speechAttempts = parsedAttempts.Select(a => new SpeechAttempt
                {
                    PatientId = progress.PatientId,
                    TrainingSessionId = trainingSession.Id,
                    ExerciseProgressId = progress.Id,
                    ExerciseId = exerciseId,
                    VocabularyId = a.VocabularyId,
                    ExpectedWord = a.ExpectedWord,
                    RecognizedWord = a.RecognizedWord,
                    SimilarityScore = a.SimilarityScore,
                    AttemptNumber = a.AttemptNumber,
                    IsCorrect = a.IsCorrect,
                    AudioDurationSeconds = a.AudioDurationSeconds,
                    Category = a.Category,
                    AttemptedAt = a.AttemptedAt
                }).ToList();

                await _attemptRepo.AddRangeAsync(speechAttempts);
                await UpdatePronunciationPatternsAsync(progress.PatientId, parsedAttempts);
            }

            var cumulativeTime = await _sessionRepo.GetTotalDurationSecondsAsync(progress.PatientId);

            var snapshot = await _snapshotRepo.AddAsync(new ProgressSnapshot
            {
                PatientId = progress.PatientId,
                TrainingSessionId = trainingSession.Id,
                SnapshotDate = endTime,
                CumulativeTrainingTimeSeconds = cumulativeTime,
                AccuracyPercent = metrics.AccuracyPercent,
                FirstAttemptSuccessRate = metrics.FirstAttemptSuccessRate,
                AverageSimilarity = metrics.AverageSimilarityScore,
                AverageAttemptsPerWord = metrics.AverageAttemptsPerWord,
                CreatedAt = DateTime.UtcNow
            });

            var categoryMetrics = PatientAnalyticsEngine.CalculateCategoryMetrics(parsedAttempts);
            var categorySnapshots = new List<CategoryPerformanceSnapshot>();

            foreach (var cat in categoryMetrics)
            {
                var previous = await _categoryRepo.GetPreviousForCategoryAsync(
                    progress.PatientId, cat.Category, endTime);

                categorySnapshots.Add(new CategoryPerformanceSnapshot
                {
                    ProgressSnapshotId = snapshot.Id,
                    PatientId = progress.PatientId,
                    Category = cat.Category,
                    AccuracyPercent = cat.AccuracyPercent,
                    AverageSimilarity = cat.AverageSimilarity,
                    AverageAttemptsPerWord = cat.AverageAttemptsPerWord,
                    WordsAttempted = cat.WordsAttempted,
                    TrendDirection = AnalyticsHelpers.DetermineTrendDirection(
                        cat.AccuracyPercent, previous?.AccuracyPercent),
                    PreviousAccuracyPercent = previous?.AccuracyPercent,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (categorySnapshots.Count > 0)
                await _categoryRepo.AddRangeAsync(categorySnapshots);

            var activePatterns = (await _patternRepo.GetActiveByPatientAsync(progress.PatientId)).ToList();
            var report = _reportGenerator.Generate(
                progress.PatientId,
                trainingSession,
                metrics,
                categoryMetrics,
                activePatterns);

            await _reportRepo.AddAsync(report);
        }

        private async Task EnrichCategoriesFromVocabularyAsync(List<ParsedSpeechAttempt> attempts)
        {
            var vocabIds = attempts
                .Where(a => a.VocabularyId.HasValue)
                .Select(a => a.VocabularyId!.Value)
                .Distinct()
                .ToList();

            if (vocabIds.Count == 0) return;

            var categories = new Dictionary<int, string>();
            foreach (var id in vocabIds)
            {
                var vocab = await _vocabularyRepo.GetByIdAsync(id);
                if (vocab?.Category != null)
                    categories[id] = AnalyticsHelpers.NormalizeCategory(vocab.Category);
            }

            for (var i = 0; i < attempts.Count; i++)
            {
                var attempt = attempts[i];
                if (attempt.VocabularyId.HasValue && categories.TryGetValue(attempt.VocabularyId.Value, out var cat))
                {
                    attempts[i] = attempt with { Category = cat };
                }
            }
        }

        private async Task UpdatePronunciationPatternsAsync(int patientId, IReadOnlyList<ParsedSpeechAttempt> attempts)
        {
            foreach (var attempt in attempts.Where(a => !a.IsCorrect))
            {
                var expected = attempt.ExpectedWord.Trim();
                var recognized = attempt.RecognizedWord.Trim();
                if (string.IsNullOrWhiteSpace(expected)) continue;

                var patternType = AnalyticsHelpers.ClassifyPatternType(expected, recognized, attempt.SimilarityScore);
                var existing = await _patternRepo.FindAsync(patientId, expected, recognized, patternType);

                if (existing != null)
                {
                    var newCount = existing.OccurrenceCount + 1;
                    existing.OccurrenceCount = newCount;
                    existing.AverageSimilarityScore = AnalyticsHelpers.Round(
                        (existing.AverageSimilarityScore * (newCount - 1) + attempt.SimilarityScore) / newCount);
                    existing.SeverityScore = AnalyticsHelpers.CalculateSeverityScore(
                        existing.AverageSimilarityScore, newCount);
                    existing.LastDetectedAt = attempt.AttemptedAt;
                    existing.Category = attempt.Category ?? existing.Category;
                    existing.VocabularyId = attempt.VocabularyId ?? existing.VocabularyId;
                    existing.IsActive = true;
                    await _patternRepo.UpdateAsync(existing);
                }
                else
                {
                    await _patternRepo.AddAsync(new PronunciationPattern
                    {
                        PatientId = patientId,
                        VocabularyId = attempt.VocabularyId,
                        ExpectedPattern = expected,
                        RecognizedPattern = recognized,
                        PatternType = patternType,
                        Category = attempt.Category,
                        OccurrenceCount = 1,
                        AverageSimilarityScore = attempt.SimilarityScore,
                        SeverityScore = AnalyticsHelpers.CalculateSeverityScore(attempt.SimilarityScore, 1),
                        AnalysisSource = AnalyticsHelpers.AnalysisSourceDeterministic,
                        MetadataJson = JsonSerializer.Serialize(new
                        {
                            source = "deterministic",
                            detectionVersion = "1.0",
                            aiReady = true
                        }),
                        IsActive = true,
                        FirstDetectedAt = attempt.AttemptedAt,
                        LastDetectedAt = attempt.AttemptedAt,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }
        }
    }
}
