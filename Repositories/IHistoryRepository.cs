using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TriageBackend.DTOs;

namespace TriageBackend.Repositories
{
    public interface IHistoryRepository
    {
        Task<(IEnumerable<ConsultationReportDto> items, long total)> GetPatientHistoryAsync(
            int patientId, DateTime? from, DateTime? to, int page, int limit);

        Task<ConsultationReportDto?> GetConsultationDetailAsync(int patientId, int consultaId);

        Task<IEnumerable<ConsultationReportDto>> GetPatientHistoryForPdfAsync(
            int patientId, DateTime? from, DateTime? to, int maxRows = 1000);

        Task<int?> GetHistorialIdByUserIdAsync(int userId);
    }
}
