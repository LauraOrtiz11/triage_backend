namespace triage_backend.Dtos
{
    public class DiagnosisFrequencyDto
    {
        public required string DiagnosisName { get; set; }
        public int TotalOccurrences { get; set; }
        public double Percentage { get; set; }
    }
}
