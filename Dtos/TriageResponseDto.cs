namespace triage_backend.Dtos
{
    public class TriageResponseDto
    {
        public string SuggestedLevel { get; set; } = string.Empty;  // blue, green, yellow, orange, red
        public decimal Confidence { get; set; }
        public string Message { get; set; } = string.Empty;         // explicación para el enfermero
    }
}
