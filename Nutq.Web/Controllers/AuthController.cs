using Microsoft.AspNetCore.Mvc;
using Nutq.Core.Interfaces;
using Nutq.Core.Auth;
using Nutq.Web.DTOs;

namespace Nutq.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "v1")] // مهم عشان Swagger يشوفها
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login/doctor")]
        [ApiExplorerSettings(GroupName = "v1")]
        public async Task<IActionResult> LoginDoctor([FromBody] LoginRequestDto dto)
        {
            var result = await _authService.LoginDoctorAsync(dto.Email, dto.Password);
            if (result == null) return Unauthorized(new { message = "Invalid credentials" });

            return Ok(result);
        }

        [HttpPost("login/patient")]
        [ApiExplorerSettings(GroupName = "v1")]
        public async Task<IActionResult> LoginPatient([FromBody] LoginRequestDto dto)
        {
            var result = await _authService.LoginPatientAsync(dto.Email, dto.Password);
            if (result == null) return Unauthorized(new { message = "Invalid credentials" });

            return Ok(result);
        }
    }
}
