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

        public DoctorAnalyticsService(
            IDoctorRepository doctorRepo,
            ITherapyPlanRepository planRepo,
            IPlanExerciseRepository planExerciseRepo,
            IExerciseProgressRepository progressRepo,
            IDoctorPatientRelationshipRepository relationshipRepo,
            IPatientAnalyticsService patientAnalyticsService,
            ITrainingSessionRepository sessionRepo,
            ICategoryPerformanceSnapshotRepository categoryRepo)
        {
            _doctorRepo = doctorRepo;
            _planRepo = planRepo;
            _planExerciseRepo = planExerciseRepo;
            _progressRepo = progressRepo;
            _relationshipRepo = relationshipRepo;
            _patientAnalyticsService = patientAnalyticsService;
            _sessionRepo = sessionRepo;
            _categoryRepo = categoryRepo;
        }

        public async Task<int> GetTotalPatientsAsync(int doctorId)
        {
            return await _relationshipRepo.CountDistinctPatientsAsync(doctorId);
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
