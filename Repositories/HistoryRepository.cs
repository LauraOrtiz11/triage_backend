using Microsoft.Data.SqlClient;
using triage_backend.Dtos;
using System.Data;

namespace triage_backend.Repositories
{
    public class HistoryRepository
    {
        private readonly string _connectionString;

        public HistoryRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        /// <summary>
        /// Obtiene el historial de un paciente a partir de su número de documento.
        /// </summary>
        public async Task<HistoryResponseDto?> GetHistoryByDocumentAsync(string documentId)
        {
            const string query = @"
                SELECT TOP 1 
                    H.ID_HISTORIAL,
                    U.ID_USUARIO,
                    U.NOMBRE_US + ' ' + U.APELLIDO_US AS PATIENT_NAME
                FROM USUARIO U
                INNER JOIN HISTORIAL H ON U.ID_USUARIO = H.ID_PACIENTE
                WHERE U.CEDULA_US = @DocumentId";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@DocumentId", documentId);
            await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new HistoryResponseDto
                {
                    HistoryId = reader.GetInt32(reader.GetOrdinal("ID_HISTORIAL")),
                    UserId = reader.GetInt32(reader.GetOrdinal("ID_USUARIO")),
                    PatientName = reader["PATIENT_NAME"]?.ToString() ?? string.Empty,
                };
            }

            return null;
        }

        /// <summary>
        /// Asocia un diagnóstico a una consulta específica (según el nuevo modelo).
        /// </summary>
        public async Task<bool> AddDiagnosisToHistoryAsync(HistoryDiagnosisRequest request)
        {
            const string insertQuery = @"
                INSERT INTO CONSULTA_DIAGNOSTICO (ID_CONSULTA, ID_DIAGNOSTICO)
                VALUES (@ConsultationId, @DiagnosisId);";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(insertQuery, connection);

            command.Parameters.AddWithValue("@ConsultationId", request.ConsultationId);
            command.Parameters.AddWithValue("@DiagnosisId", request.DiagnosisId);

            await connection.OpenAsync();
            int rows = await command.ExecuteNonQueryAsync();
            return rows > 0;
        }
    }
}
