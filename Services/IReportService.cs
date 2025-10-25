using triage_backend.Dtos;

namespace triage_backend.Services
{
    public interface IReportService
    {
        byte[] GenerateTriageReport(string userName, DateTime startDate, DateTime endDate);
        string GetReportFileName(string userName);
    }
}
