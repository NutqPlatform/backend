using Nutq.Core.Entities;
using Nutq.Core.Interfaces;

namespace Nutq.Core.Services
{
    public class PatientDashboardService : IPatientDashboardService
    {
        private readonly IPatientRepository _patientRepository;

        public PatientDashboardService(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        public async Task<List<TherapyPlan>> GetPatientPlansAsync(int patientId)
        {
            return await _patientRepository.GetPatientPlansAsync(patientId);
        }

        public async Task<List<ExerciseProgress>> GetPatientProgressAsync(int patientId)
        {
            return await _patientRepository.GetPatientProgressAsync(patientId);
        }
    }
}
