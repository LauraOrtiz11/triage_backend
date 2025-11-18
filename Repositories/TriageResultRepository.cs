using Microsoft.Data.SqlClient;
using System.Data;
using triage_backend.Utilities;
using triage_backend.Dtos;
using triage_backend.Services;
using System.Net.Mail;

namespace triage_backend.Repositories
{
    public class TriageResultRepository
    {
        private readonly ContextDB _context;
        private readonly EmailBackgroundService _emailService;

        public TriageResultRepository(ContextDB context, EmailBackgroundService emailService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        // Guarda el resultado, recalcula turno y notifica
        public bool SaveTriageResult(TriageResultDto result)
        {
            using var conn = (SqlConnection)_context.OpenConnection();
            using var tx = conn.BeginTransaction();

            try
            {
                // 1) Insertar nuevo resultado
                const string insertSql = @"
INSERT INTO TRIAGE_RESULTADO 
(ID_Triage, ID_Prioridad, ID_Usuario, Es_Prioridad_Final, Fecha_Registro)
VALUES (@TriageId, @PriorityId, @UserId, @IsFinalPriority, GETDATE());";

                using (var cmd = new SqlCommand(insertSql, conn, tx))
                {
                    cmd.Parameters.AddWithValue("@TriageId", result.TriageId);
                    cmd.Parameters.AddWithValue("@PriorityId", result.PriorityId);
                    cmd.Parameters.AddWithValue("@UserId", result.NurseId);
                    cmd.Parameters.AddWithValue("@IsFinalPriority", result.IsFinalPriority);
                    cmd.ExecuteNonQuery();
                }

                // 2) Obtener info necesaria para turno y correo
                string color = "X";
                string priorityName = "Sin prioridad";
                string patientEmail = "";
                string patientName = "Paciente";

                const string infoSql = @"
SELECT
    P.COLOR_PRIO     AS Color,
    P.NOMBRE_PRIO    AS PriorityName,
    U.CORREO_US      AS Email,
    (U.NOMBRE_US + ' ' + U.APELLIDO_US) AS FullName
FROM TRIAGE T
JOIN USUARIO U  ON U.ID_USUARIO   = T.ID_PACIENTE
JOIN PRIORIDAD P ON P.ID_PRIORIDAD = @PriorityId
WHERE T.ID_TRIAGE = @TriageId;";

                using (var cmd = new SqlCommand(infoSql, conn, tx))
                {
                    cmd.Parameters.AddWithValue("@PriorityId", result.PriorityId);
                    cmd.Parameters.AddWithValue("@TriageId", result.TriageId);

                    using var r = cmd.ExecuteReader();
                    if (r.Read())
                    {
                        color = r["Color"]?.ToString() ?? "X";
                        priorityName = r["PriorityName"]?.ToString() ?? "Sin prioridad";
                        patientEmail = r["Email"]?.ToString() ?? "";
                        patientName = r["FullName"]?.ToString() ?? "Paciente";
                    }
                }

                // 3) Generar turno seguro por orden de llegada
                string turnCode = GenerateTurnCode(conn, tx, result.TriageId, result.PriorityId, color);

                // 4) Actualizar turno en TRIAGE
                const string updSql = "UPDATE TRIAGE SET TURNO = @Turn WHERE ID_TRIAGE = @Id;";
                using (var cmd = new SqlCommand(updSql, conn, tx))
                {
                    cmd.Parameters.AddWithValue("@Turn", turnCode);
                    cmd.Parameters.AddWithValue("@Id", result.TriageId);
                    cmd.ExecuteNonQuery();
                }

                tx.Commit();

                // 5) ENVIAR CORREO EN SEGUNDO PLANO (NUNCA BLOQUEA)
                if (!string.IsNullOrWhiteSpace(patientEmail))
                {
                    string subject = "Actualización de su turno y prioridad";
                    string body = EmailTemplates.BuildPriorityUpdateBody(patientName, priorityName, turnCode);

                    MailMessage msg = EmailUtility.BuildEmail(patientEmail, subject, body);
                    _emailService.Enqueue(msg);
                }

                return true;
            }
            catch
            {
                try { tx.Rollback(); } catch { }
                throw;
            }
        }

        // Calcula turno seguro
        private static string GenerateTurnCode(SqlConnection conn, SqlTransaction tx, int triageId, int priorityId, string color)
        {
            DateTime fechaRegistroActual;

            // Obtener fecha
            const string fechaSql = "SELECT FECHA_REGISTRO FROM TRIAGE WHERE ID_TRIAGE = @Id;";
            using (var cmd = new SqlCommand(fechaSql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@Id", triageId);
                fechaRegistroActual = (DateTime)cmd.ExecuteScalar();
            }

            // Contar orden
            const string ordenSql = @"
SELECT COUNT(*) + 1
FROM TRIAGE
WHERE ID_PRIORIDAD = @Prio
  AND (
        FECHA_REGISTRO < @Fecha
        OR (FECHA_REGISTRO = @Fecha AND ID_TRIAGE < @IdTriage)
      );";

            int position;
            using (var cmd = new SqlCommand(ordenSql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@Prio", priorityId);
                cmd.Parameters.AddWithValue("@Fecha", fechaRegistroActual);
                cmd.Parameters.AddWithValue("@IdTriage", triageId);
                position = Convert.ToInt32(cmd.ExecuteScalar());
            }

            string initial = string.IsNullOrEmpty(color) ? "X" : color.Substring(0, 1).ToUpper();
            return $"turno-{initial}{position}";
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

                using var reader = await command.ExecuteReaderAsync();
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

            _context.CloseConnection();
            return results;
        }

        // Trae nombre y descripción según ID triage
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

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    result = new TriagePriorityInfoDto
                    {
                        PriorityName = reader["PriorityName"].ToString() ?? "",
                        PriorityDescription = reader["PriorityDescription"].ToString() ?? ""
                    };
                }
            }

            _context.CloseConnection();
            return result;
        }

        // Trae todas las prioridades
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
