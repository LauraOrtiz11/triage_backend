using Microsoft.Data.SqlClient;
using triage_backend.Dtos;
using triage_backend.Utilities;

namespace triage_backend.Repositories
{
    public class PriorityUpdateRepository
    {
        private readonly ContextDB _context;

        public PriorityUpdateRepository(ContextDB context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Devuelve el estado actual del paciente con su última prioridad y turno.
        /// </summary>
        public PatientStatusDto? GetPatientStatus(int triageId)
        {
            using var conn = (SqlConnection)_context.OpenConnection();

            const string query = @"
                SELECT TOP 1
                    U.NOMBRE_US + ' ' + U.APELLIDO_US AS FullName,
                    PR.NOMBRE_PRIO AS PriorityLevel,
                    PR.COLOR_PRIO AS PriorityColor,
                    T.TURNO AS TurnCode,
                    E.NOMBRE_US + ' ' + E.APELLIDO_US AS NurseName,
                    T.FECHA_REGISTRO AS ArrivalDate,
                    T.SINTOMAS AS Symptoms,
                    T.PRESION_ARTERIAL AS BloodPressure,
                    T.TEMPERATURA AS Temperature,
                    T.FRECUENCIA_CARD AS HeartRate,
                    T.FRECUENCIA_RES AS RespiratoryRate,
                    T.OXIGENACION AS OxygenSaturation
                FROM TRIAGE_RESULTADO TR
                INNER JOIN TRIAGE T ON T.ID_TRIAGE = TR.ID_Triage
                INNER JOIN PRIORIDAD PR ON PR.ID_PRIORIDAD = TR.ID_Prioridad
                INNER JOIN USUARIO U ON U.ID_USUARIO = T.ID_PACIENTE
                LEFT JOIN USUARIO E ON E.ID_USUARIO = T.ID_ENFERMERO
                WHERE TR.ID_Triage = @IdTriage
                ORDER BY TR.FECHA_REGISTRO DESC;";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@IdTriage", triageId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new PatientStatusDto
                {
                    FullName = reader["FullName"]?.ToString(),
                    PriorityLevel = reader["PriorityLevel"]?.ToString(),
                    PriorityColor = reader["PriorityColor"]?.ToString(),
                    TurnCode = reader["TurnCode"]?.ToString(),
                    NurseName = reader["NurseName"]?.ToString(),
                    ArrivalDate = reader["ArrivalDate"] as DateTime?,
                    Symptoms = reader["Symptoms"]?.ToString(),
                    BloodPressure = reader["BloodPressure"]?.ToString(),
                    Temperature = reader["Temperature"] != DBNull.Value ? Convert.ToDouble(reader["Temperature"]) : null,
                    HeartRate = reader["HeartRate"] != DBNull.Value ? Convert.ToInt32(reader["HeartRate"]) : null,
                    RespiratoryRate = reader["RespiratoryRate"] != DBNull.Value ? Convert.ToInt32(reader["RespiratoryRate"]) : null,
                    OxygenSaturation = reader["OxygenSaturation"] != DBNull.Value ? Convert.ToInt32(reader["OxygenSaturation"]) : null
                };
            }

            return null;
        }
    }
}
    