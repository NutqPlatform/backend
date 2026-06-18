using Nutq.Core.Entities;
using Nutq.Core.Interfaces;

namespace Nutq.Core.Services
{
    public class DoctorAnalyticsService : IDoctorAnalyticsService
    {
        private readonly IDoctorRepository _doctorRepo;
        private readonly ITherapyPlanRepository _planRepo;
        private readonly IPlanExerciseRepository _planExerciseRepo;
        private readonly IExerciseProgressRepository _progressRepo;
        private readonly IDoctorPatientRelationshipRepository _relationshipRepo;

        public DoctorAnalyticsService(
            IDoctorRepository doctorRepo,
            ITherapyPlanRepository planRepo,
            IPlanExerciseRepository planExerciseRepo,
            IExerciseProgressRepository progressRepo,
            IDoctorPatientRelationshipRepository relationshipRepo)
        {
            _doctorRepo = doctorRepo;
            _planRepo = planRepo;
            _planExerciseRepo = planExerciseRepo;
            _progressRepo = progressRepo;
            _relationshipRepo = relationshipRepo;
        }

        public async Task<int> GetTotalPatientsAsync(int doctorId)
        {
            return await _relationshipRepo.CountDistinctPatientsAsync(doctorId);
        }

        public async Task<int> GetTotalPlansAsync(int doctorId)
        {
            var plans = await _planRepo.GetPlansByDoctorAsync(doctorId) ?? new List<TherapyPlan>();
            return plans.Count;
        }

        public async Task<int> GetTotalExercisesAsync(int doctorId)
        {
            var plans = await _planRepo.GetPlansByDoctorAsync(doctorId) ?? new List<TherapyPlan>();
            var planIds = plans.Select(p => p.Id).ToList();
            var exercises = await _planExerciseRepo.GetByPlanIdsAsync(planIds) ?? new List<PlanExercise>();
            return exercises.Count;
        }

       public async Task<double> GetAverageCompletionRateAsync(int doctorId)
{
    var plans = await _planRepo.GetPlansByDoctorAsync(doctorId) ?? new List<TherapyPlan>();
    if (!plans.Any())
        return 0;

    var planIds = plans.Select(p => p.Id).ToList();
    var planExercises = await _planExerciseRepo.GetByPlanIdsAsync(planIds) ?? new List<PlanExercise>();
    if (!planExercises.Any())
        return 0;

    var progresses = await _progressRepo.GetByPlanExerciseIdsAsync(planExercises.Select(pe => pe.Id).ToList()) ?? new List<ExerciseProgress>();

    int totalExercises = planExercises.Count;

    // أي تمرين مكتمل مرة واحدة على الأقل يعتبر مكتمل
    int completedExercises = progresses
        .Where(p => p.Completed)
        .Select(p => p.PlanExerciseId)
        .Distinct()
        .Count();

    double completionRate = (double)completedExercises / totalExercises * 100;

    return completionRate;
}

    }
}
