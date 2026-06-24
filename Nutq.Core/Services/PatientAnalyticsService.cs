using System.Text.Json;
using System.Linq;
using System.Collections.Generic;
using Nutq.Core.Interfaces;
using Nutq.Core.Entities;

namespace Nutq.Core.Services
{
    public class PatientAnalyticsService : IPatientAnalyticsService
    {
        private readonly IPatientRepository _patientRepo;
        private readonly IDoctorPatientRelationshipRepository _relationshipRepo;
        private readonly ITrainingSessionRepository _sessionRepo;
        private readonly IProgressSnapshotRepository _snapshotRepo;
        private readonly ICategoryPerformanceSnapshotRepository _categoryRepo;
        private readonly ISessionClinicalReportRepository _reportRepo;
        private readonly ISpeechAttemptRepository _attemptRepo;
        private readonly IVocabularyRepository _vocabularyRepo;
        private readonly ITherapyPlanRepository _planRepo;
        private readonly IPlanExerciseRepository _planExerciseRepo;

        public PatientAnalyticsService(
            IPatientRepository patientRepo,
            IDoctorPatientRelationshipRepository relationshipRepo,
            ITrainingSessionRepository sessionRepo,
            IProgressSnapshotRepository snapshotRepo,
            ICategoryPerformanceSnapshotRepository categoryRepo,
            ISessionClinicalReportRepository reportRepo,
            ISpeechAttemptRepository attemptRepo,
            IVocabularyRepository vocabularyRepo,
            ITherapyPlanRepository planRepo,
            IPlanExerciseRepository planExerciseRepo)
        {
            _patientRepo = patientRepo;
            _relationshipRepo = relationshipRepo;
            _sessionRepo = sessionRepo;
            _snapshotRepo = snapshotRepo;
            _categoryRepo = categoryRepo;
            _reportRepo = reportRepo;
            _attemptRepo = attemptRepo;
            _vocabularyRepo = vocabularyRepo;
            _planRepo = planRepo;
            _planExerciseRepo = planExerciseRepo;
        }

        public async Task<bool> CanDoctorAccessPatientAsync(int doctorId, int patientId)
        {
            var patient = await _patientRepo.GetByIdAsync(patientId);
            if (patient == null) return false;
            if (patient.DoctorId == doctorId) return true;
            return await _relationshipRepo.HasRelationshipAsync(doctorId, patientId);
        }

        public async Task<PatientPerformanceSummary> GetSummaryAsync(int patientId)
        {
            var sessions = (await _sessionRepo.GetByPatientAsync(patientId)).ToList();
            var snapshots = (await _snapshotRepo.GetByPatientAsync(patientId)).ToList();
            var latestCategories = (await _categoryRepo.GetLatestByPatientAsync(patientId)).ToList();

            var totalTime = sessions.Sum(s => s.TotalDurationSeconds);
            var accuracy = snapshots.Count > 0 ? AnalyticsHelpers.Round(snapshots.Average(s => s.AccuracyPercent)) : 0;
            var firstAttempt = snapshots.Count > 0 ? AnalyticsHelpers.Round(snapshots.Average(s => s.FirstAttemptSuccessRate)) : 0;
            var avgSimilarity = snapshots.Count > 0 ? AnalyticsHelpers.Round(snapshots.Average(s => s.AverageSimilarity)) : 0;
            var avgAttempts = snapshots.Count > 0 ? AnalyticsHelpers.Round(snapshots.Average(s => s.AverageAttemptsPerWord)) : 0;

            var strongest = latestCategories
                .OrderByDescending(c => c.AccuracyPercent)
                .Take(3)
                .Select(c => c.Category);

            var weakest = latestCategories
                .OrderBy(c => c.AccuracyPercent)
                .Take(3)
                .Select(c => c.Category);

            var needsFocus = latestCategories
                .Where(c => c.TrendDirection == "Declining" || c.AccuracyPercent < 70)
                .OrderBy(c => c.AccuracyPercent)
                .Take(3)
                .Select(c => c.Category);

            return new PatientPerformanceSummary(
                sessions.Count,
                totalTime,
                accuracy,
                firstAttempt,
                avgSimilarity,
                avgAttempts,
                strongest,
                weakest,
                needsFocus);
        }

        public async Task<IEnumerable<TrainingSessionSummary>> GetSessionsAsync(int patientId, int? doctorId = null, DateTime? from = null, DateTime? to = null)
        {
            IEnumerable<TrainingSession> sessions;
            if (doctorId.HasValue)
            {
                var plans = await _planRepo.GetByDoctorAndPatientAsync(doctorId.Value, patientId);
                var planIds = plans.Select(p => p.Id).ToList();
                var planExercises = await _planExerciseRepo.GetByPlanIdsAsync(planIds);
                var planExerciseIds = planExercises.Select(pe => pe.Id).ToList();

                var list = planExerciseIds.Any()
                    ? (await _sessionRepo.GetByPlanExerciseIdsAsync(planExerciseIds)).ToList()
                    : new List<TrainingSession>();

                if (from.HasValue) list = list.Where(s => s.StartTime >= from.Value).ToList();
                if (to.HasValue) list = list.Where(s => s.EndTime <= to.Value).ToList();
                sessions = list.OrderByDescending(s => s.StartTime);
            }
            else
            {
                sessions = await _sessionRepo.GetByPatientAsync(patientId, from, to);
            }

            return sessions.Select(s => new TrainingSessionSummary(
                s.Id,
                s.ExerciseProgressId,
                s.Exercise?.Name ?? "Exercise",
                s.Exercise?.Category,
                s.StartTime,
                s.EndTime,
                s.TotalDurationSeconds,
                s.WordsCompleted,
                s.FirstAttemptCorrectCount,
                s.AverageSimilarityScore,
                s.AccuracyPercent));
        }

        public async Task<PatientProgressTrends> GetProgressAsync(int patientId, string period)
        {
            var snapshots = (await _snapshotRepo.GetByPatientAsync(patientId)).ToList();
            var grouped = AnalyticsHelpers.GroupByPeriod(snapshots, period, s => s.SnapshotDate);

            var dataPoints = grouped.Select(g => new ProgressDataPoint(
                g.Key,
                AnalyticsHelpers.Round(g.Average(s => s.AccuracyPercent)),
                AnalyticsHelpers.Round(g.Average(s => s.FirstAttemptSuccessRate)),
                AnalyticsHelpers.Round(g.Average(s => s.AverageSimilarity)),
                g.Max(s => s.CumulativeTrainingTimeSeconds))).ToList();

            var improvement = PatientAnalyticsEngine.CalculateOverallImprovement(
                dataPoints.Select(d => (d.Date, d.AccuracyPercent)));

            return new PatientProgressTrends(period, dataPoints, improvement);
        }

        public async Task<CategoryAnalysisResult> GetCategoryAnalysisAsync(int patientId)
        {
            var latest = (await _categoryRepo.GetLatestByPatientAsync(patientId)).ToList();
            var items = latest.Select(MapCategoryItem).ToList();

            return new CategoryAnalysisResult(
                items,
                items.OrderByDescending(i => i.AccuracyPercent).Take(3),
                items.OrderBy(i => i.AccuracyPercent).Take(3),
                items.Where(i => i.TrendDirection == "Improving"),
                items.Where(i => i.TrendDirection == "Declining" || i.AccuracyPercent < 70)
                    .OrderBy(i => i.AccuracyPercent));
        }

        public async Task<ChartDataResult> GetChartDataAsync(int patientId, DateTime? from = null, DateTime? to = null)
        {
            var snapshots = (await _snapshotRepo.GetByPatientAsync(patientId, from, to)).ToList();

            var accuracyOverTime = snapshots.Select(s => new ChartSeriesPoint(s.SnapshotDate, s.AccuracyPercent));
            var similarityOverTime = snapshots.Select(s => new ChartSeriesPoint(s.SnapshotDate, s.AverageSimilarity));
            var firstAttemptOverTime = snapshots.Select(s => new ChartSeriesPoint(s.SnapshotDate, s.FirstAttemptSuccessRate));

            var categoryHistory = (await _categoryRepo.GetByPatientAsync(patientId, from, to)).ToList();
            var categorySeries = categoryHistory
                .GroupBy(c => c.Category)
                .Select(g => new CategoryTrendSeries(
                    g.Key,
                    g.Select(c => new ChartSeriesPoint(c.ProgressSnapshot!.SnapshotDate, c.AccuracyPercent))))
                .ToList();

            var improvementTrend = snapshots
                .Select((s, index) =>
                {
                    var window = snapshots.Take(index + 1).TakeLast(Math.Min(5, index + 1));
                    return new ChartSeriesPoint(s.SnapshotDate, AnalyticsHelpers.Round(window.Average(w => w.AccuracyPercent)));
                })
                .ToList();

            return new ChartDataResult(
                accuracyOverTime,
                similarityOverTime,
                firstAttemptOverTime,
                categorySeries,
                improvementTrend);
        }

        public async Task<IEnumerable<ClinicalReportSummary>> GetReportsAsync(int patientId, int? doctorId = null, DateTime? from = null, DateTime? to = null)
        {
            IEnumerable<SessionClinicalReport> reports;
            if (doctorId.HasValue)
            {
                var plans = await _planRepo.GetByDoctorAndPatientAsync(doctorId.Value, patientId);
                var planIds = plans.Select(p => p.Id).ToList();
                var planExercises = await _planExerciseRepo.GetByPlanIdsAsync(planIds);
                var planExerciseIds = planExercises.Select(pe => pe.Id).ToList();

                var sessions = planExerciseIds.Any()
                    ? (await _sessionRepo.GetByPlanExerciseIdsAsync(planExerciseIds)).ToList()
                    : new List<TrainingSession>();

                if (from.HasValue) sessions = sessions.Where(s => s.StartTime >= from.Value).ToList();
                if (to.HasValue) sessions = sessions.Where(s => s.EndTime <= to.Value).ToList();

                var sessionIds = sessions.Select(s => s.Id).ToList();
                reports = sessionIds.Any()
                    ? await _reportRepo.GetByTrainingSessionIdsAsync(sessionIds)
                    : new List<SessionClinicalReport>();
            }
            else
            {
                reports = await _reportRepo.GetByPatientAsync(patientId, from, to);
            }

            return reports.Select(r => new ClinicalReportSummary(
                r.Id,
                r.TrainingSessionId,
                r.GeneratedAt,
                r.SessionDurationSeconds,
                r.ExercisesCompleted,
                r.AccuracyRate,
                r.FirstAttemptSuccessRate,
                r.AveragePronunciationSimilarity));
        }

        public async Task<ClinicalReportDetail?> GetReportAsync(int patientId, int sessionId, int? doctorId = null)
        {
            var session = await _sessionRepo.GetByIdWithDetailsAsync(sessionId);
            if (session == null || session.PatientId != patientId) return null;

            if (doctorId.HasValue)
            {
                var planEx = await _planExerciseRepo.GetByIdAsync(session.PlanExerciseId);
                if (planEx == null) return null;

                var plan = await _planRepo.GetByIdAsync(planEx.TherapyPlanId);
                if (plan == null || plan.DoctorId != doctorId.Value) return null;
            }

            var report = session.ClinicalReport
                ?? await _reportRepo.GetByTrainingSessionIdAsync(sessionId);

            if (report == null) return null;

            return new ClinicalReportDetail(
                report.Id,
                report.TrainingSessionId,
                report.GeneratedAt,
                report.SessionDurationSeconds,
                report.ExercisesCompleted,
                report.AccuracyRate,
                report.FirstAttemptSuccessRate,
                report.AveragePronunciationSimilarity,
                DeserializeStringList(report.StrengthAreasJson),
                DeserializeStringList(report.WeaknessAreasJson),
                DeserializeRecommendations(report.RecommendedFocusJson),
                report.AnalysisSource);
        }

        private static CategoryPerformanceItem MapCategoryItem(Core.Entities.CategoryPerformanceSnapshot c) =>
            new(c.Category, c.AccuracyPercent, c.AverageSimilarity, c.AverageAttemptsPerWord,
                c.WordsAttempted, c.TrendDirection, c.PreviousAccuracyPercent);

        private static IEnumerable<string> DeserializeStringList(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return Array.Empty<string>();
            try
            {
                return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        private static IEnumerable<RecommendedFocusItem> DeserializeRecommendations(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return Array.Empty<RecommendedFocusItem>();
            try
            {
                using var doc = JsonDocument.Parse(json);
                return doc.RootElement.EnumerateArray().Select(el => new RecommendedFocusItem(
                    el.TryGetProperty("category", out var cat) ? cat.GetString() ?? "" : "",
                    el.TryGetProperty("rationale", out var rat) ? rat.GetString() ?? "" : "",
                    el.TryGetProperty("priority", out var pri) ? pri.GetInt32() : 0));
            }
            catch
            {
                return Array.Empty<RecommendedFocusItem>();
            }
        }

        public async Task<PatientExerciseSessionAnalytics?> GetExerciseSessionAnalyticsAsync(int patientId, int planExerciseId)
        {
            var session = await _sessionRepo.GetByPatientAndPlanExerciseAsync(patientId, planExerciseId);
            if (session == null) return null;

            var attempts = (await _attemptRepo.GetByTrainingSessionAsync(session.Id)).ToList();
            var vocabCache = new Dictionary<int, (string English, string Arabic)>();

            async Task<(string English, string Arabic)> GetVocabLabels(int? vocabularyId, string expectedWord)
            {
                if (vocabularyId.HasValue && vocabCache.TryGetValue(vocabularyId.Value, out var cached))
                    return cached;

                if (vocabularyId.HasValue)
                {
                    var vocab = await _vocabularyRepo.GetByIdAsync(vocabularyId.Value);
                    if (vocab != null)
                    {
                        var labels = (vocab.WordEnglish, vocab.WordArabic);
                        vocabCache[vocabularyId.Value] = labels;
                        return labels;
                    }
                }

                return (expectedWord, expectedWord);
            }

            var wordGroups = attempts
                .GroupBy(a => $"{a.VocabularyId}|{a.ExpectedWord}")
                .ToList();

            var words = new List<WordSessionPerformance>();
            foreach (var group in wordGroups)
            {
                var ordered = group.OrderBy(a => a.AttemptNumber).ThenBy(a => a.AttemptedAt).ToList();
                var first = ordered.First();
                var labels = await GetVocabLabels(first.VocabularyId, first.ExpectedWord);
                var bestSimilarity = ordered.Max(a => a.SimilarityScore);
                var avgSimilarity = AnalyticsHelpers.Round(ordered.Average(a => a.SimilarityScore));
                var succeeded = ordered.Any(a => a.IsCorrect);

                words.Add(new WordSessionPerformance(
                    first.VocabularyId,
                    first.ExpectedWord,
                    labels.English,
                    labels.Arabic,
                    ordered.Count,
                    ordered.First().IsCorrect,
                    AnalyticsHelpers.Round(bestSimilarity),
                    avgSimilarity,
                    succeeded,
                    ordered.Select(a => new SpeechAttemptSummary(
                        a.AttemptNumber,
                        a.ExpectedWord,
                        a.RecognizedWord,
                        a.SimilarityScore,
                        a.IsCorrect,
                        a.AudioDurationSeconds,
                        a.AttemptedAt))));
            }

            var firstAttemptRate = words.Count > 0
                ? AnalyticsHelpers.Round((double)words.Count(w => w.FirstAttemptCorrect) / words.Count * 100)
                : 0;

            var report = session.ClinicalReport;
            IEnumerable<string> strengths = Array.Empty<string>();
            IEnumerable<string> weaknesses = Array.Empty<string>();
            if (report != null)
            {
                strengths = DeserializeStringList(report.StrengthAreasJson);
                weaknesses = DeserializeStringList(report.WeaknessAreasJson);
            }

            return new PatientExerciseSessionAnalytics(
                session.Id,
                session.ExerciseProgressId,
                session.Exercise?.Name ?? "Exercise",
                session.StartTime,
                session.EndTime,
                session.TotalDurationSeconds,
                session.WordsCompleted,
                session.FirstAttemptCorrectCount,
                session.AccuracyPercent,
                firstAttemptRate,
                session.AverageSimilarityScore,
                words.OrderByDescending(w => w.TotalAttempts).ThenBy(w => w.BestSimilarityScore),
                strengths,
                weaknesses);
        }
    }
}
