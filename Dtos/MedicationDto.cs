namespace triage_backend.Dtos
{
    public class MedicationDto
    {
        public int IdMedication { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Provider { get; set; }
    }
}
