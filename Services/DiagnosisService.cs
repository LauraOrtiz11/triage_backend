
using Microsoft.Extensions.Configuration;
using System.Data;
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
        private readonly IConfiguration _config;

        public DiagnosisService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<DiagnosisDto?> GetDiagnosisByIdAsync(int id)
        {
            string connectionString = _config.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"SELECT ID_DIAGNOSTICO, NOMBRE_DIAG, OBSERV_DIAG
                                 FROM DIAGNOSTICO
                                 WHERE ID_DIAGNOSTICO = @DiagnosisId";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@DiagnosisId", id);

                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new DiagnosisDto
                            {
                                DiagnosisId = reader.GetInt32(reader.GetOrdinal("ID_DIAGNOSTICO")),
                                DiagnosisName = reader["NOMBRE_DIAG"].ToString(),
                                DiagnosisNotes = reader["OBSERV_DIAG"].ToString()
                            };
                        }
                    }
                }
            }

            return null;
        }

        public async Task<List<DiagnosisDto>> GetAllDiagnosesAsync()
        {
            var diagnoses = new List<DiagnosisDto>();
            string connectionString = _config.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"SELECT ID_DIAGNOSTICO, NOMBRE_DIAG, OBSERV_DIAG FROM DIAGNOSTICO";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            diagnoses.Add(new DiagnosisDto
                            {
                                DiagnosisId = reader.GetInt32(reader.GetOrdinal("ID_DIAGNOSTICO")),
                                DiagnosisName = reader["NOMBRE_DIAG"].ToString(),
                                DiagnosisNotes = reader["OBSERV_DIAG"].ToString()
                            });
                        }
                    }
                }
            }

            return diagnoses;
        }
    }
}
