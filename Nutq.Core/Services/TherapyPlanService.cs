using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using Nutq.Core.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IDoctorPatientRelationshipRepository _relationshipRepo;

        public TherapyPlanService(
            ITherapyPlanRepository planRepo,
            IPlanExerciseRepository planExerciseRepo,
            IExerciseRepository exerciseRepo,
            IDoctorRepository doctorRepo,
            IPatientRepository patientRepo,
            IDoctorPatientRelationshipRepository relationshipRepo)
        {
            _planRepo = planRepo;
            _planExerciseRepo = planExerciseRepo;
            _exerciseRepo = exerciseRepo;
            _doctorRepo = doctorRepo;
            _patientRepo = patientRepo;
            _relationshipRepo = relationshipRepo;
        }

        public async Task<TherapyPlan> CreatePlanAsync(int doctorId, int patientId, CreateTherapyPlanCommand command)
        {
            if (command.Exercises == null || command.Exercises.Count == 0)
                throw new Exception("A therapy plan must contain at least one exercise.");

            var doctor = await _doctorRepo.GetByIdAsync(doctorId);
            if (doctor == null) throw new Exception("Doctor not found");

            var patient = await _patientRepo.GetByIdAsync(patientId);
            if (patient == null) throw new Exception("Patient not found");
            var activeRelationship = await _relationshipRepo.GetActiveAsync(doctorId, patientId);
            if (activeRelationship == null)
                throw new Exception("You cannot create a plan for a patient not assigned to you.");

            var endDate = command.EndDate ?? command.StartDate.AddDays(7);
            if (endDate <= command.StartDate)
                throw new Exception("End date must be after start date.");

            await PauseActivePlansForPatientAsync(patientId);

            var plan = new TherapyPlan
            {
                DoctorId = doctorId,
                PatientId = patientId,
                Description = command.Description,
                Status = "Active",
                StartDate = command.StartDate,
                EndDate = endDate
            };

            await _planRepo.AddAsync(plan);

            foreach (var exerciseCommand in command.Exercises)
            {
                await AddExerciseToPlanInternalAsync(plan.Id, exerciseCommand);
            }

            return plan;
        }

        public async Task<IEnumerable<PlanExercise>> AddExerciseToPlanAsync(int planId, AddPlanExerciseCommand command)
        {
            var plan = await _planRepo.GetByIdAsync(planId);
            if (plan == null) throw new Exception("Therapy plan not found");

            await EnsureDoctorCanManagePlanAsync(plan);

            return await AddExerciseToPlanInternalAsync(planId, command);
        }

        private async Task<IEnumerable<PlanExercise>> AddExerciseToPlanInternalAsync(int planId, AddPlanExerciseCommand command)
{
    var exercise = await _exerciseRepo.GetByIdAsync(command.ExerciseId);
    if (exercise == null) throw new Exception("Exercise not found");

    if (command.Repetition < 1)
        throw new Exception("Repetition must be at least 1");

    var created = new List<PlanExercise>();
    for (var i = 0; i < command.Repetition; i++)
    {
        var planExercise = new PlanExercise
        {
            TherapyPlanId = planId,
            ExerciseId = command.ExerciseId,
            DurationMinutes = command.DurationMinutes,
            Repetition = 1,
            AiConstraints = command.AiConstraints
        };

        var saved = await _planExerciseRepo.AddAsync(planExercise);
        saved.Exercise = exercise;
        created.Add(saved);
    }

    return created;
}

        public async Task<IEnumerable<TherapyPlan>> GetPlansForPatientAsync(int doctorId, int patientId)
        {
            var patient = await _patientRepo.GetByIdAsync(patientId);
            if (patient == null)
                throw new Exception("Patient not found");

            // A doctor can always view plans they created for a patient,
            // regardless of whether the relationship is still active (i.e., after release or transfer).
            // Authorization is based on plan ownership (plan.DoctorId), not active relationship.
            if (!await _relationshipRepo.HasRelationshipAsync(doctorId, patientId))
                throw new Exception("Patient does not belong to this doctor");

            // Return ALL of this doctor's plans for this patient — archived and non-archived.
            // The UI handles the read-only display for archived/former-patient plans.
            var plans = await _planRepo.GetByDoctorAndPatientAsync(doctorId, patientId);
            return plans;
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

            await EnsureDoctorCanManagePlanAsync(plan);
            
            if (plan.Status != "Active")
                throw new Exception("Can only edit active plans");

            var planExercise = await _planExerciseRepo.GetByIdAsync(planExerciseId);
            if (planExercise == null) throw new Exception("Plan exercise not found");
            
            if (planExercise.TherapyPlanId != planId)
                throw new Exception("Exercise does not belong to this plan");

            var planExercises = await _planExerciseRepo.GetByPlanIdsAsync(new List<int> { planId });
            if (planExercises.Count <= 1)
                throw new Exception("A therapy plan must contain at least one exercise.");

            await _planExerciseRepo.DeleteAsync(planExerciseId);
        }

        public async Task UpdatePlanStatusAsync(int planId, string status)
        {
            var plan = await _planRepo.GetByIdAsync(planId);
            if (plan == null) throw new Exception("Therapy plan not found");

            await EnsureDoctorCanManagePlanAsync(plan);

            if (status == "Active")
            {
                await PauseActivePlansForPatientAsync(plan.PatientId, planId);
            }

            plan.Status = status;
            await _planRepo.UpdateAsync(plan);
        }

        public async Task<TherapyPlan> UpdatePlanAsync(int planId, UpdateTherapyPlanCommand command)
        {
            var plan = await _planRepo.GetByIdAsync(planId);
            if (plan == null) throw new Exception("Therapy plan not found");

            await EnsureDoctorCanManagePlanAsync(plan);

            if (command.Description != null)
                plan.Description = command.Description;

            if (command.EndDate.HasValue)
            {
                if (command.EndDate.Value <= plan.StartDate)
                    throw new Exception("End date must be after start date.");
                plan.EndDate = command.EndDate.Value;
            }

            await _planRepo.UpdateAsync(plan);
            return plan;
        }

        private async Task PauseActivePlansForPatientAsync(int patientId, int? exceptPlanId = null)
        {
            var patientPlans = await _planRepo.GetByPatientIdAsync(patientId);
            foreach (var existingPlan in patientPlans.Where(p => !p.IsArchived && p.Status == "Active" && p.Id != exceptPlanId))
            {
                existingPlan.Status = "Paused";
                await _planRepo.UpdateAsync(existingPlan);
            }
        }

        public async Task<IEnumerable<TherapyPlan>> GetActivePlansForDoctorAsync(int doctorId)
{
    var allPlans = await _planRepo.GetOngoingPlansByDoctorAsync(doctorId);

    return allPlans.Where(p =>
        p.Status == "Active" &&
        (p.EndDate == null || p.EndDate > DateTime.UtcNow)
    );
}

        public async Task<IEnumerable<TherapyPlan>> GetOngoingPlansForDoctorAsync(int doctorId)
        {
            return await _planRepo.GetOngoingPlansByDoctorAsync(doctorId);
        }

        public async Task<IEnumerable<TherapyPlan>> GetPlansByDoctorAsync(int doctorId)
        {
            return await _planRepo.GetPlansByDoctorAsync(doctorId);
        }

        private async Task EnsureDoctorCanManagePlanAsync(TherapyPlan plan)
        {
            if (plan.IsArchived)
                throw new Exception("This plan is archived and cannot be modified.");

            var activeRelationship = await _relationshipRepo.GetActiveAsync(plan.DoctorId, plan.PatientId);
            if (activeRelationship == null)
                throw new Exception("Patient is no longer assigned to you. Plans and reports are read-only.");
        }

    }
}