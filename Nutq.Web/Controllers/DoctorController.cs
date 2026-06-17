using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using Nutq.Core.Interfaces;
using Nutq.Web.DTOs;

namespace Nutq.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorService _doctorService;
        private readonly IWebHostEnvironment _env;

            public DoctorController(IDoctorService doctorService, IWebHostEnvironment env)
            {
                _doctorService = doctorService;
                _env = env;
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
        string? fileUrl = null;
        if (!string.IsNullOrEmpty(dto.DiagnosisFileBase64) && !string.IsNullOrEmpty(dto.DiagnosisFileName))
        {
            try
            {
                var uploadsRoot = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "diagnoses");
                Directory.CreateDirectory(uploadsRoot);

                var safeFileName = Path.GetFileName(dto.DiagnosisFileName);
                var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                var outName = $"{patientId}_{timestamp}_{safeFileName}";
                var outPath = Path.Combine(uploadsRoot, outName);

                var bytes = Convert.FromBase64String(dto.DiagnosisFileBase64);
                await System.IO.File.WriteAllBytesAsync(outPath, bytes);

                fileUrl = $"/uploads/diagnoses/{outName}";
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = "Failed to save diagnosis file: " + ex.Message });
            }
        }

        await _doctorService.UpdatePatientDiagnosisAsync(doctorId, patientId, dto.Diagnosis ?? string.Empty, fileUrl);
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
        string? cvUrl = dto.CV;
        if (!string.IsNullOrEmpty(dto.CvFileBase64) && !string.IsNullOrEmpty(dto.CvFileName))
        {
            try
            {
                var uploadsRoot = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "cv");
                Directory.CreateDirectory(uploadsRoot);
                var safeFileName = Path.GetFileName(dto.CvFileName);
                var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                var outName = $"{doctorId}_{timestamp}_{safeFileName}";
                var outPath = Path.Combine(uploadsRoot, outName);
                var bytes = Convert.FromBase64String(dto.CvFileBase64);
                await System.IO.File.WriteAllBytesAsync(outPath, bytes);
                cvUrl = $"/uploads/cv/{outName}";
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = "Failed to save CV file: " + ex.Message });
            }
        }

        // Parse dateOfBirth string safely as UTC
        DateTime? parsedDob = null;
        if (!string.IsNullOrWhiteSpace(dto.DateOfBirth))
        {
            if (DateTime.TryParse(dto.DateOfBirth, null, System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal, out var dt))
                parsedDob = dt;
        }

        await _doctorService.UpdateDoctorProfileAsync(
            doctorId,
            dto.ProfilePicture,
            cvUrl,
            dto.Name,
            dto.PhoneNumber,
            dto.CommunicationInfo,
            dto.Address,
            parsedDob,
            dto.CvText
        );

        var updated = await _doctorService.GetDoctorProfileAsync(doctorId);
        return Ok(updated);
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
