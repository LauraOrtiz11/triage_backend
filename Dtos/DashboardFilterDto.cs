namespace triage_backend.Dtos
{
    public class DashboardFilterDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Optional filters
        public int? PriorityId { get; set; }
        public int? DoctorId { get; set; }
        public int? NurseId { get; set; }
    }
}
