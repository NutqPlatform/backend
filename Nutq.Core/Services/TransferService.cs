using Nutq.Core.Entities;
using Nutq.Core.Interfaces;

namespace Nutq.Core.Services
{
    public class TransferService : ITransferService
    {
        private readonly IPatientRepository _patientRepo;
        private readonly IDoctorRepository _doctorRepo;
        private readonly IDoctorPatientRelationshipRepository _relationshipRepo;
        private readonly ITransferRequestRepository _transferRepo;
        private readonly ITherapyPlanRepository _planRepo;

        public TransferService(
            IPatientRepository patientRepo,
            IDoctorRepository doctorRepo,
            IDoctorPatientRelationshipRepository relationshipRepo,
            ITransferRequestRepository transferRepo,
            ITherapyPlanRepository planRepo)
        {
            _patientRepo = patientRepo;
            _doctorRepo = doctorRepo;
            _relationshipRepo = relationshipRepo;
            _transferRepo = transferRepo;
            _planRepo = planRepo;
        }

        public Task LeaveDoctorAsync(int patientId) =>
            EndPatientDoctorRelationshipAsync(patientId, null);

        public async Task ReleasePatientAsync(int doctorId, int patientId)
        {
            var patient = await _patientRepo.GetByIdAsync(patientId);
            if (patient == null) throw new Exception("Patient not found");
            if (patient.DoctorId != doctorId)
                throw new Exception("Patient does not belong to this doctor");

            await EndPatientDoctorRelationshipAsync(patientId, doctorId);
        }

        public async Task<TransferRequest> RequestTransferAsync(int patientId, int toDoctorId, string? message)
        {
            var patient = await _patientRepo.GetByIdAsync(patientId);
            if (patient == null) throw new Exception("Patient not found");
            if (patient.DoctorId.HasValue)
                throw new Exception("Leave your current doctor before requesting a new one");

            return await CreateTransferRequestAsync(patientId, toDoctorId, message, null);
        }

        public async Task<TransferRequest> DoctorInitiateTransferAsync(int doctorId, int patientId, int toDoctorId, string? message)
        {
            var patient = await _patientRepo.GetByIdAsync(patientId);
            if (patient == null) throw new Exception("Patient not found");
            if (patient.DoctorId != doctorId)
                throw new Exception("Patient does not belong to this doctor");
            if (toDoctorId == doctorId)
                throw new Exception("Cannot transfer patient to the same doctor");

            await EndPatientDoctorRelationshipAsync(patientId, doctorId);
            return await CreateTransferRequestAsync(patientId, toDoctorId, message, doctorId);
        }

        public async Task<TransferRequest> AcceptTransferAsync(int doctorId, int requestId)
        {
            var request = await _transferRepo.GetByIdWithDetailsAsync(requestId);
            if (request == null) throw new Exception("Transfer request not found");
            if (request.ToDoctorId != doctorId)
                throw new Exception("This request is not for you");
            if (request.Status != "Pending")
                throw new Exception("Request is no longer pending");

            var patient = await _patientRepo.GetByIdAsync(request.PatientId);
            if (patient == null) throw new Exception("Patient not found");
            if (patient.DoctorId.HasValue)
                throw new Exception("Patient already has an assigned doctor");

            patient.DoctorId = doctorId;
            await _patientRepo.UpdateAsync(patient);

            await _relationshipRepo.AddAsync(new DoctorPatientRelationship
            {
                DoctorId = doctorId,
                PatientId = patient.Id,
                AssignedAt = DateTime.UtcNow
            });

            request.Status = "Accepted";
            request.RespondedAt = DateTime.UtcNow;
            await _transferRepo.UpdateAsync(request);

            await CancelOtherPendingRequestsAsync(patient.Id, request.Id);
            return request;
        }

        public async Task<TransferRequest> RejectTransferAsync(int doctorId, int requestId)
        {
            var request = await _transferRepo.GetByIdWithDetailsAsync(requestId);
            if (request == null) throw new Exception("Transfer request not found");
            if (request.ToDoctorId != doctorId)
                throw new Exception("This request is not for you");
            if (request.Status != "Pending")
                throw new Exception("Request is no longer pending");

            request.Status = "Rejected";
            request.RespondedAt = DateTime.UtcNow;
            await _transferRepo.UpdateAsync(request);
            return request;
        }

        public async Task CancelTransferRequestAsync(int patientId, int requestId)
        {
            var request = await _transferRepo.GetByIdAsync(requestId);
            if (request == null) throw new Exception("Transfer request not found");
            if (request.PatientId != patientId)
                throw new Exception("Not your transfer request");
            if (request.Status != "Pending")
                throw new Exception("Only pending requests can be cancelled");

            request.Status = "Cancelled";
            request.RespondedAt = DateTime.UtcNow;
            await _transferRepo.UpdateAsync(request);
        }

        public async Task<IEnumerable<object>> GetPendingRequestsForDoctorAsync(int doctorId)
        {
            var requests = await _transferRepo.GetPendingByDoctorIdAsync(doctorId);
            return requests.Select(MapRequestDto);
        }

        public async Task<IEnumerable<object>> GetRequestsForPatientAsync(int patientId)
        {
            var requests = await _transferRepo.GetByPatientIdAsync(patientId);
            return requests.Select(MapRequestDto);
        }

        public async Task<IEnumerable<object>> GetFormerPatientsAsync(int doctorId)
        {
            var relationships = await _relationshipRepo.GetFormerByDoctorIdAsync(doctorId);
            return relationships.Select(r => new
            {
                patientId = r.PatientId,
                name = r.Patient.Name,
                email = r.Patient.Email,
                profilePicture = r.Patient.ProfilePicture,
                assignedAt = r.AssignedAt,
                leftAt = r.EndedAt,
                diagnosis = r.DiagnosisTextSnapshot,
                diagnosisFileUrl = r.DiagnosisFileUrlSnapshot
            });
        }

        private async Task<TransferRequest> CreateTransferRequestAsync(int patientId, int toDoctorId, string? message, int? initiatedByDoctorId)
        {
            var toDoctor = await _doctorRepo.GetByIdAsync(toDoctorId);
            if (toDoctor == null) throw new Exception("Target doctor not found");

            var existing = await _transferRepo.GetPendingForPatientAndDoctorAsync(patientId, toDoctorId);
            if (existing != null)
                throw new Exception("A pending request to this doctor already exists");

            int? fromDoctorId = initiatedByDoctorId;
            if (!fromDoctorId.HasValue)
            {
                var lastEnded = await _relationshipRepo.GetLastEndedForPatientAsync(patientId);
                fromDoctorId = lastEnded?.DoctorId;
            }

            var request = new TransferRequest
            {
                PatientId = patientId,
                FromDoctorId = fromDoctorId,
                ToDoctorId = toDoctorId,
                Status = "Pending",
                Message = message,
                InitiatedByDoctorId = initiatedByDoctorId,
                CreatedAt = DateTime.UtcNow
            };

            await _transferRepo.AddAsync(request);
            return request;
        }

        private async Task EndPatientDoctorRelationshipAsync(int patientId, int? expectedDoctorId)
        {
            var patient = await _patientRepo.GetByIdAsync(patientId);
            if (patient == null) throw new Exception("Patient not found");

            if (!patient.DoctorId.HasValue)
                throw new Exception("Patient has no assigned doctor");

            var doctorId = patient.DoctorId.Value;
            if (expectedDoctorId.HasValue && doctorId != expectedDoctorId.Value)
                throw new Exception("Patient does not belong to this doctor");

            var relationship = await _relationshipRepo.GetActiveAsync(doctorId, patientId);
            if (relationship != null)
            {
                relationship.EndedAt = DateTime.UtcNow;
                relationship.DiagnosisTextSnapshot = patient.DiagnosisText;
                relationship.DiagnosisFileUrlSnapshot = patient.DiagnosisFileUrl;
                await _relationshipRepo.UpdateAsync(relationship);
            }

            await ArchivePlansAsync(doctorId, patientId);

            patient.DoctorId = null;
            await _patientRepo.UpdateAsync(patient);
        }

        private async Task ArchivePlansAsync(int doctorId, int patientId)
        {
            var plans = await _planRepo.GetByDoctorAndPatientAsync(doctorId, patientId);
            foreach (var plan in plans.Where(p => !p.IsArchived))
            {
                if (plan.Status == "Active")
                    plan.Status = "Paused";
                plan.IsArchived = true;
                await _planRepo.UpdateAsync(plan);
            }
        }

        private async Task CancelOtherPendingRequestsAsync(int patientId, int exceptRequestId)
        {
            var requests = await _transferRepo.GetByPatientIdAsync(patientId);
            foreach (var req in requests.Where(r => r.Id != exceptRequestId && r.Status == "Pending"))
            {
                req.Status = "Cancelled";
                req.RespondedAt = DateTime.UtcNow;
                await _transferRepo.UpdateAsync(req);
            }
        }

        private static object MapRequestDto(TransferRequest r) => new
        {
            r.Id,
            patientId = r.PatientId,
            patientName = r.Patient?.Name,
            fromDoctorId = r.FromDoctorId,
            fromDoctorName = r.FromDoctor?.Name,
            toDoctorId = r.ToDoctorId,
            toDoctorName = r.ToDoctor?.Name,
            r.Status,
            r.Message,
            r.CreatedAt,
            r.RespondedAt
        };
    }
}
