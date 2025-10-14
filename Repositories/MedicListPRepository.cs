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
        /// permitiendo filtrar por nombre o cédula.
        /// </summary>
        public List<MedicListPDto> GetMedicListP(MedicListFilterDto? filter = null)
        {
            var patients = new List<MedicListPDto>();

            var query = @"
                SELECT 
                    P.NOMBRE_US + ' ' + P.APELLIDO_US AS NOMBRE_COMPLETO,
                    P.CEDULA_US AS CEDULA,
                    T.SINTOMAS,
                    ISNULL(PRR.NOMBRE_PRIO, 'Sin resultado') AS PRIORIDAD,
                    ISNULL(PRR.COLOR_PRIO, 'Sin color') AS COLOR,
                    FORMAT(T.FECHA_REGISTRO, 'HH:mm:ss') AS HORA_REGISTRO,
                    CASE 
                        WHEN T.ID_MEDICO IS NULL THEN 'Sin asignar'
                        ELSE ISNULL(M.NOMBRE_US + ' ' + M.APELLIDO_US, 'Sin asignar')
                    END AS MEDICO_TRATANTE
                FROM USUARIO P
                INNER JOIN TRIAGE T 
                    ON P.ID_USUARIO = T.ID_PACIENTE
                OUTER APPLY (
                    SELECT TOP 1 PR2.NOMBRE_PRIO, PR2.COLOR_PRIO
                    FROM TRIAGE_RESULTADO TR
                    INNER JOIN PRIORIDAD PR2 ON TR.ID_PRIORIDAD = PR2.ID_PRIORIDAD
                    WHERE TR.ID_TRIAGE = T.ID_TRIAGE
                    ORDER BY TR.FECHA_REGISTRO DESC
                ) AS PRR
                LEFT JOIN USUARIO M 
                    ON T.ID_MEDICO = M.ID_USUARIO
                WHERE P.ID_ESTADO = 1
            ";

            // Agregar condiciones de filtro dinámicamente
            var parameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(filter?.FullName))
            {
                query += " AND (P.NOMBRE_US + ' ' + P.APELLIDO_US) LIKE @FullName";
                parameters.Add( new SqlParameter("@FullName", $"%{filter.FullName}%"));
            }

            if (!string.IsNullOrWhiteSpace(filter?.Identification))
            {
                query += " AND P.CEDULA_US LIKE @Identification";
                parameters.Add(new SqlParameter("@Identification", $"%{filter.Identification}%"));
            }

            // Ordenamiento por prioridad y hora
            query += @"
                ORDER BY 
                    CASE 
                        WHEN PRR.COLOR_PRIO = 'Rojo' THEN 1
                        WHEN PRR.COLOR_PRIO = 'Naranja' THEN 2
                        WHEN PRR.COLOR_PRIO = 'Amarillo' THEN 3
                        WHEN PRR.COLOR_PRIO = 'Verde' THEN 4
                        WHEN PRR.COLOR_PRIO = 'Azul' THEN 5
                        ELSE 6
                    END,
                    T.FECHA_REGISTRO DESC;";

            try
            {
                using var connection = _context.OpenConnection();
                using var command = new SqlCommand(query, (SqlConnection)connection);
                command.CommandType = CommandType.Text;

                if (parameters.Count > 0)
                {
                    command.Parameters.AddRange(parameters.ToArray());
                }

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var dto = new MedicListPDto
                    {
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
                throw new Exception("Error al obtener los pacientes desde la base de datos.", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[General Error] {ex.Message}");
                throw;
            }
            finally
            {
                _context.CloseConnection();
            }
        }
    }
}
