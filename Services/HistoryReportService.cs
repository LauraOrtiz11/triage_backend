using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TriageBackend.DTOs;
using TriageBackend.Repositories;
using TriageBackend.Utilities;

namespace TriageBackend.Services
{
    public class HistoryReportService : IHistoryReportService
    {
        private readonly IHistoryRepository _repo;
        private readonly IPdfGeneratorHistoryReport _pdfGenerator;

        public HistoryReportService(IHistoryRepository repo, IPdfGeneratorHistoryReport pdfGenerator)
        {
            _repo = repo;
            _pdfGenerator = pdfGenerator;
        }

        public async Task<(IEnumerable<ConsultationReportDto> items, long total)> GetPatientHistoryAsync(int patientId, DateTime? from, DateTime? to, int page, int limit)
        {
            return await _repo.GetPatientHistoryAsync(patientId, from, to, page, limit);
        }

        public async Task<ConsultationReportDto?> GetConsultationDetailAsync(int patientId, int consultaId)
        {
            
            return await _repo.GetConsultationDetailAsync(patientId, consultaId);
        }

        public async Task<byte[]> GeneratePatientHistoryPdfAsync(int patientId, DateTime? from, DateTime? to)
        {
            var list = (await _repo.GetPatientHistoryForPdfAsync(patientId, from, to, 2000)).ToList();
            return _pdfGenerator.GenerateConsultationsPdf(list);
        }

        public async Task<int?> GetHistorialIdByUserIdAsync(int userId)
        {
            
            return await _repo.GetHistorialIdByUserIdAsync(userId);
        }
    }

}
