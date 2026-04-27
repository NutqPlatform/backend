using Microsoft.AspNetCore.Mvc;
using Nutq.Core.Interfaces;
using Nutq.Web.DTOs;

namespace Nutq.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        
        [HttpPost("{doctorId}/generate-patient-code")]
        public async Task<IActionResult> GeneratePatientCode(int doctorId)
        {
            try
            {
                var code = await _doctorService.GeneratePatientCodeAsync(doctorId);

                return Ok(new
                {
                    success = true,
                    message = "Patient code generated successfully",
                    code = code
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }


[HttpGet("{doctorId}/patients")]
public async Task<IActionResult> GetPatients(int doctorId)
{
    try
    {
        var patients = await _doctorService.GetDoctorPatientsAsync(doctorId);
        return Ok(new { patients });
    }
    catch (Exception ex)
    {
        return BadRequest(new { success = false, error = ex.Message });
    }
}

[HttpGet("{doctorId}/patients/{patientId}")]
public async Task<IActionResult> GetPatient(int doctorId, int patientId)
{
    try
    {
        var patient = await _doctorService.GetPatientByIdAsync(doctorId, patientId);
        return Ok(patient);
    }
    catch (Exception ex)
    {
        // Check if it's a "not found" type error
        if (ex.Message.Contains("not found") || ex.Message.Contains("does not belong"))
            return NotFound(new { success = false, error = ex.Message });
        
        return BadRequest(new { success = false, error = ex.Message });
    }
}

[HttpPut("{doctorId}/patients/{patientId}/diagnosis")]
public async Task<IActionResult> UpdatePatientDiagnosis(int doctorId, int patientId, [FromBody] UpdateDiagnosisDto dto)
{
    try
    {
        await _doctorService.UpdatePatientDiagnosisAsync(doctorId, patientId, dto.Diagnosis ?? string.Empty);
        return Ok(new { success = true, message = "Diagnosis updated successfully" });
    }
    catch (Exception ex)
    {
        return BadRequest(new { success = false, error = ex.Message });
    }
}

[HttpGet("{doctorId}/profile")]
public async Task<IActionResult> GetProfile(int doctorId)
{
    try
    {
        var profile = await _doctorService.GetDoctorProfileAsync(doctorId);
        return Ok(profile);
    }
    catch (Exception ex)
    {
        return BadRequest(new { success = false, error = ex.Message });
    }
}

[HttpPut("{doctorId}/profile")]
public async Task<IActionResult> UpdateProfile(int doctorId, [FromBody] UpdateDoctorProfileDto dto)
{
    try
    {
        await _doctorService.UpdateDoctorProfileAsync(doctorId, dto.ProfilePicture, dto.CV);
        return Ok(new { success = true, message = "Profile updated successfully" });
    }
    catch (Exception ex)
    {
        return BadRequest(new { success = false, error = ex.Message });
    }
}

[HttpPut("{doctorId}/password")]
public async Task<IActionResult> UpdatePassword(int doctorId, [FromBody] UpdatePasswordDto dto)
{
    try
    {
        await _doctorService.UpdateDoctorPasswordAsync(doctorId, dto.CurrentPassword, dto.NewPassword);
        return Ok(new { success = true, message = "Password updated successfully" });
    }
    catch (Exception ex)
    {
        return BadRequest(new { success = false, error = ex.Message });
    }
}

[HttpGet("all")]
public async Task<IActionResult> GetAllDoctorsWithCommunications()
{
    try
    {
        var doctors = await _doctorService.GetAllDoctorsWithCommunicationsAsync();
        return Ok(new { doctors });
    }
    catch (Exception ex)
    {
        return BadRequest(new { success = false, error = ex.Message });
    }
}

[HttpGet("{doctorId}/communications")]
public async Task<IActionResult> GetDoctorCommunications(int doctorId)
{
    try
    {
        var doctor = await _doctorService.GetDoctorWithCommunicationsAsync(doctorId);
        return Ok(doctor);
    }
    catch (Exception ex)
    {
        if (ex.Message.Contains("not found"))
            return NotFound(new { success = false, error = ex.Message });

        return BadRequest(new { success = false, error = ex.Message });
    }
}

    }
}
