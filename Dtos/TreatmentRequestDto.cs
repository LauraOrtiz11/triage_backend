namespace triage_backend.Dtos
{
    public class TreatmentRequestDto
    {
        public int IdHistory { get; set; }       
        public string? Description { get; set; }    
        public List<int>? MedicationIds { get; set; }  
        public List<int>? ExamIds { get; set; }
    }
}