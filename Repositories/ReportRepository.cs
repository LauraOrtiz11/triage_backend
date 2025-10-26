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
            const string query = @"
                SELECT
                    AVG(DATEDIFF(MINUTE, T.FECHA_REGISTRO, C.FECHA_INICIO_CONSULTA)) AS AvgWaitTime,
                    AVG(DATEDIFF(MINUTE, C.FECHA_INICIO_CONSULTA, C.FECHA_FIN_CONSULTA)) AS AvgAttentionTime,
                    AVG(DATEDIFF(MINUTE, T.FECHA_REGISTRO, C.FECHA_FIN_CONSULTA)) AS TotalTriageTime
                FROM TRIAGE T
                INNER JOIN CONSULTA C ON C.ID_TRIAGE = T.ID_TRIAGE
                WHERE T.FECHA_REGISTRO BETWEEN @StartDate AND @EndDate
                    AND T.ID_ESTADO = 1
                    AND C.ID_ESTADO = 1;";

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
                    TotalTriageTime = reader["TotalTriageTime"] == DBNull.Value ? 0 : Convert.ToDouble(reader["TotalTriageTime"])
                };
            }

            return new ReportDto
            {
                AvgWaitTime = 0,
                AvgAttentionTime = 0,
                TotalTriageTime = 0
            };
        }
    }
}
