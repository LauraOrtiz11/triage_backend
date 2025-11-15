using System.Collections.Generic;
using TriageBackend.DTOs;

namespace TriageBackend.Utilities
{
    public interface IPdfGeneratorHistoryReport
    {
        byte[] GenerateConsultationsPdf(IEnumerable<ConsultationReportDto> consultations);
    }
}
