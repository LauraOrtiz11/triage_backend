using Microsoft.Data.SqlClient;
using System.Data;
using triage_backend.Dtos;
using triage_backend.Utilities;

namespace triage_backend.Repositories
{
    public class MedicListPRepository
    {
        private readonly ContextDB _context;

        public MedicListPRepository(ContextDB context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Obtiene la lista de pacientes activos con su nivel de prioridad, hora de llegada y médico tratante,
        /// permitiendo filtrar por nombre o cédula, solo mostrando triajes activos y que no estén en estado 2.
        /// </summary>
        public List<MedicListPDto> GetMedicListP(MedicListFilterDto? filter = null)
        {
            var patients = new List<MedicListPDto>();

            try
            {
                using var connection = _context.OpenConnection();
                using var command = new SqlCommand("SP_GetMedicListP", (SqlConnection)connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                // Parametros (sin cambiar nombres)
                command.Parameters.AddWithValue("@FullName", (object?)filter?.FullName ?? DBNull.Value);
                command.Parameters.AddWithValue("@Identification", (object?)filter?.Identification ?? DBNull.Value);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var dto = new MedicListPDto
                    {
                        TriageId = reader["ID_TRIAGE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ID_TRIAGE"]),
                        FullName = reader["NOMBRE_COMPLETO"]?.ToString() ?? "Sin nombre",
                        Identification = reader["CEDULA"]?.ToString() ?? "Sin documento",
                        Symptoms = reader["SINTOMAS"]?.ToString() ?? "No especificados",
                        PriorityLevel = reader["PRIORIDAD"]?.ToString() ?? "No asignada",
                        Color = reader["COLOR"]?.ToString() ?? "Sin color",
                        ArrivalHour = reader["HORA_REGISTRO"]?.ToString() ?? "--:--:--",
                        MedicName = reader["MEDICO_TRATANTE"]?.ToString() ?? "Sin asignar"
                    };
                    patients.Add(dto);
                }

                return patients;
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"[SQL Error] {ex.Message}");
                throw new Exception("Error al obtener los pacientes activos desde la base de datos.", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[General Error] {ex.Message}");
                throw new Exception("Error inesperado al consultar la lista de pacientes.", ex);
            }
            finally
            {
                _context.CloseConnection();
            }
        }

    }
}
