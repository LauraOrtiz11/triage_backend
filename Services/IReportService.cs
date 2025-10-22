using System;

namespace triage_backend.Services
{
    public interface IReportService
    {
       
        byte[] GenerateTriageReport(string generatedBy);
        string GetReportFileName(string generatedBy);
    }
}
