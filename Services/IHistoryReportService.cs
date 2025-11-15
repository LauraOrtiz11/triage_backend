using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TriageBackend.DTOs;

namespace TriageBackend.Services
{
    public interface IHistoryReportService
    {
        Task<(IEnumerable<ConsultationReportDto> items, long total)> GetPatientHistoryAsync(int patientId, DateTime? from, DateTime? to, int page, int limit);
        Task<ConsultationReportDto?> GetConsultationDetailAsync(int patientId, int consultaId);
        Task<byte[]> GeneratePatientHistoryPdfAsync(int patientId, DateTime? from, DateTime? to);

        Task<int?> GetHistorialIdByUserIdAsync(int userId);

    }
}
