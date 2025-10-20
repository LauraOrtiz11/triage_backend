namespace triage_backend.Dtos
{
    public class PatientStatusDto
    {
        public string? FullName { get; set; }
        public string? PriorityLevel { get; set; }
        public string? PriorityColor { get; set; }
        public string? TurnCode { get; set; }
        public string? NurseName { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public string? Symptoms { get; set; }
        public string? BloodPressure { get; set; }
        public double? Temperature { get; set; }
        public int? HeartRate { get; set; }
        public int? OxygenSaturation { get; set; }
        public int? RespiratoryRate { get; set; }
    }
}
