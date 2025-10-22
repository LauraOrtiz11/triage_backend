using Microsoft.Data.SqlClient;
using triage_backend.Dtos;
using triage_backend.Interfaces;
using triage_backend.Utilities;

namespace triage_backend.Repositories
{
    public class TriageRepository : ITriageRepository
    {
        private readonly ContextDB _context;

        public TriageRepository(ContextDB context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<int> InsertTriageAsync(
            TriageRequestDto request,
            string suggestedLevel,
            int ID_Patient,
            int ID_Doctor,
            int ID_Nurse,
            int ID_Priority,
            int ID_State,
            int PatientAge)
        {
            using var conn = (SqlConnection)_context.OpenConnection();
            using var tx = conn.BeginTransaction();

            try
            {
                // 1) Insert TRIAGE
                int triageId;
                const string insertSql = @"
INSERT INTO TRIAGE (
    ID_PACIENTE, ID_MEDICO, ID_PRIORIDAD, ID_ESTADO, FECHA_REGISTRO,
    SINTOMAS, TEMPERATURA, FRECUENCIA_CARD, FRECUENCIA_RES,
    PRESION_ARTERIAL, OXIGENACION, ID_ENFERMERO
)
VALUES (
    @ID_PACIENTE, @ID_MEDICO, @ID_PRIORIDAD, @ID_ESTADO, GETDATE(),
    @SINTOMAS, @TEMPERATURA, @FRECUENCIA_CARD, @FRECUENCIA_RES,
    @PRESION_ARTERIAL, @OXIGENACION, @ID_ENFERMERO
);
SELECT SCOPE_IDENTITY();";

                using (var cmd = new SqlCommand(insertSql, conn, tx))
                {
                    cmd.Parameters.AddWithValue("@ID_PACIENTE", ID_Patient);
                    cmd.Parameters.AddWithValue("@ID_MEDICO", (object?)ID_Doctor ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ID_PRIORIDAD", ID_Priority);
                    cmd.Parameters.AddWithValue("@ID_ESTADO", ID_State);
                    cmd.Parameters.AddWithValue("@SINTOMAS", (object?)request.Symptoms ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TEMPERATURA", request.VitalSigns.Temperature);
                    cmd.Parameters.AddWithValue("@FRECUENCIA_CARD", request.VitalSigns.HeartRate);
                    cmd.Parameters.AddWithValue("@FRECUENCIA_RES", request.VitalSigns.RespiratoryRate);
                    cmd.Parameters.AddWithValue("@PRESION_ARTERIAL", (object?)request.VitalSigns.BloodPressure ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@OXIGENACION", request.VitalSigns.OxygenSaturation);
                    cmd.Parameters.AddWithValue("@ID_ENFERMERO", ID_Nurse);

                    var res = await cmd.ExecuteScalarAsync();
                    triageId = Convert.ToInt32(res);
                }

                // 2) Registrar prioridad inicial en TRIAGE_RESULTADO
                const string triageResSql = @"
INSERT INTO TRIAGE_RESULTADO (ID_Triage, ID_Prioridad, ID_Usuario, Es_Prioridad_Final, Fecha_Registro)
VALUES (@IdTriage, @IdPrioridad, @IdUsuario, 0, GETDATE());";
                using (var cmd = new SqlCommand(triageResSql, conn, tx))
                {
                    cmd.Parameters.AddWithValue("@IdTriage", triageId);
                    cmd.Parameters.AddWithValue("@IdPrioridad", ID_Priority);
                    cmd.Parameters.AddWithValue("@IdUsuario", ID_Nurse);
                    await cmd.ExecuteNonQueryAsync();
                }

                // 3) Obtener color/nombre prioridad
                string color = "X";
                string priorityName = "Sin prioridad";
                const string prioSql = "SELECT NOMBRE_PRIO, COLOR_PRIO FROM PRIORIDAD WHERE ID_PRIORIDAD = @Id;";
                using (var cmd = new SqlCommand(prioSql, conn, tx))
                {
                    cmd.Parameters.AddWithValue("@Id", ID_Priority);
                    using var r = await cmd.ExecuteReaderAsync();
                    if (await r.ReadAsync())
                    {
                        priorityName = r["NOMBRE_PRIO"].ToString() ?? "Sin prioridad";
                        color = r["COLOR_PRIO"].ToString() ?? "X";
                    }
                }

                // 4) Generar turno seguro
                string turnCode = GenerateTurnCode(conn, tx, triageId, ID_Priority, color);

                // 5) Actualizar turno en TRIAGE
                const string updSql = "UPDATE TRIAGE SET TURNO = @Turn WHERE ID_TRIAGE = @Id;";
                using (var cmd = new SqlCommand(updSql, conn, tx))
                {
                    cmd.Parameters.AddWithValue("@Turn", turnCode);
                    cmd.Parameters.AddWithValue("@Id", triageId);
                    await cmd.ExecuteNonQueryAsync();
                }

                // 6) Obtener email/nombre paciente
                string email = "";
                string patientName = "Paciente";
                const string mailSql = @"
SELECT U.CORREO_US, U.NOMBRE_US + ' ' + U.APELLIDO_US AS PACIENTE
FROM USUARIO U
JOIN TRIAGE T ON T.ID_PACIENTE = U.ID_USUARIO
WHERE T.ID_TRIAGE = @Id;";
                using (var cmd = new SqlCommand(mailSql, conn, tx))
                {
                    cmd.Parameters.AddWithValue("@Id", triageId);
                    using var r = await cmd.ExecuteReaderAsync();
                    if (await r.ReadAsync())
                    {
                        email = r["CORREO_US"]?.ToString() ?? "";
                        patientName = r["PACIENTE"]?.ToString() ?? "Paciente";
                    }
                }

                tx.Commit();

                // 7) Enviar correo fuera de la tx
                if (!string.IsNullOrWhiteSpace(email))
                {
                    var subject = "Registro de turno y prioridad en triage";
                    var body = EmailTemplates.BuildPriorityUpdateBody(patientName, priorityName, turnCode);
                    EmailUtility.SendEmail(email, subject, body);
                }

                return triageId;
            }
            catch
            {
                try { tx.Rollback(); } catch { /* noop */ }
                throw;
            }
        }

        private static string GenerateTurnCode(SqlConnection conn, SqlTransaction tx, int triageId, int priorityId, string color)
        {
            DateTime fechaActual;

            const string fechaSql = "SELECT FECHA_REGISTRO FROM TRIAGE WHERE ID_TRIAGE = @Id;";
            using (var cmd = new SqlCommand(fechaSql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@Id", triageId);
                fechaActual = (DateTime)cmd.ExecuteScalar();
            }

            const string ordenSql = @"
SELECT COUNT(*) + 1
FROM TRIAGE
WHERE ID_PRIORIDAD = @Prio
  AND (
        FECHA_REGISTRO < @Fecha
        OR (FECHA_REGISTRO = @Fecha AND ID_TRIAGE < @IdTriage)
      );";

            int pos;
            using (var cmd = new SqlCommand(ordenSql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@Prio", priorityId);
                cmd.Parameters.AddWithValue("@Fecha", fechaActual);
                cmd.Parameters.AddWithValue("@IdTriage", triageId);
                pos = Convert.ToInt32(cmd.ExecuteScalar());
            }

            string initial = string.IsNullOrEmpty(color) ? "X" : color.Substring(0, 1).ToUpper();
            return $"turno-{initial}{pos}";
        }
    }
}