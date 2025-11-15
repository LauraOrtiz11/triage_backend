using System;

namespace triage_backend.Dtos
{
    public class ReportDto
    {
        public string GeneratedBy { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
        public string RangeDescription { get; set; } = string.Empty;

        public double AvgWaitTime { get; set; }       // Tiempo promedio de espera (minutos)
        public double AvgAttentionTime { get; set; }  // Tiempo promedio de atención (minutos)
        public double TotalTriageTime { get; set; }   // Tiempo total del proceso (minutos)
        public string AvgWaitTimeHHMM { get; set; } =string.Empty;
        public string AvgAttentionTimeHHMM { get; set; } = string.Empty;
        public string TotalTriageTimeHHMM { get; set; } = string.Empty;

    }
}
