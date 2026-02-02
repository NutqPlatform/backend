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

        public WeeklyReportController(IWeeklyReportRepository weeklyReportRepository)
        {
            _weeklyReportRepository = weeklyReportRepository;
        }

        [HttpPost]
        public async Task<IActionResult> CreateReport([FromBody] WeeklyReportCreateDto dto)
        {
            try
            {
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
            try
            {
                var report = await _weeklyReportRepository.GetByIdAsync(id);
                if (report == null)
                {
                    return NotFound(new { error = "Report not found" });
                }

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
            var report = await _weeklyReportRepository.GetByTherapyPlanIdAsync(planId);
            if (report == null)
            {
                return Ok(null);
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
            var reports = await _weeklyReportRepository.GetByPatientIdAsync(patientId);

            var list = reports.Select(r => new WeeklyReportDto
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
    }
}
