namespace triage_backend.Dtos
{
    public class TreatmentRequestDto
    {
        public int IdDiagnosis { get; set; } // ID del diagnóstico
        public string? Description { get; set; } // Texto del tratamiento
        public List<int>? MedicationIds { get; set; } // IDs de medicamentos asociados
    }
}
