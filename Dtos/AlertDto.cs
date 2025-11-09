namespace triage_backend.Dtos
{
    public class AlertDto
    {
        public int IdAlert { get; set; }
        public int IdPatient { get; set; }
        public int IdNurse { get; set; }
        public DateTime AlertDate { get; set; }
        public int? IdStatus { get; set; }
        public string? StatusName { get; set; }
    }

    public class CreateAlertDto
    {
        public int IdPatient { get; set; }
        
    }

    public class AlertDetailDto
    {
        public int IdAlert { get; set; }
        public int IdPatient { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string PriorityName { get; set; } = string.Empty;
        public string PriorityColor { get; set; } = string.Empty;
        public DateTime AlertDate { get; set; } 
        public string StatusName { get; set; } = string.Empty;
        public string Symptoms { get; set; } = string.Empty;
        public DateTime? TriageDate { get; set; }
    }
}
