namespace triage_backend.Dtos
{
    public class DiagnosisDto
    {
        public int DiagnosisId { get; set; }
        public string DiagnosisName { get; set; } = string.Empty;
        public string DiagnosisNotes { get; set; } = string.Empty;
    }
}
 