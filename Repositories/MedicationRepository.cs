using Microsoft.Data.SqlClient;
using triage_backend.Dtos;

namespace triage_backend.Repositories
{
    public class MedicationRepository
    {
        private readonly string _connectionString = string.Empty;

        public MedicationRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        public IEnumerable<MedicationDto> GetAllMedications()
        {
            var list = new List<MedicationDto>();

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var sql = "SELECT ID_MEDICAMENTO, NOMBRE_MEDICA, DESCRIP_MEDICA, PROVEEDOR_MEDICA FROM MEDICAMENTO;";
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new MedicationDto
                        {
                            IdMedication = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                            Provider = reader.IsDBNull(3) ? null : reader.GetString(3)
                        });
                    }
                }
            }

            return list;
        }

        public MedicationDto? GetMedicationById(int id)
        {
            MedicationDto? result = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var sql = "SELECT ID_MEDICAMENTO, NOMBRE_MEDICA, DESCRIP_MEDICA, PROVEEDOR_MEDICA FROM MEDICAMENTO WHERE ID_MEDICAMENTO = @Id;";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result = new MedicationDto
                            {
                                IdMedication = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                                Provider = reader.IsDBNull(3) ? null : reader.GetString(3)
                            };
                        }
                    }
                }
            }

            return result;
        }
    }
}
