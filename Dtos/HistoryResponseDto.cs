namespace triage_backend.Dtos
{
    public class HistoryResponseDto
    {
        public int HistoryId { get; set; }
        public int UserId { get; set; }
        public string? PatientName { get; set; }
    }
}
