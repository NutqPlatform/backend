using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using Nutq.Core.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nutq.Core.Services
{
    public class TherapyPlanService : ITherapyPlanService
    {
        private readonly ITherapyPlanRepository _planRepo;
        private readonly IPlanExerciseRepository _planExerciseRepo;
        private readonly IExerciseRepository _exerciseRepo;
        private readonly IDoctorRepository _doctorRepo;
        private readonly IPatientRepository _patientRepo;

        public TherapyPlanService(
            ITherapyPlanRepository planRepo,
            IPlanExerciseRepository planExerciseRepo,
            IExerciseRepository exerciseRepo,
            IDoctorRepository doctorRepo,
            IPatientRepository patientRepo)
        {
            _planRepo = planRepo;
            _planExerciseRepo = planExerciseRepo;
            _exerciseRepo = exerciseRepo;
            _doctorRepo = doctorRepo;
            _patientRepo = patientRepo;
        }

        public async Task<TherapyPlan> CreatePlanAsync(int doctorId, int patientId, CreateTherapyPlanCommand command)
{
    var doctor = await _doctorRepo.GetByIdAsync(doctorId);
    if (doctor == null) throw new Exception("Doctor not found");

    var patient = await _patientRepo.GetByIdAsync(patientId);
    if (patient == null) throw new Exception("Patient not found");
    if (patient.DoctorId != doctorId)
        throw new Exception("You cannot create a plan for a patient not assigned to you.");


    var plan = new TherapyPlan
    {
        DoctorId = doctorId,
        PatientId = patientId,
        Description = command.Description,
        Status = command.Status,
        StartDate = command.StartDate,
        EndDate = command.EndDate
    };

    await _planRepo.AddAsync(plan);

    return plan;
}

        public async Task<PlanExercise> AddExerciseToPlanAsync(int planId, AddPlanExerciseCommand command)
{
    var plan = await _planRepo.GetByIdAsync(planId);
    if (plan == null) throw new Exception("Therapy plan not found");

    var exercise = await _exerciseRepo.GetByIdAsync(command.ExerciseId);
    if (exercise == null) throw new Exception("Exercise not found");

    var planExercise = new PlanExercise
    {
        TherapyPlanId = planId,
        ExerciseId = command.ExerciseId,
        DurationMinutes = command.DurationMinutes,
        Repetition = command.Repetition,
        AiConstraints = command.AiConstraints
    };

    var created = await _planExerciseRepo.AddAsync(planExercise);

    
    return created;
}

        public async Task<IEnumerable<TherapyPlan>> GetPlansForPatientAsync(int doctorId, int patientId)
        {
            
            return await _planRepo.GetByDoctorAndPatientAsync(doctorId, patientId);
        }

        public async Task<TherapyPlan?> GetPlanByIdAsync(int planId)
        {
            return await _planRepo.GetByIdAsync(planId);
        }

        public async Task<TherapyPlan?> GetPlanWithExercisesForPatientAsync(int planId, int patientId)
        {
            return await _planRepo.GetPlanWithExercisesForPatientAsync(planId, patientId);
        }

        public async Task DeleteExerciseFromPlanAsync(int planId, int planExerciseId)
        {
            var plan = await _planRepo.GetByIdAsync(planId);
            if (plan == null) throw new Exception("Therapy plan not found");
            
            if (plan.Status != "Active")
                throw new Exception("Can only edit active plans");

            var planExercise = await _planExerciseRepo.GetByIdAsync(planExerciseId);
            if (planExercise == null) throw new Exception("Plan exercise not found");
            
            if (planExercise.TherapyPlanId != planId)
                throw new Exception("Exercise does not belong to this plan");

            await _planExerciseRepo.DeleteAsync(planExerciseId);
        }

        public async Task UpdatePlanStatusAsync(int planId, string status)
        {
            var plan = await _planRepo.GetByIdAsync(planId);
            if (plan == null) throw new Exception("Therapy plan not found");

            plan.Status = status;
            await _planRepo.UpdateAsync(plan);
        }

        
        public async Task<IEnumerable<TherapyPlan>> GetActivePlansForDoctorAsync(int doctorId)
{
    var allPlans = await _planRepo.GetPlansByDoctorAsync(doctorId);

    return allPlans.Where(p =>
        p.Status == "Active" &&
        (p.EndDate == null || p.EndDate > DateTime.UtcNow)
    );
}

    }
}
