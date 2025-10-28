namespace triage_backend.Dtos
{
    public class TreatmentRequestDto
    {
        public string? Description { get; set; }
        public int ConsultationId { get; set; }          
        public List<int>? MedicationIds { get; set; }
        public List<int>? ExamIds { get; set; }
    }
}