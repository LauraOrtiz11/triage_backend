using Microsoft.Data.SqlClient;
using System.Data;
using triage_backend.Utilities;
using triage_backend.Dtos;

namespace triage_backend.Repositories
{
    public class TriageResultRepository
    {
        private readonly ContextDB _context;

        public TriageResultRepository(ContextDB context)
        {
            _context = context;
        }

        // === Guarda la decisión del triage ===
        public bool SaveTriageResult(TriageResultDto result)
        {
            using (var connection = _context.OpenConnection())
            {
                const string query = @"
                    INSERT INTO TRIAGE_RESULTADO (ID_Triage, ID_Prioridad, ID_Usuario, Es_Prioridad_Final)
                    VALUES (@TriageId, @PriorityId, @UserId, @IsFinalPriority);
                ";

                using (var command = new SqlCommand(query, (SqlConnection)connection))
                {
                    command.Parameters.AddWithValue("@TriageId", result.TriageId);
                    command.Parameters.AddWithValue("@PriorityId", result.PriorityId);
                    command.Parameters.AddWithValue("@UserId", result.NurseId);
                    command.Parameters.AddWithValue("@IsFinalPriority", result.IsFinalPriority);

                    int rows = command.ExecuteNonQuery();
                    return rows > 0;
                }
            }
        }

        // === Trae información del paciente y su triage ===
        public async Task<List<TriageResultPatientInfoDto>> GetPatientTriageInfoAsync(int triageId)
        {
            var results = new List<TriageResultPatientInfoDto>();

            const string query = @"
        SELECT TOP 1
            (U.Nombre_Us + ' ' + U.Apellido_Us) AS FullName,
            DATEDIFF(YEAR, U.Fecha_Nac_Us, GETDATE()) AS Age,
            CASE 
                WHEN U.Sexo_Us = 1 THEN 'Masculino'
                WHEN U.Sexo_Us = 0 THEN 'Femenino'
                ELSE 'No especificado'
            END AS Gender,
            T.Sintomas AS Symptoms,
            CONCAT(
                'Temp: ', CAST(T.Temperatura AS VARCHAR(10)), '°C, ',
                'FC: ', CAST(T.Frecuencia_Card AS VARCHAR(10)), ' bpm, ',
                'PA: ', T.Presion_Arterial, ', ',
                'FR: ', CAST(T.Frecuencia_Res AS VARCHAR(10)), ' rpm, ',
                'O₂: ', CAST(T.Oxigenacion AS VARCHAR(10)), '%'
            ) AS VitalSigns,
            P.Nombre_Prio AS PriorityName
        FROM TRIAGE AS T
            INNER JOIN USUARIO AS U ON U.ID_Usuario = T.ID_Paciente
            INNER JOIN TRIAGE_RESULTADO AS TR ON TR.ID_Triage = T.ID_Triage
            INNER JOIN PRIORIDAD AS P ON P.ID_Prioridad = TR.ID_Prioridad
        WHERE 
            T.ID_Triage = @TriageId
        ORDER BY 
            TR.ID_Triage_Resultado DESC;";

            using (var connection = _context.OpenConnection())
            using (var command = new SqlCommand(query, (SqlConnection)connection))
            {
                command.Parameters.AddWithValue("TriageId", triageId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        results.Add(new TriageResultPatientInfoDto
                        {
                            FullName = reader["FullName"].ToString() ?? "",
                            Age = Convert.ToInt32(reader["Age"]),
                            Gender = reader["Gender"].ToString() ?? "",
                            Symptoms = reader["Symptoms"].ToString() ?? "",
                            VitalSigns = reader["VitalSigns"].ToString() ?? "",
                            PriorityName = reader["PriorityName"].ToString() ?? ""
                        });
                    }
                }
            }

            
            _context.CloseConnection();

            return results;
        }
        // Trae el nombre y la descripcion de la prioridad segun el ID del triage
        public async Task<TriagePriorityInfoDto?> GetPriorityInfoByTriageIdAsync(int triageId)
        {
            TriagePriorityInfoDto? result = null;

            const string query = @"
        SELECT 
            P.Nombre_Prio AS PriorityName,
            P.Descrip_Prio AS PriorityDescription
        FROM TRIAGE AS T
            INNER JOIN PRIORIDAD AS P ON P.ID_Prioridad = T.ID_Prioridad
        WHERE 
            T.ID_Triage = @TriageId;";

            using (var connection = _context.OpenConnection())
            using (var command = new SqlCommand(query, (SqlConnection)connection))
            {
                command.Parameters.AddWithValue("@TriageId", triageId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        result = new TriagePriorityInfoDto
                        {
                            PriorityName = reader["PriorityName"].ToString() ?? "",
                            PriorityDescription = reader["PriorityDescription"].ToString() ?? ""
                        };
                    }
                }
            }

            _context.CloseConnection();
            return result;
        }


        // === Trae todas las prioridades disponibles ===
        public async Task<List<PriorityInfoDto>> GetAllPrioritiesAsync()
        {
            var priorities = new List<PriorityInfoDto>();

            const string query = @"
        SELECT 
            ID_Prioridad AS PriorityId,
            Nombre_Prio AS PriorityName,
            Descrip_Prio AS PriorityDescription
        FROM PRIORIDAD
        ORDER BY ID_Prioridad ASC;";

            using (var connection = _context.OpenConnection())
            using (var command = new SqlCommand(query, (SqlConnection)connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    priorities.Add(new PriorityInfoDto
                    {
                        PriorityId = Convert.ToInt32(reader["PriorityId"]),
                        PriorityName = reader["PriorityName"].ToString() ?? "",
                        PriorityDescription = reader["PriorityDescription"].ToString() ?? ""
                    });
                }
            }

            _context.CloseConnection();
            return priorities;
        }

    }
}
