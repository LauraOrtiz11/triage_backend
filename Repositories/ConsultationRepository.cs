using System;
using System.Data;
using Microsoft.Data.SqlClient;
using triage_backend.Dtos;
using triage_backend.Utilities;

namespace triage_backend.Repositories
{
    public class ConsultationRepository
    {
        private readonly ContextDB _context;

        public ConsultationRepository(ContextDB context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Inicia una nueva consulta médica y devuelve el ID de la consulta creada.
        /// Si algo falla, devuelve 0.
        /// </summary>
        public int StartConsultation(StartConsultationDto model)
        {
            const string checkHistoryQuery = @"
                SELECT TOP 1 ID_HISTORIAL 
                FROM HISTORIAL 
                WHERE ID_PACIENTE = (
                    SELECT ID_PACIENTE FROM TRIAGE WHERE ID_TRIAGE = @IdTriage
                );";

            const string insertHistoryQuery = @"
                INSERT INTO HISTORIAL (ID_PACIENTE, ID_MEDICO, ID_ALERTA, FECHA_REGISTRO)
                VALUES (
                    (SELECT ID_PACIENTE FROM TRIAGE WHERE ID_TRIAGE = @IdTriage),
                    @IdMedic,
                    NULL,
                    GETDATE()
                );
                SELECT SCOPE_IDENTITY();";

            // 🔹 Ahora también actualiza el ID_MEDICO del triage
            const string updateTriageQuery = @"
                UPDATE TRIAGE 
                SET 
                    ID_ESTADO = 2, 
                    FECHA_FIN_TRIAGE = GETDATE(),
                    ID_MEDICO = @IdMedic
                WHERE ID_TRIAGE = @IdTriage;";

            const string insertConsultationQuery = @"
                INSERT INTO CONSULTA 
                    (ID_HISTORIAL, ID_MEDICO, ID_TRIAGE, ID_ESTADO, FECHA_INICIO_CONSULTA, FECHA_FIN_CONSULTA)
                OUTPUT INSERTED.ID_CONSULTA
                VALUES (@IdHistorial, @IdMedic, @IdTriage, 1, GETDATE(), NULL);";

            using (var connection = _context.OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    int idHistorial;

                    // 1️⃣ Verificar si ya existe historial del paciente
                    using (var checkCmd = new SqlCommand(checkHistoryQuery, (SqlConnection)connection, (SqlTransaction)transaction))
                    {
                        checkCmd.Parameters.AddWithValue("@IdTriage", model.IdTriage);
                        var result = checkCmd.ExecuteScalar();
                        idHistorial = (result != null && result != DBNull.Value) ? Convert.ToInt32(result) : 0;
                    }

                    // 2️⃣ Si no existe historial, crear uno nuevo
                    if (idHistorial == 0)
                    {
                        using (var insertHistCmd = new SqlCommand(insertHistoryQuery, (SqlConnection)connection, (SqlTransaction)transaction))
                        {
                            insertHistCmd.Parameters.AddWithValue("@IdTriage", model.IdTriage);
                            insertHistCmd.Parameters.AddWithValue("@IdMedic", model.IdMedic);
                            idHistorial = Convert.ToInt32(insertHistCmd.ExecuteScalar());
                        }
                    }

                    // 3️⃣ Actualizar el triage (estado + médico)
                    using (var updateTriageCmd = new SqlCommand(updateTriageQuery, (SqlConnection)connection, (SqlTransaction)transaction))
                    {
                        updateTriageCmd.Parameters.AddWithValue("@IdTriage", model.IdTriage);
                        updateTriageCmd.Parameters.AddWithValue("@IdMedic", model.IdMedic);
                        updateTriageCmd.ExecuteNonQuery();
                    }

                    // 4️⃣ Crear la consulta y obtener su ID
                    int idConsulta;
                    using (var insertConsCmd = new SqlCommand(insertConsultationQuery, (SqlConnection)connection, (SqlTransaction)transaction))
                    {
                        insertConsCmd.Parameters.AddWithValue("@IdHistorial", idHistorial);
                        insertConsCmd.Parameters.AddWithValue("@IdMedic", model.IdMedic);
                        insertConsCmd.Parameters.AddWithValue("@IdTriage", model.IdTriage);
                        idConsulta = Convert.ToInt32(insertConsCmd.ExecuteScalar());
                    }

                    // 5️⃣ Confirmar la transacción
                    transaction.Commit();

                    Console.WriteLine($"[INFO] Consulta creada correctamente (ID_CONSULTA: {idConsulta}, ID_HISTORIAL: {idHistorial}, MÉDICO: {model.IdMedic}).");
                    return idConsulta;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"[SQL ERROR] {ex.Message}");
                    return 0;
                }
                finally
                {
                    _context.CloseConnection();
                }
            }
        }
    }
}
