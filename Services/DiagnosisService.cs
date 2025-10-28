using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using triage_backend.Dtos;

namespace triage_backend.Services
{
    public interface IDiagnosisService
    {
        Task<DiagnosisDto?> GetDiagnosisByIdAsync(int id);
        Task<List<DiagnosisDto>> GetAllDiagnosesAsync();
    }

    public class DiagnosisService : IDiagnosisService
    {
        private readonly string _connectionString;

        public DiagnosisService(IConfiguration config)
        {
            // ✅ Garantiza que nunca sea null
            _connectionString = config.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        public async Task<DiagnosisDto?> GetDiagnosisByIdAsync(int id)
        {
            if (id <= 0) return null;

            using var connection = new SqlConnection(_connectionString);
            const string query = @"
                SELECT ID_DIAGNOSTICO, NOMBRE_DIAG, OBSERV_DIAG
                FROM DIAGNOSTICO
                WHERE ID_DIAGNOSTICO = @DiagnosisId";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@DiagnosisId", id);

            await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new DiagnosisDto
                {
                    DiagnosisId = reader.GetInt32(reader.GetOrdinal("ID_DIAGNOSTICO")),
                    DiagnosisName = reader["NOMBRE_DIAG"]?.ToString() ?? string.Empty,
                    DiagnosisNotes = reader["OBSERV_DIAG"]?.ToString() ?? string.Empty
                };
            }

            return null;
        }

        public async Task<List<DiagnosisDto>> GetAllDiagnosesAsync()
        {
            var diagnoses = new List<DiagnosisDto>();

            using var connection = new SqlConnection(_connectionString);
            const string query = @"SELECT ID_DIAGNOSTICO, NOMBRE_DIAG, OBSERV_DIAG FROM DIAGNOSTICO";

            using var command = new SqlCommand(query, connection);
            await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                diagnoses.Add(new DiagnosisDto
                {
                    DiagnosisId = reader.GetInt32(reader.GetOrdinal("ID_DIAGNOSTICO")),
                    DiagnosisName = reader["NOMBRE_DIAG"]?.ToString() ?? string.Empty,
                    DiagnosisNotes = reader["OBSERV_DIAG"]?.ToString() ?? string.Empty
                });
            }

            return diagnoses;
        }
    }
}
