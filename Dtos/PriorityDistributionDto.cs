namespace triage_backend.Dtos
{
    public class PriorityDistributionDto
    {
        public required string PriorityName { get; set; }
        public int TotalPatients { get; set; }
        public double Percentage { get; set; }
    }
}
