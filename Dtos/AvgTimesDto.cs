namespace triage_backend.Dtos
{
    public class AvgTimesDto
    {
        public required string Hour { get; set; }
        public double WaitingTime { get; set; }
        public double AttentionTime { get; set; }
    }
}
