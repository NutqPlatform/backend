using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
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
            var user = JwtAuthorizationHelper.GetCurrentUser(Request);
            if (user == null || user.Value.Role != "doctor" || user.Value.UserId != doctorId)
                return Unauthorized(new { success = false, error = "Not authorized" });

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
            var user = JwtAuthorizationHelper.GetCurrentUser(Request);
            if (user == null || user.Value.Role != "doctor" || user.Value.UserId != doctorId)
                return Unauthorized(new { success = false, error = "Not authorized" });

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
            var user = JwtAuthorizationHelper.GetCurrentUser(Request);
            if (user == null || user.Value.Role != "doctor" || user.Value.UserId != doctorId)
                return Unauthorized(new { success = false, error = "Not authorized" });

            try
            {
                var patient = await _doctorService.GetPatientByIdAsync(doctorId, patientId);
                return Ok(patient);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("not found") || ex.Message.Contains("does not belong"))
                    return NotFound(new { success = false, error = ex.Message });
                
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        [HttpPut("{doctorId}/patients/{patientId}/diagnosis")]
        public async Task<IActionResult> UpdatePatientDiagnosis(int doctorId, int patientId, [FromBody] UpdateDiagnosisDto dto)
        {
            var user = JwtAuthorizationHelper.GetCurrentUser(Request);
            if (user == null || user.Value.Role != "doctor" || user.Value.UserId != doctorId)
                return Unauthorized(new { success = false, error = "Not authorized" });

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
            var user = JwtAuthorizationHelper.GetCurrentUser(Request);
            // GET /{doctorId}/profile: open to any authenticated user (patient or doctor can view profiles)
            if (user == null)
                return Unauthorized(new { success = false, error = "Authentication required" });

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
            var user = JwtAuthorizationHelper.GetCurrentUser(Request);
            if (user == null || user.Value.Role != "doctor" || user.Value.UserId != doctorId)
                return Unauthorized(new { success = false, error = "Not authorized" });

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
            var user = JwtAuthorizationHelper.GetCurrentUser(Request);
            if (user == null || user.Value.Role != "doctor" || user.Value.UserId != doctorId)
                return Unauthorized(new { success = false, error = "Not authorized" });

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
            // GET /all: open to any authenticated user — patients and doctors can view doctor profiles
            // JWT is optional; used only to include private data (patients/weeklyReports) for the doctor themselves
            // No hard auth block — unauthenticated requests simply get no private data

            try
            {
                // Read JWT optionally — no token means no private data is exposed
                var user = JwtAuthorizationHelper.GetCurrentUser(Request);

                var doctors = await _doctorService.GetAllDoctorsWithCommunicationsAsync();

                // Use camelCase to match ASP.NET's default serialization and the frontend's expectations
                var serializeOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(doctors, serializeOptions);
                using var doc = JsonDocument.Parse(jsonBytes);
                var root = doc.RootElement;

                var filtered = new List<JsonElement>();

                foreach (var element in root.EnumerateArray())
                {
                    // Properties are now camelCase: "id", "name", etc.
                    int docId = element.TryGetProperty("id", out var idProp) ? idProp.GetInt32() : 0;
                    bool isSelf = user.HasValue && user.Value.Role == "doctor" && user.Value.UserId == docId;

                    if (isSelf)
                    {
                        filtered.Add(element.Clone());
                    }
                    else
                    {
                        // Rebuild element without patients and weeklyReports
                        var dict = new Dictionary<string, object?>();
                        foreach (var prop in element.EnumerateObject())
                        {
                            if (prop.Name == "patients" || prop.Name == "weeklyReports")
                                continue;
                            dict[prop.Name] = JsonSerializer.Deserialize<object>(prop.Value.GetRawText());
                        }

                        // Determine the original patient count and create placeholders
                        int originalPatientCount = 0;
                        if (element.TryGetProperty("patients", out var patientsProp) && patientsProp.ValueKind == JsonValueKind.Array)
                        {
                            originalPatientCount = patientsProp.GetArrayLength();
                        }
                        var dummyPatients = new List<object>();
                        for (int i = 0; i < originalPatientCount; i++)
                        {
                            dummyPatients.Add(new { id = 0 });
                        }
                        dict["patients"] = dummyPatients;

                        // Determine original reports and populate either the requester's actual report or placeholders
                        var dummyReports = new List<object>();
                        if (element.TryGetProperty("weeklyReports", out var reportsProp) && reportsProp.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var r in reportsProp.EnumerateArray())
                            {
                                int reportPatientId = r.TryGetProperty("patientId", out var pIdProp) ? pIdProp.GetInt32() : 0;
                                // If logged-in user is a patient and this report is theirs, they can see it
                                if (user.HasValue && user.Value.Role == "patient" && user.Value.UserId == reportPatientId)
                                {
                                    dummyReports.Add(JsonSerializer.Deserialize<object>(r.GetRawText())!);
                                }
                                else
                                {
                                    dummyReports.Add(new { id = 0, patientId = 0 });
                                }
                            }
                        }
                        dict["weeklyReports"] = dummyReports;

                        var rebuiltBytes = JsonSerializer.SerializeToUtf8Bytes(dict);
                        using var rebuiltDoc = JsonDocument.Parse(rebuiltBytes);
                        filtered.Add(rebuiltDoc.RootElement.Clone());
                    }
                }

                return Ok(new { doctors = filtered });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpGet("{doctorId}/communications")]
        public async Task<IActionResult> GetDoctorCommunications(int doctorId)
        {
            // GET /{doctorId}/communications: open to any authenticated user
            // JWT is optional; only used to expose patients/weeklyReports to the doctor themselves

            try
            {
                // Read JWT optionally — no token or non-doctor token means private data is stripped
                var user = JwtAuthorizationHelper.GetCurrentUser(Request);
                var doctor = await _doctorService.GetDoctorWithCommunicationsAsync(doctorId);
                bool isSelf = user.HasValue && user.Value.Role == "doctor" && user.Value.UserId == doctorId;

                var serializeOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(doctor, serializeOptions);
                using var doc = JsonDocument.Parse(jsonBytes);
                var element = doc.RootElement;

                if (isSelf)
                {
                    return Content(element.GetRawText(), "application/json");
                }

                // Strip patients and weeklyReports for non-self requests, preserving count with placeholder items
                var dict = new Dictionary<string, object?>();
                foreach (var prop in element.EnumerateObject())
                {
                    if (prop.Name == "patients" || prop.Name == "weeklyReports")
                        continue;
                    dict[prop.Name] = JsonSerializer.Deserialize<object>(prop.Value.GetRawText());
                }

                int originalPatientCount = 0;
                if (element.TryGetProperty("patients", out var patientsProp) && patientsProp.ValueKind == JsonValueKind.Array)
                {
                    originalPatientCount = patientsProp.GetArrayLength();
                }
                var dummyPatients = new List<object>();
                for (int i = 0; i < originalPatientCount; i++)
                {
                    dummyPatients.Add(new { id = 0 });
                }
                dict["patients"] = dummyPatients;

                var dummyReports = new List<object>();
                if (element.TryGetProperty("weeklyReports", out var reportsProp) && reportsProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var r in reportsProp.EnumerateArray())
                    {
                        int reportPatientId = r.TryGetProperty("patientId", out var pIdProp) ? pIdProp.GetInt32() : 0;
                        if (user.HasValue && user.Value.Role == "patient" && user.Value.UserId == reportPatientId)
                        {
                            dummyReports.Add(JsonSerializer.Deserialize<object>(r.GetRawText())!);
                        }
                        else
                        {
                            dummyReports.Add(new { id = 0, patientId = 0 });
                        }
                    }
                }
                dict["weeklyReports"] = dummyReports;

                return Ok(dict);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("not found"))
                    return NotFound(new { success = false, error = ex.Message });

                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }
}
