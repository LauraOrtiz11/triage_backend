namespace triage_backend.Dtos
{
    public class TreatmentRequestDto
    {
        public int IdHistory { get; set; }             // ID del historial médico
        public string? Description { get; set; }       // Indicaciones, procedimientos, etc.
        public List<int>? MedicationIds { get; set; }  // IDs de medicamentos (opcional)
    }
}