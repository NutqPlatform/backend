using Microsoft.AspNetCore.Mvc;
using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using Nutq.Web.DTOs;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Nutq.Web.Controllers
{
    [ApiController]
    [Route("api/weekly-reports")]
    public class WeeklyReportController : ControllerBase
    {
        private readonly IWeeklyReportRepository _weeklyReportRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly ITherapyPlanRepository _therapyPlanRepository;
        private readonly IDoctorPatientRelationshipRepository _relationshipRepo;

        public WeeklyReportController(
            IWeeklyReportRepository weeklyReportRepository,
            IPatientRepository patientRepository,
            ITherapyPlanRepository therapyPlanRepository,
            IDoctorPatientRelationshipRepository relationshipRepo)
        {
            _weeklyReportRepository = weeklyReportRepository;
            _patientRepository = patientRepository;
            _therapyPlanRepository = therapyPlanRepository;
            _relationshipRepo = relationshipRepo;
        }

        private int? GetCurrentDoctorId()
        {
            var user = JwtAuthorizationHelper.GetCurrentUser(Request);
            if (user == null || user.Value.Role != "doctor")
                return null;
            return user.Value.UserId;
        }

        private (int UserId, string Role)? GetCurrentUser()
        {
            return JwtAuthorizationHelper.GetCurrentUser(Request);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReport([FromBody] WeeklyReportCreateDto dto)
        {
            var loggedInDoctorId = GetCurrentDoctorId();
            if (loggedInDoctorId == null || loggedInDoctorId != dto.DoctorId)
                return Forbid();

            try
            {
                var validationError = await ValidateDoctorCanManageReportAsync(dto.DoctorId, dto.PatientId, dto.TherapyPlanId);
                if (validationError != null)
                    return BadRequest(new { error = validationError });

                // Check if report already exists for this plan
                if (dto.TherapyPlanId.HasValue)
                {
                    var existingReport = await _weeklyReportRepository.GetByTherapyPlanIdAsync(dto.TherapyPlanId.Value);
                    if (existingReport != null)
                    {
                        return BadRequest(new { error = "A report already exists for this plan. Use the update endpoint instead." });
                    }
                }

                var entity = new WeeklyReport
                {
                    DoctorId = dto.DoctorId,
                    PatientId = dto.PatientId,
                    TherapyPlanId = dto.TherapyPlanId,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    TotalHours = dto.TotalHours,
                    DoctorNotes = dto.DoctorNotes
                };

                var created = await _weeklyReportRepository.AddAsync(entity);

                var result = new WeeklyReportDto
                {
                    Id = created.Id,
                    DoctorId = created.DoctorId,
                    PatientId = created.PatientId,
                    TherapyPlanId = created.TherapyPlanId,
                    StartDate = created.StartDate,
                    EndDate = created.EndDate,
                    TotalHours = created.TotalHours,
                    DoctorNotes = created.DoctorNotes
                };

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReport(int id, [FromBody] WeeklyReportCreateDto dto)
        {
            var loggedInDoctorId = GetCurrentDoctorId();
            if (loggedInDoctorId == null || loggedInDoctorId != dto.DoctorId)
                return Forbid();

            try
            {
                var report = await _weeklyReportRepository.GetByIdAsync(id);
                if (report == null)
                {
                    return NotFound(new { error = "Report not found" });
                }

                if (report.DoctorId != loggedInDoctorId)
                    return Forbid();

                var validationError = await ValidateDoctorCanManageReportAsync(
                    report.DoctorId,
                    report.PatientId,
                    report.TherapyPlanId);
                if (validationError != null)
                    return BadRequest(new { error = validationError });

                report.DoctorNotes = dto.DoctorNotes;
                report.TotalHours = dto.TotalHours;
                report.StartDate = dto.StartDate;
                report.EndDate = dto.EndDate;

                await _weeklyReportRepository.UpdateAsync(report);

                var result = new WeeklyReportDto
                {
                    Id = report.Id,
                    DoctorId = report.DoctorId,
                    PatientId = report.PatientId,
                    TherapyPlanId = report.TherapyPlanId,
                    StartDate = report.StartDate,
                    EndDate = report.EndDate,
                    TotalHours = report.TotalHours,
                    DoctorNotes = report.DoctorNotes
                };

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("plan/{planId}")]
        public async Task<IActionResult> GetReportByPlan(int planId)
        {
            var user = GetCurrentUser();
            if (user == null)
                return Forbid();

            var report = await _weeklyReportRepository.GetByTherapyPlanIdAsync(planId);
            if (report == null)
            {
                return Ok(null);
            }

            if (user.Value.Role == "doctor")
            {
                if (report.DoctorId != user.Value.UserId)
                    return Forbid();
            }
            else if (user.Value.Role == "patient")
            {
                if (report.PatientId != user.Value.UserId)
                    return Forbid();
            }
            else
            {
                return Forbid();
            }

            var result = new WeeklyReportDto
            {
                Id = report.Id,
                DoctorId = report.DoctorId,
                PatientId = report.PatientId,
                TherapyPlanId = report.TherapyPlanId,
                StartDate = report.StartDate,
                EndDate = report.EndDate,
                TotalHours = report.TotalHours,
                DoctorNotes = report.DoctorNotes
            };

            return Ok(result);
        }

        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetReportsForPatient(int patientId)
        {
            var user = GetCurrentUser();
            if (user == null)
                return Forbid();

            if (user.Value.Role == "doctor")
            {
                if (!await _relationshipRepo.HasRelationshipAsync(user.Value.UserId, patientId))
                    return Forbid();
            }
            else if (user.Value.Role == "patient")
            {
                if (patientId != user.Value.UserId)
                    return Forbid();
            }
            else
            {
                return Forbid();
            }

            var reports = await _weeklyReportRepository.GetByPatientIdAsync(patientId);
            IEnumerable<WeeklyReport> filteredReports = reports;

            if (user.Value.Role == "doctor")
            {
                filteredReports = reports.Where(r => r.DoctorId == user.Value.UserId);
            }

            var list = filteredReports.Select(r => new WeeklyReportDto
            {
                Id = r.Id,
                DoctorId = r.DoctorId,
                PatientId = r.PatientId,
                TherapyPlanId = r.TherapyPlanId,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                TotalHours = r.TotalHours,
                DoctorNotes = r.DoctorNotes
            }).ToList();

            return Ok(list);
        }

        private async Task<string?> ValidateDoctorCanManageReportAsync(int doctorId, int patientId, int? therapyPlanId)
        {
            var patient = await _patientRepository.GetByIdAsync(patientId);
            if (patient == null)
                return "Patient not found";

            var active = await _relationshipRepo.GetActiveAsync(doctorId, patientId);
            if (active == null)
                return "Patient is no longer assigned to you. Reports cannot be created or edited.";

            if (therapyPlanId.HasValue)
            {
                var plan = await _therapyPlanRepository.GetByIdAsync(therapyPlanId.Value);
                if (plan == null)
                    return "Therapy plan not found";

                if (plan.IsArchived)
                    return "This plan is archived and reports cannot be modified.";

                if (plan.DoctorId != doctorId || plan.PatientId != patientId)
                    return "Therapy plan does not belong to this doctor and patient.";
            }

            return null;
        }
    }
}
