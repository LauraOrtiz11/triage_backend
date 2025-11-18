using Microsoft.Data.SqlClient;
using triage_backend.Dtos;
using triage_backend.Interfaces;
using triage_backend.Services;
using triage_backend.Utilities;

namespace triage_backend.Repositories
{
    public class TriageRepository : ITriageRepository
    {
        private readonly ContextDB _context;
        private readonly EmailBackgroundService _emailService;

        public TriageRepository(ContextDB context, EmailBackgroundService emailService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
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
            using var cmd = new SqlCommand("SP_InsertTriageFull", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            // Parámetros de entrada
            cmd.Parameters.AddWithValue("@ID_PACIENTE", ID_Patient);
            cmd.Parameters.AddWithValue("@ID_MEDICO", ID_Doctor);
            cmd.Parameters.AddWithValue("@ID_ENFERMERO", ID_Nurse);
            cmd.Parameters.AddWithValue("@ID_PRIORIDAD", ID_Priority);
            cmd.Parameters.AddWithValue("@ID_ESTADO", ID_State);
            cmd.Parameters.AddWithValue("@SINTOMAS", (object?)request.Symptoms ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TEMPERATURA", request.VitalSigns.Temperature);
            cmd.Parameters.AddWithValue("@FRECUENCIA_CARD", request.VitalSigns.HeartRate);
            cmd.Parameters.AddWithValue("@FRECUENCIA_RES", request.VitalSigns.RespiratoryRate);
            cmd.Parameters.AddWithValue("@PRESION_ARTERIAL", (object?)request.VitalSigns.BloodPressure ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@OXIGENACION", request.VitalSigns.OxygenSaturation);

            // Parámetros de salida
            var outId = new SqlParameter("@TRIAGE_ID", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output };
            var outPriority = new SqlParameter("@PRIORIDAD_NOMBRE", System.Data.SqlDbType.NVarChar, 100) { Direction = System.Data.ParameterDirection.Output };
            var outTurn = new SqlParameter("@TURNO", System.Data.SqlDbType.NVarChar, 50) { Direction = System.Data.ParameterDirection.Output };

            cmd.Parameters.Add(outId);
            cmd.Parameters.Add(outPriority);
            cmd.Parameters.Add(outTurn);

            // Ejecutar SP
            await cmd.ExecuteNonQueryAsync();

            int triageId = Convert.ToInt32(outId.Value);
            string priorityName = outPriority.Value?.ToString() ?? "Sin prioridad";
            string turnCode = outTurn.Value?.ToString() ?? "N/A";

            // ==========================================
            // EXTRAER EMAIL DEL PACIENTE
            // ==========================================
            const string mailSql = @"
                SELECT 
                    U.CORREO_US, 
                    U.NOMBRE_US + ' ' + U.APELLIDO_US AS PACIENTE
                FROM USUARIO U
                JOIN TRIAGE T ON T.ID_PACIENTE = U.ID_USUARIO
                WHERE T.ID_TRIAGE = @Id;";

            string email = "";
            string patientName = "Paciente";

            using (var cmdMail = new SqlCommand(mailSql, conn))
            {
                cmdMail.Parameters.AddWithValue("@Id", triageId);

                using var reader = await cmdMail.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    email = reader["CORREO_US"]?.ToString() ?? "";
                    patientName = reader["PACIENTE"]?.ToString() ?? "Paciente";
                }
            }

            // ==========================================
            // ENVIAR CORREO EN SEGUNDO PLANO (NO BLOQUEA)
            // ==========================================
            if (!string.IsNullOrWhiteSpace(email))
            {
                string subject = "Registro de turno y prioridad en triage";
                string body = EmailTemplates.BuildPriorityUpdateBody(patientName, priorityName, turnCode);

                var msg = EmailUtility.BuildEmail(email, subject, body);
                _emailService.Enqueue(msg);
            }

            return triageId;
        }
    }
}
