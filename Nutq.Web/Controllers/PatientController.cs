using Microsoft.AspNetCore.Mvc;
using Nutq.Core.Interfaces;
using Nutq.Web.DTOs;

namespace Nutq.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public PatientController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        [HttpGet("{patientId}/profile")]
        public async Task<IActionResult> GetProfile(int patientId)
        {
            try
            {
                var profile = await _patientService.GetPatientProfileAsync(patientId);
                return Ok(profile);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        [HttpPut("{patientId}/profile")]
        public async Task<IActionResult> UpdateProfile(int patientId, [FromBody] UpdatePatientProfileDto dto)
        {
            try
            {
                DateTime? parsedDob = null;
                if (!string.IsNullOrWhiteSpace(dto.DateOfBirth))
                {
                    if (DateTime.TryParse(dto.DateOfBirth, out var dt))
                        parsedDob = DateTime.SpecifyKind(dt.Date, DateTimeKind.Utc);
                }

                await _patientService.UpdatePatientProfileAsync(
                    patientId,
                    dto.ProfilePicture,
                    dto.PhoneNumber,
                    parsedDob
                );

                var updated = await _patientService.GetPatientProfileAsync(patientId);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        [HttpPut("{patientId}/password")]
        public async Task<IActionResult> UpdatePassword(int patientId, [FromBody] UpdatePasswordDto dto)
        {
            try
            {
                await _patientService.UpdatePatientPasswordAsync(patientId, dto.CurrentPassword, dto.NewPassword);
                return Ok(new { success = true, message = "Password updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        [HttpGet("{patientId}/doctor")]
        public async Task<IActionResult> GetAttendingDoctor(int patientId)
        {
            try
            {
                var doctor = await _patientService.GetAttendingDoctorAsync(patientId);
                if (doctor == null)
                    return Ok(new { hasDoctor = false });
                return Ok(doctor);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }
    }
}
