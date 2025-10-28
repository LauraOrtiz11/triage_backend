using Microsoft.Data.SqlClient;
using triage_backend.Dtos;
using System.Data;

namespace triage_backend.Repositories
{
    public class TreatmentRepository
    {
        private readonly string _connectionString = string.Empty;

        public TreatmentRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        /// <summary>
        /// Registra un tratamiento asociado a una consulta, junto con sus medicamentos y exámenes.
        /// </summary>
        public int RegisterTreatment(TreatmentRequestDto request)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var tx = conn.BeginTransaction();

            try
            {
                // Insertar tratamiento vinculado a la consulta (NO al historial)
                const string insertTreatmentSql = @"
                    INSERT INTO TRATAMIENTO (DESCRIP_TRATA, ID_CONSULTA)
                    OUTPUT INSERTED.ID_TRATAMIENTO
                    VALUES (@Desc, @IdConsulta);";

                int idTreatment;
                using (var cmd = new SqlCommand(insertTreatmentSql, conn, tx))
                {
                    cmd.Parameters.AddWithValue("@Desc", (object?)request.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IdConsulta", request.ConsultationId);
                    idTreatment = (int)cmd.ExecuteScalar();
                }

                // Asociar medicamentos al tratamiento
                if (request.MedicationIds != null && request.MedicationIds.Any())
                {
                    const string insertTreatMedSql = @"
                        INSERT INTO TRATAMIENTO_MEDICAMENTO (ID_TRATAMIENTO, ID_MEDICAMENTO)
                        VALUES (@TreatId, @MedId);";

                    foreach (var medId in request.MedicationIds)
                    {
                        using var cmd = new SqlCommand(insertTreatMedSql, conn, tx);
                        cmd.Parameters.AddWithValue("@TreatId", idTreatment);
                        cmd.Parameters.AddWithValue("@MedId", medId);
                        cmd.ExecuteNonQuery();
                    }
                }

                // Asociar exámenes al tratamiento
                if (request.ExamIds != null && request.ExamIds.Any())
                {
                    const string insertTreatExamSql = @"
                        INSERT INTO TRATAMIENTO_EXAMEN (ID_TRATAMIENTO, ID_EXAMEN)
                        VALUES (@TreatId, @ExamId);";

                    foreach (var examId in request.ExamIds)
                    {
                        using var cmd = new SqlCommand(insertTreatExamSql, conn, tx);
                        cmd.Parameters.AddWithValue("@TreatId", idTreatment);
                        cmd.Parameters.AddWithValue("@ExamId", examId);
                        cmd.ExecuteNonQuery();
                    }
                }

                // Marcar la consulta como finalizada
                const string updateConsultaSql = @"
                    UPDATE CONSULTA
                    SET FECHA_FIN_CONSULTA = GETDATE(),
                        ID_ESTADO = 2 -- Finalizado
                    WHERE ID_CONSULTA = @IdConsulta;";

                using (var cmd = new SqlCommand(updateConsultaSql, conn, tx))
                {
                    cmd.Parameters.AddWithValue("@IdConsulta", request.ConsultationId);
                    cmd.ExecuteNonQuery();
                }

                // Confirmar transacción
                tx.Commit();
                return idTreatment;
            }
            catch
            {
                tx.Rollback();
                return 0;
            }
        }
    }
}
