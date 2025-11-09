using Microsoft.Data.SqlClient;
using System.Data;
using triage_backend.Dtos;
using triage_backend.Utilities;

namespace triage_backend.Repositories
{
    public class ReportRepository
    {
        private readonly ContextDB _context;

        public ReportRepository(ContextDB context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene estadísticas de tiempos promedio del triage entre dos fechas.
        /// </summary>
        public ReportDto GetTriageStats(DateTime startDate, DateTime endDate)
        {
            const string query = "EXEC SP_ReportTriageStats @StartDate, @EndDate";

            using var conn = _context.OpenConnection();
            using var cmd = new SqlCommand(query, (SqlConnection)conn);
            cmd.Parameters.AddWithValue("@StartDate", startDate);
            cmd.Parameters.AddWithValue("@EndDate", endDate);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new ReportDto
                {
                    AvgWaitTime = reader["AvgWaitTime"] == DBNull.Value ? 0 : Convert.ToDouble(reader["AvgWaitTime"]),
                    AvgAttentionTime = reader["AvgAttentionTime"] == DBNull.Value ? 0 : Convert.ToDouble(reader["AvgAttentionTime"]),
                    TotalTriageTime = reader["TotalTriageTime"] == DBNull.Value ? 0 : Convert.ToDouble(reader["TotalTriageTime"]),
                    AvgWaitTimeHHMM = reader["AvgWaitTime_HHMM"]?.ToString() ?? "00:00",
                    AvgAttentionTimeHHMM = reader["AvgAttentionTime_HHMM"]?.ToString() ?? "00:00",
                    TotalTriageTimeHHMM = reader["TotalTriageTime_HHMM"]?.ToString() ?? "00:00"
                };
            }

            return new ReportDto();
        }

    }
}
