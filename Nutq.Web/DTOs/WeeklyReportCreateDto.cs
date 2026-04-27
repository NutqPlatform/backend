using System;

namespace Nutq.Web.DTOs
{
    public class WeeklyReportCreateDto
    {
        public int DoctorId { get; set; }
        public int PatientId { get; set; }
        public int? TherapyPlanId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double TotalHours { get; set; }
        public string? DoctorNotes { get; set; }
    }
}
