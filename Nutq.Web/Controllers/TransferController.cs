using Microsoft.AspNetCore.Mvc;
using Nutq.Core.Interfaces;

namespace Nutq.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransferController : ControllerBase
    {
        private readonly ITransferService _transferService;

        public TransferController(ITransferService transferService)
        {
            _transferService = transferService;
        }

        [HttpPost("patient/{patientId}/leave")]
        public async Task<IActionResult> LeaveDoctor(int patientId)
        {
            try
            {
                await _transferService.LeaveDoctorAsync(patientId);
                return Ok(new { success = true, message = "Left doctor successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("doctor/{doctorId}/patients/{patientId}/release")]
        public async Task<IActionResult> ReleasePatient(int doctorId, int patientId)
        {
            try
            {
                await _transferService.ReleasePatientAsync(doctorId, patientId);
                return Ok(new { success = true, message = "Patient released successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("doctor/{doctorId}/patients/{patientId}/transfer")]
        public async Task<IActionResult> DoctorInitiateTransfer(int doctorId, int patientId, [FromBody] TransferRequestDto dto)
        {
            try
            {
                var request = await _transferService.DoctorInitiateTransferAsync(doctorId, patientId, dto.ToDoctorId, dto.Message);
                return Ok(TransferRequestMapper.Map(request));
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("patient/{patientId}/request")]
        public async Task<IActionResult> RequestTransfer(int patientId, [FromBody] TransferRequestDto dto)
        {
            try
            {
                var request = await _transferService.RequestTransferAsync(patientId, dto.ToDoctorId, dto.Message);
                return Ok(TransferRequestMapper.Map(request));
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("doctor/{doctorId}/requests/{requestId}/accept")]
        public async Task<IActionResult> AcceptRequest(int doctorId, int requestId)
        {
            try
            {
                var request = await _transferService.AcceptTransferAsync(doctorId, requestId);
                return Ok(new { success = true, request });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("doctor/{doctorId}/requests/{requestId}/reject")]
        public async Task<IActionResult> RejectRequest(int doctorId, int requestId)
        {
            try
            {
                var request = await _transferService.RejectTransferAsync(doctorId, requestId);
                return Ok(new { success = true, request });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("patient/{patientId}/requests/{requestId}")]
        public async Task<IActionResult> CancelRequest(int patientId, int requestId)
        {
            try
            {
                await _transferService.CancelTransferRequestAsync(patientId, requestId);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("doctor/{doctorId}/requests")]
        public async Task<IActionResult> GetDoctorRequests(int doctorId)
        {
            try
            {
                var requests = await _transferService.GetPendingRequestsForDoctorAsync(doctorId);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("patient/{patientId}/requests")]
        public async Task<IActionResult> GetPatientRequests(int patientId)
        {
            try
            {
                var requests = await _transferService.GetRequestsForPatientAsync(patientId);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("doctor/{doctorId}/former-patients")]
        public async Task<IActionResult> GetFormerPatients(int doctorId)
        {
            try
            {
                var patients = await _transferService.GetFormerPatientsAsync(doctorId);
                return Ok(patients);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    public class TransferRequestDto
    {
        public int ToDoctorId { get; set; }
        public string? Message { get; set; }
    }

    internal static class TransferRequestMapper
    {
        public static object Map(Nutq.Core.Entities.TransferRequest r) => new
        {
            r.Id,
            patientId = r.PatientId,
            fromDoctorId = r.FromDoctorId,
            toDoctorId = r.ToDoctorId,
            r.Status,
            r.Message,
            r.CreatedAt,
            r.RespondedAt
        };
    }
}
