namespace triage_backend.Dtos
{
    public class MedicListPDto
    {
        public int TriageId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Identification { get; set; } = string.Empty;
        public string Symptoms { get; set; } = string.Empty;
        public string PriorityLevel { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string ArrivalHour { get; set; } = string.Empty;
        public string MedicName { get; set; } = string.Empty;
    }

    public class MedicListFilterDto
    {
        public string? FullName { get; set; }
        public string? Identification { get; set; }
    }
}
