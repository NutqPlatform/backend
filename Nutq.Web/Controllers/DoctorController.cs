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

        /// <summary>
        /// Generate invitation code for a patient (doctor creates it)
        /// </summary>
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
public async Task<IActionResult> GetDoctorPatients(int doctorId)
{
    try
    {
        var patients = await _doctorService.GetDoctorPatientsAsync(doctorId);

        return Ok(new
        {
            success = true,
            patients = patients
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

    }
}
