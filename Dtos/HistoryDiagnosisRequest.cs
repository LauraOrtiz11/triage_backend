namespace triage_backend.Dtos
{
    public class HistoryDiagnosisRequest
    {
        public int ConsultationId { get; set; }   // NUEVO: id de la consulta
        public int DiagnosisId { get; set; }
    }
}
