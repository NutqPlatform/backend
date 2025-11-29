using Microsoft.AspNetCore.Mvc;
using Nutq.Core.Interfaces;

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



    }
}
