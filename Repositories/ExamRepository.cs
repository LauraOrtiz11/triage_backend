using Microsoft.Data.SqlClient;
using triage_backend.Dtos;

namespace triage_backend.Repositories
{
    public class ExamRepository
    {
        private readonly string _connectionString = string.Empty;

        public ExamRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        public IEnumerable<ExamDto> GetAllExams()
        {
            var list = new List<ExamDto>();

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var sql = "SELECT ID_EXAMEN, NOMBRE_EXAM, DESCRIP_EXAM FROM EXAMEN;";
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new ExamDto
                        {
                            IdExam = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? null : reader.GetString(2)
                        });
                    }
                }
            }

            return list;
        }

        public ExamDto? GetExamById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var sql = "SELECT ID_EXAMEN, NOMBRE_EXAM, DESCRIP_EXAM FROM EXAMEN WHERE ID_EXAMEN = @Id;";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new ExamDto
                            {
                                IdExam = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Description = reader.IsDBNull(2) ? null : reader.GetString(2)
                            };
                        }
                    }
                }
            }

            return null;
        }
    }
}
