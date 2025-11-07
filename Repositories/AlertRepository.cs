using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using triage_backend.Dtos;
using triage_backend.Utilities;

namespace triage_backend.Repositories
{
    public class AlertRepository
    {
        private readonly ContextDB _context;

        public AlertRepository(ContextDB context)
        {
            _context = context;
        }

        /// <summary>
        /// Registers a deterioration alert for a patient.
        /// The nurse is automatically determined from the latest triage record.
        /// </summary>
        public void RegisterAlert(CreateAlertDto dto)
        {
            const string getNurseQuery = @"
                SELECT TOP 1 ID_ENFERMERO
                FROM TRIAGE
                WHERE ID_PACIENTE = @IdPatient
                ORDER BY FECHA_REGISTRO DESC;
            ";

            int? idNurse = null;

            using var connection = _context.OpenConnection();
            using (var cmd = new SqlCommand(getNurseQuery, (SqlConnection)connection))
            {
                cmd.Parameters.AddWithValue("@IdPatient", dto.IdPatient);
                var result = cmd.ExecuteScalar();
                if (result != DBNull.Value && result != null)
                    idNurse = Convert.ToInt32(result);
            }

            if (idNurse == null)
                throw new InvalidOperationException("No se encontró un enfermero asociado al paciente.");

            const string insertAlertQuery = @"
                INSERT INTO ALERTA (ID_PACIENTE, ID_ENFERMERO, FECHA_LIM, ID_ESTADO)
                VALUES (@IdPatient, @IdNurse, GETDATE(), 1); -- 1 = Pendiente
            ";

            using (var cmd = new SqlCommand(insertAlertQuery, (SqlConnection)connection))
            {
                cmd.Parameters.AddWithValue("@IdPatient", dto.IdPatient);
                cmd.Parameters.AddWithValue("@IdNurse", idNurse);
                cmd.ExecuteNonQuery();
            }

            _context.CloseConnection();
        }

        /// <summary>
        /// Returns all deterioration alerts (for all nurses).
        /// Shows patients who are still in triage or have no final consultation.
        /// </summary>
        public List<AlertDetailDto> GetAllAlerts()
        {
            const string query = @"
                SELECT 
                    A.ID_ALERTA,
                    A.FECHA_LIM AS ALERT_DATE,
                    A.ID_ESTADO,
                    E.NOMBRE_EST AS STATUS_NAME,
                    P.ID_USUARIO AS ID_PATIENT,
                    (P.NOMBRE_US + ' ' + P.APELLIDO_US) AS PATIENT_NAME,
                    T.ID_TRIAGE,
                    PR.NOMBRE_PRIO AS PRIORITY_NAME,
                    PR.COLOR_PRIO AS PRIORITY_COLOR,
                    T.SINTOMAS,
                    T.FECHA_REGISTRO AS TRIAGE_DATE,
                    A.ID_ENFERMERO
                FROM ALERTA A
                INNER JOIN USUARIO P ON A.ID_PACIENTE = P.ID_USUARIO
                INNER JOIN ESTADO E ON A.ID_ESTADO = E.ID_ESTADO
                LEFT JOIN TRIAGE T ON T.ID_PACIENTE = P.ID_USUARIO
                LEFT JOIN PRIORIDAD PR ON T.ID_PRIORIDAD = PR.ID_PRIORIDAD
                WHERE 
                    (T.ID_ESTADO = 1 OR T.ID_ESTADO IS NULL)
                ORDER BY A.FECHA_LIM DESC;
            ";

            var alerts = new List<AlertDetailDto>();
            using var connection = _context.OpenConnection();
            using var cmd = new SqlCommand(query, (SqlConnection)connection);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                alerts.Add(new AlertDetailDto
                {
                    IdAlert = Convert.ToInt32(reader["ID_ALERTA"]),
                    IdPatient = Convert.ToInt32(reader["ID_PATIENT"]),
                    PatientName = reader["PATIENT_NAME"].ToString(),
                    PriorityName = reader["PRIORITY_NAME"]?.ToString() ?? "Sin prioridad",
                    PriorityColor = reader["PRIORITY_COLOR"]?.ToString(),
                    AlertDate = Convert.ToDateTime(reader["ALERT_DATE"]),
                    StatusName = reader["STATUS_NAME"].ToString(),
                    Symptoms = reader["SINTOMAS"]?.ToString(),
                    TriageDate = reader["TRIAGE_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["TRIAGE_DATE"])
                });
            }

            _context.CloseConnection();
            return alerts;
        }

        /// <summary>
        /// Updates the alert status (1 = Pendiente, 2 = Atendido).
        /// </summary>
        public void UpdateAlertStatus(int idAlert, int idStatus)
        {
            const string query = @"
                UPDATE ALERTA 
                SET ID_ESTADO = @IdStatus
                WHERE ID_ALERTA = @IdAlert;
            ";

            using var connection = _context.OpenConnection();
            using var cmd = new SqlCommand(query, (SqlConnection)connection);
            cmd.Parameters.AddWithValue("@IdAlert", idAlert);
            cmd.Parameters.AddWithValue("@IdStatus", idStatus);

            cmd.ExecuteNonQuery();
            _context.CloseConnection();
        }
    }
}
