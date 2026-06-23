using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using Nutq.Core.Models;
using Nutq.Core.Services;

namespace Nutq.Core.Services
{
    public class DoctorAnalyticsService : IDoctorAnalyticsService
    {
        private readonly IDoctorRepository _doctorRepo;
        private readonly ITherapyPlanRepository _planRepo;
        private readonly IPlanExerciseRepository _planExerciseRepo;
        private readonly IExerciseProgressRepository _progressRepo;
        private readonly IDoctorPatientRelationshipRepository _relationshipRepo;
        private readonly IPatientAnalyticsService _patientAnalyticsService;
        private readonly ITrainingSessionRepository _sessionRepo;
        private readonly ICategoryPerformanceSnapshotRepository _categoryRepo;
        private readonly ISpeechAttemptRepository _attemptRepo;
        private readonly ISessionClinicalReportRepository _reportRepo;
        private readonly IProgressSnapshotRepository _snapshotRepo;

        public DoctorAnalyticsService(
            IDoctorRepository doctorRepo,
            ITherapyPlanRepository planRepo,
            IPlanExerciseRepository planExerciseRepo,
            IExerciseProgressRepository progressRepo,
            IDoctorPatientRelationshipRepository relationshipRepo,
            IPatientAnalyticsService patientAnalyticsService,
            ITrainingSessionRepository sessionRepo,
            ICategoryPerformanceSnapshotRepository categoryRepo,
            ISpeechAttemptRepository attemptRepo,
            ISessionClinicalReportRepository reportRepo,
            IProgressSnapshotRepository snapshotRepo)
        {
            _doctorRepo = doctorRepo;
            _planRepo = planRepo;
            _planExerciseRepo = planExerciseRepo;
            _progressRepo = progressRepo;
            _relationshipRepo = relationshipRepo;
            _patientAnalyticsService = patientAnalyticsService;
            _sessionRepo = sessionRepo;
            _categoryRepo = categoryRepo;
            _attemptRepo = attemptRepo;
            _reportRepo = reportRepo;
            _snapshotRepo = snapshotRepo;
        }

        public async Task<int> GetTotalPatientsAsync(int doctorId)
        {
            var active = await _relationshipRepo.GetActiveByDoctorIdAsync(doctorId);
            return active.Select(r => r.PatientId).Distinct().Count();
        }

        public async Task<int> GetTotalPlansAsync(int doctorId)
        {
            var plans = await _planRepo.GetOngoingPlansByDoctorAsync(doctorId);
            return plans.Count;
        }

        public async Task<int> GetTotalExercisesAsync(int doctorId)
        {
            var plans = await _planRepo.GetOngoingPlansByDoctorAsync(doctorId);
            var planIds = plans.Select(p => p.Id).ToList();
            var exercises = await _planExerciseRepo.GetByPlanIdsAsync(planIds) ?? new List<PlanExercise>();
            return exercises.Count;
        }

        public async Task<double> GetAverageCompletionRateAsync(int doctorId)
        {
            var plans = await _planRepo.GetOngoingPlansByDoctorAsync(doctorId);
            if (!plans.Any())
                return 0;

            var planIds = plans.Select(p => p.Id).ToList();
            var planExercises = await _planExerciseRepo.GetByPlanIdsAsync(planIds) ?? new List<PlanExercise>();
            if (!planExercises.Any())
                return 0;

            var progresses = await _progressRepo.GetByPlanExerciseIdsAsync(planExercises.Select(pe => pe.Id).ToList()) ?? new List<ExerciseProgress>();

            int totalExercises = planExercises.Count;
            int completedExercises = progresses
                .Where(p => p.Completed)
                .Select(p => p.PlanExerciseId)
                .Distinct()
                .Count();

            return (double)completedExercises / totalExercises * 100;
        }

        public async Task<PatientLongitudinalAnalytics?> GetPatientLongitudinalAnalyticsAsync(int doctorId, int patientId)
        {
            if (!await _patientAnalyticsService.CanDoctorAccessPatientAsync(doctorId, patientId))
                return null;

            var sessionProjections = await _sessionRepo.GetSessionTimelineProjectionsAsync(patientId);
            if (sessionProjections.Count == 0)
            {
                return new PatientLongitudinalAnalytics(
                    patientId,
                    new TrendResult(0, "InsufficientData"),
                    Array.Empty<SessionTimelineEntry>(),
                    Array.Empty<CategoryTrendEntry>());
            }

            var categoryProjections = await _categoryRepo.GetCategoryScoreProjectionsAsync(patientId);
            var categoriesBySession = categoryProjections
                .GroupBy(c => c.TrainingSessionId)
                .ToDictionary(
                    g => g.Key,
                    g => (IReadOnlyList<CategoryScoreEntry>)g
                        .Select(c => new CategoryScoreEntry(c.Category, AnalyticsHelpers.Round(c.AccuracyPercent)))
                        .ToList());

            var sessionTimeline = sessionProjections
                .Select(session =>
                {
                    categoriesBySession.TryGetValue(session.TrainingSessionId, out var categoryScores);
                    return new SessionTimelineEntry(
                        session.TrainingSessionId,
                        session.StartTime,
                        session.EndTime,
                        AnalyticsHelpers.Round(session.OverallScore),
                        categoryScores ?? Array.Empty<CategoryScoreEntry>());
                })
                .ToList();

            var overallTrend = ComputeOverallTrend(sessionTimeline);
            var categoryTrends = ComputeCategoryTrends(sessionTimeline);

            return new PatientLongitudinalAnalytics(
                patientId,
                overallTrend,
                sessionTimeline,
                categoryTrends);
        }

        // ─── Plan Analytics ──────────────────────────────────────────────────────

        public async Task<TherapyPlanAnalytics?> GetTherapyPlanAnalyticsAsync(int doctorId, int planId)
        {
            // 1. Load plan and verify ownership
            var plan = await _planRepo.GetPlanWithExercisesByIdAsync(planId);
            if (plan == null || plan.DoctorId != doctorId)
                return null;

            var planExercises = plan.PlanExercises ?? new List<PlanExercise>();
            var planExerciseIds = planExercises.Select(pe => pe.Id).ToList();

            // 2. Load training sessions for this plan
            var sessions = planExerciseIds.Any()
                ? (await _sessionRepo.GetByPlanExerciseIdsAsync(planExerciseIds)).ToList()
                : new List<TrainingSession>();

            var sessionIds = sessions.Select(s => s.Id).ToList();

            // 3. Load speech attempts and clinical reports for those sessions
            var attempts = sessionIds.Any()
                ? (await _attemptRepo.GetByTrainingSessionIdsAsync(sessionIds)).ToList()
                : new List<SpeechAttempt>();

            var reports = sessionIds.Any()
                ? (await _reportRepo.GetByTrainingSessionIdsAsync(sessionIds)).ToList()
                : new List<SessionClinicalReport>();

            // 4. Compute plan summary stats for comparison windows
            var currentAccuracy = sessions.Any() ? sessions.Average(s => s.AccuracyPercent) : 0;
            var currentSimilarity = sessions.Any() ? sessions.Average(s => s.AverageSimilarityScore) : 0;
            var currentFirstAttempt = attempts.Any()
                ? BuildFirstAttemptRate(attempts) : 0;

            // 5. Build progress comparisons
            var progressComparison = await BuildProgressComparisonAsync(
                plan, sessions, currentAccuracy, currentSimilarity, currentFirstAttempt);

            // 6. Delegate computation to the engine
            var analytics = PlanAnalyticsEngine.Compute(plan, sessions, attempts, reports, progressComparison);
            return analytics;
        }

        private static double BuildFirstAttemptRate(IReadOnlyList<SpeechAttempt> attempts)
        {
            var byWord = attempts.GroupBy(a => $"{a.VocabularyId}|{a.ExpectedWord}").ToList();
            if (!byWord.Any()) return 0;
            var firstAttemptCorrect = byWord.Count(g => g.OrderBy(a => a.AttemptNumber).First().IsCorrect);
            return Math.Round((double)firstAttemptCorrect / byWord.Count * 100, 2);
        }

        private async Task<PlanProgressComparison> BuildProgressComparisonAsync(
            TherapyPlan plan,
            IReadOnlyList<TrainingSession> planSessions,
            double currentAccuracy,
            double currentSimilarity,
            double currentFirstAttempt)
        {
            var patientId = plan.PatientId;
            var now = DateTime.UtcNow;

            // vs. last session in this plan (second-to-last)
            PlanPeriodComparison vsPrevSession;
            if (planSessions.Count >= 2)
            {
                var prevSession = planSessions[^2];
                vsPrevSession = PlanAnalyticsEngine.BuildPeriodComparison(
                    "Previous Session",
                    currentAccuracy,
                    currentSimilarity,
                    currentFirstAttempt,
                    prevSession.AccuracyPercent,
                    prevSession.AverageSimilarityScore,
                    null);
            }
            else
            {
                vsPrevSession = new PlanPeriodComparison("Previous Session", null, null, null, false);
            }

            // vs. previous plan for same patient
            PlanPeriodComparison vsPrevPlan;
            var allPatientPlans = (await _planRepo.GetByPatientIdAsync(patientId))
                .OrderByDescending(p => p.StartDate)
                .ToList();

            var prevPlan = allPatientPlans.FirstOrDefault(p => p.Id != plan.Id && p.StartDate < plan.StartDate);
            if (prevPlan != null)
            {
                var prevPlanExerciseIds = (await _planExerciseRepo.GetByPlanIdsAsync(new List<int> { prevPlan.Id }))
                    .Select(pe => pe.Id).ToList();
                var prevPlanSessions = prevPlanExerciseIds.Any()
                    ? (await _sessionRepo.GetByPlanExerciseIdsAsync(prevPlanExerciseIds)).ToList()
                    : new List<TrainingSession>();

                if (prevPlanSessions.Any())
                {
                    vsPrevPlan = PlanAnalyticsEngine.BuildPeriodComparison(
                        "Previous Plan",
                        currentAccuracy,
                        currentSimilarity,
                        currentFirstAttempt,
                        prevPlanSessions.Average(s => s.AccuracyPercent),
                        prevPlanSessions.Average(s => s.AverageSimilarityScore),
                        null);
                }
                else
                {
                    vsPrevPlan = new PlanPeriodComparison("Previous Plan", null, null, null, false);
                }
            }
            else
            {
                vsPrevPlan = new PlanPeriodComparison("Previous Plan", null, null, null, false);
            }

            // vs. last 7 days
            var sevenDaysAgo = now.AddDays(-7);
            var allRecentSessions7 = (await _sessionRepo.GetByPatientAsync(patientId, sevenDaysAgo, now)).ToList();
            var refSessions7 = allRecentSessions7.Where(s => !planSessions.Any(ps => ps.Id == s.Id)).ToList();
            var vs7Days = refSessions7.Any()
                ? PlanAnalyticsEngine.BuildPeriodComparison("Last 7 Days", currentAccuracy, currentSimilarity, currentFirstAttempt,
                    refSessions7.Average(s => s.AccuracyPercent), refSessions7.Average(s => s.AverageSimilarityScore), null)
                : new PlanPeriodComparison("Last 7 Days", null, null, null, false);

            // vs. last 30 days
            var thirtyDaysAgo = now.AddDays(-30);
            var allRecentSessions30 = (await _sessionRepo.GetByPatientAsync(patientId, thirtyDaysAgo, now)).ToList();
            var refSessions30 = allRecentSessions30.Where(s => !planSessions.Any(ps => ps.Id == s.Id)).ToList();
            var vs30Days = refSessions30.Any()
                ? PlanAnalyticsEngine.BuildPeriodComparison("Last 30 Days", currentAccuracy, currentSimilarity, currentFirstAttempt,
                    refSessions30.Average(s => s.AccuracyPercent), refSessions30.Average(s => s.AverageSimilarityScore), null)
                : new PlanPeriodComparison("Last 30 Days", null, null, null, false);

            return new PlanProgressComparison(vsPrevSession, vsPrevPlan, vs7Days, vs30Days);
        }

        // ─── Longitudinal trend helpers ──────────────────────────────────────────

        private static TrendResult ComputeOverallTrend(IReadOnlyList<SessionTimelineEntry> timeline)
        {
            if (timeline.Count < 2)
                return new TrendResult(0, "InsufficientData");

            var firstScore = timeline.First().OverallScore;
            var lastScore = timeline.Last().OverallScore;
            var delta = AnalyticsHelpers.Round(lastScore - firstScore);
            var direction = AnalyticsHelpers.DetermineTrendDirection(lastScore, firstScore);
            return new TrendResult(delta, direction);
        }

        private static IReadOnlyList<CategoryTrendEntry> ComputeCategoryTrends(IReadOnlyList<SessionTimelineEntry> timeline)
        {
            var categorySeries = timeline
                .SelectMany(session => session.CategoryScores.Select(score => new
                {
                    session.StartTime,
                    score.Category,
                    score.AccuracyPercent
                }))
                .GroupBy(x => x.Category)
                .Select(g =>
                {
                    var ordered = g.OrderBy(x => x.StartTime).ToList();
                    var firstScore = ordered.First().AccuracyPercent;
                    var lastScore = ordered.Last().AccuracyPercent;

                    if (ordered.Count < 2)
                    {
                        return new CategoryTrendEntry(
                            g.Key,
                            0,
                            "InsufficientData",
                            firstScore,
                            lastScore);
                    }

                    var delta = AnalyticsHelpers.Round(lastScore - firstScore);
                    var direction = AnalyticsHelpers.DetermineTrendDirection(lastScore, firstScore);
                    return new CategoryTrendEntry(g.Key, delta, direction, firstScore, lastScore);
                })
                .OrderBy(c => c.Category)
                .ToList();

            return categorySeries;
        }
    }
}
