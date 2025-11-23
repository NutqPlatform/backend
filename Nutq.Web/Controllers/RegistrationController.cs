using Microsoft.AspNetCore.Mvc;
using Nutq.Core.Services;
using Nutq.Core.Interfaces;
using Nutq.Core.Commands;

namespace Nutq.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationController : ControllerBase
    {
        private readonly IRegistrationService _registrationService;

        public RegistrationController(IRegistrationService registrationService)
        {
            _registrationService = registrationService;
        }

        [HttpPost("doctor")]
        public async Task<IActionResult> RegisterDoctor([FromBody] DoctorRegisterCommand command)
        {
            var result = await _registrationService.RegisterDoctorAsync(command);
            return result ? Ok("Doctor registered") : BadRequest("Registration failed");
        }

        [HttpPost("patient")]
        public async Task<IActionResult> RegisterPatient([FromBody] PatientRegisterCommand command)
        {
            var result = await _registrationService.RegisterPatientAsync(command);
            return result ? Ok("Patient registered") : BadRequest("Registration failed");
        }
    }
}
