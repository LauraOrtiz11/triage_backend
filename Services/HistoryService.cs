using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using triage_backend.Dtos;

namespace triage_backend.Services
{
    public interface IHistoryService
    {
        Task<HistoryResponseDto?> GetHistoryByDocumentAsync(string documentId);
        Task<bool> AddDiagnosisToHistoryAsync(HistoryDiagnosisRequest request);
    }

    public class HistoryService : IHistoryService
    {
        private readonly IConfiguration _config;

        public HistoryService(IConfiguration config)
        {
            _config = config;
        }

        // 🔹 Get History by Patient Document
        public async Task<HistoryResponseDto?> GetHistoryByDocumentAsync(string documentId)
        {
            string connectionString = _config.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT TOP 1 
                        H.ID_HISTORIAL,
                        U.ID_USUARIO,
                        U.NOMBRE_US + ' ' + U.APELLIDO_US AS PATIENT_NAME
                    FROM USUARIO U
                    INNER JOIN HISTORIAL H ON U.ID_USUARIO = H.ID_PACIENTE
                    WHERE U.CEDULA_US = @DocumentId";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@DocumentId", documentId);

                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new HistoryResponseDto
                            {
                                HistoryId = reader.GetInt32(reader.GetOrdinal("ID_HISTORIAL")),
                                UserId = reader.GetInt32(reader.GetOrdinal("ID_USUARIO")),
                                PatientName = reader["PATIENT_NAME"].ToString()
                            };
                        }
                    }
                }
            }

            return null;
        }

        // 🔹 Add Diagnosis to a History
        public async Task<bool> AddDiagnosisToHistoryAsync(HistoryDiagnosisRequest request)
        {
            string connectionString = _config.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string checkQuery = @"SELECT COUNT(*) 
                                      FROM HISTORIAL_DIAGNOSTICO 
                                      WHERE ID_HISTORIAL = @HistoryId 
                                      AND ID_DIAGNOSTICO = @DiagnosisId";

                string insertQuery = @"INSERT INTO HISTORIAL_DIAGNOSTICO (ID_HISTORIAL, ID_DIAGNOSTICO)
                                       VALUES (@HistoryId, @DiagnosisId)";

                await connection.OpenAsync();

                // Check for duplicates first
                using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@HistoryId", request.HistoryId);
                    checkCommand.Parameters.AddWithValue("@DiagnosisId", request.DiagnosisId);

                    int exists = (int)await checkCommand.ExecuteScalarAsync();
                    if (exists > 0)
                        return false; // Already registered
                }

                // Insert if not duplicate
                using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                {
                    insertCommand.Parameters.AddWithValue("@HistoryId", request.HistoryId);
                    insertCommand.Parameters.AddWithValue("@DiagnosisId", request.DiagnosisId);

                    int rowsAffected = await insertCommand.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
    }
}
