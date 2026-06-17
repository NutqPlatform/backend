using Microsoft.AspNetCore.Mvc;
using Nutq.Core.Interfaces;
using Nutq.Web.DTOs;

namespace Nutq.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IInvitationCodeRepository _invitationCodeRepository;

        public AdminController(
            IAdminService adminService,
            IDoctorRepository doctorRepository,
            IPatientRepository patientRepository,
            IInvitationCodeRepository invitationCodeRepository)
        {
            _adminService = adminService;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
            _invitationCodeRepository = invitationCodeRepository;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AdminLoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest(new { message = "Email and password are required" });

            var result = await _adminService.LoginAsync(request.Email, request.Password);
            
            if (!result.Success)
                return Unauthorized(new { message = result.Message });

            return Ok(result);
        }

        [HttpGet("doctors")]
        public async Task<IActionResult> GetAllDoctors()
        {
            var doctors = await _doctorRepository.GetAllAsync();
            return Ok(doctors.Select(d => new
            {
                d.Id,
                d.Name,
                d.Email,
                d.PhoneNumber,
                d.IsBlocked,
                d.AverageRating
            }));
        }

        [HttpGet("patients")]
        public async Task<IActionResult> GetAllPatients()
        {
            var patients = await _patientRepository.GetAllAsync();
            return Ok(patients.Select(p => new
            {
                p.Id,
                p.Name,
                p.Email,
                p.PhoneNumber,
                p.DoctorId,
                p.IsBlocked
            }));
        }

        [HttpPost("doctors/{doctorId}/block")]
        public async Task<IActionResult> BlockDoctor(int doctorId)
        {
            var result = await _adminService.BlockDoctorAsync(doctorId);
            if (!result)
                return NotFound(new { message = "Doctor not found" });

            return Ok(new { message = "Doctor blocked successfully" });
        }

        [HttpPost("doctors/{doctorId}/unblock")]
        public async Task<IActionResult> UnblockDoctor(int doctorId)
        {
            var result = await _adminService.UnblockDoctorAsync(doctorId);
            if (!result)
                return NotFound(new { message = "Doctor not found" });

            return Ok(new { message = "Doctor unblocked successfully" });
        }

        [HttpPost("patients/{patientId}/block")]
        public async Task<IActionResult> BlockPatient(int patientId)
        {
            var result = await _adminService.BlockPatientAsync(patientId);
            if (!result)
                return NotFound(new { message = "Patient not found" });

            return Ok(new { message = "Patient blocked successfully" });
        }

        [HttpPost("patients/{patientId}/unblock")]
        public async Task<IActionResult> UnblockPatient(int patientId)
        {
            var result = await _adminService.UnblockPatientAsync(patientId);
            if (!result)
                return NotFound(new { message = "Patient not found" });

            return Ok(new { message = "Patient unblocked successfully" });
        }

        [HttpPost("generate-code")]
        public async Task<IActionResult> GenerateDoctorCode([FromBody] System.Text.Json.JsonElement request)
        {
            int? adminId = null;
            int count = 1;

            try
            {
                if (request.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    if (request.TryGetProperty("adminId", out var p) && p.ValueKind == System.Text.Json.JsonValueKind.Number && p.TryGetInt32(out var aid))
                        adminId = aid;
                    if (request.TryGetProperty("count", out var c) && c.ValueKind == System.Text.Json.JsonValueKind.Number && c.TryGetInt32(out var cnt))
                        count = Math.Max(1, Math.Min(100, cnt));
                }
                else if (request.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    // Accept plain string bodies (e.g. "123") or arbitrary string; try to parse integer
                    var s = request.GetString();
                    if (int.TryParse(s, out var parsed)) adminId = parsed;
                }
            }
            catch
            {
                // ignore parsing errors and use defaults
            }

            var codes = new List<string>();
            for (int i = 0; i < count; i++)
            {
                var code = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
                var invitationCode = new Nutq.Core.Entities.InvitationCode
                {
                    Code = code,
                    Type = "Doctor",
                    AdminId = adminId,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(30),
                    Used = false
                };
                await _invitationCodeRepository.AddAsync(invitationCode);
                codes.Add(code);
            }

            return Ok(new { codes = codes });
        }
    }
}
