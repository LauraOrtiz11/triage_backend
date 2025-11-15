using System;

namespace TriageBackend.DTOs
{
    public class ConsultationReportDto
    {
        // Datos consulta
        public int ConsultationId { get; set; }            // ID_CONSULTA
        public int? HistorialId { get; set; }              // ID_HISTORIAL
        public int DoctorId { get; set; }                  // ID_MEDICO
        public int TriageId { get; set; }                  // ID_TRIAGE
        public int EstadoId { get; set; }                  // ID_ESTADO
        public DateTime FechaInicioConsulta { get; set; }  // FECHA_INICIO_CONSULTA
        public DateTime? FechaFinConsulta { get; set; }    // FECHA_FIN_CONSULTA

        // Último diagnóstico asociado (si existe)
        public int? DiagnosisId { get; set; }              // ID_DIAGNOSTICO (desde consulta_diagnostico)
        public int? DiagnosisRowId { get; set; }           // ID_CD (id del registro diagnóstico)
                                                           // Nuevos campos
        public string? DiagnosisName { get; set; }
        public string? DiagnosisObservation { get; set; }
        public string? DoctorFullName { get; set; }

        public int? TreatmentId { get; set; }
        public string? TreatmentDescription { get; set; }

        // CSV de IDs
        public string? MedicationIds { get; set; }
        public string? ExamIds { get; set; }
    }
}
