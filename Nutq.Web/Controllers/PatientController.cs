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
                await _patientService.UpdatePatientProfileAsync(patientId, dto.ProfilePicture);
                return Ok(new { success = true, message = "Profile updated successfully" });
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
                    return NotFound(new { success = false, error = "Doctor not found" });
                return Ok(doctor);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }
    }
}
