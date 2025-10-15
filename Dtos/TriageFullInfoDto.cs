namespace triage_backend.Dtos
{
    public class TriageFullInfoDto
    {
        public class TriageDetailsDto
        {
            public int PatientId { get; set; }
            public string? PatientName { get; set; }
            public string? PatientDocument { get; set; }
            public string? Gender { get; set; }
            public DateTime? BirthDate { get; set; }
            public int TriageId { get; set; }
            public string? Symptoms { get; set; }
            public decimal? Temperature { get; set; }
            public string? BloodPressure { get; set; }
            public int? HeartRate { get; set; }
            public int? RespiratoryRate { get; set; }
            public decimal? OxygenSaturation { get; set; }
            public string? LastPriority { get; set; }
            public string? AssignedDoctor { get; set; }
            public string? TriageTime { get; set; }
        }

        public class PatientHistoryDto
        {
            public int? ConsultationId { get; set; }
            public string? StartTime { get; set; }
            public string? EndTime { get; set; }
            public int? StatusId { get; set; }
            public int? DiagnosisId { get; set; }
            public string? DiagnosisName { get; set; }
            public string? DiagnosisObservation { get; set; }
            public int? TreatmentId { get; set; }
            public string? TreatmentDescription { get; set; }
            public string? TreatmentObservation { get; set; }
            public string? TreatmentDose { get; set; }
        }
    }
}
