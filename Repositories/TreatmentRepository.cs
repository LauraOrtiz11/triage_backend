using Microsoft.Data.SqlClient;
using triage_backend.Dtos;

namespace triage_backend.Repositories
{
    public class TreatmentRepository
    {
        private readonly string _connectionString = string.Empty;

        public TreatmentRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        public int RegisterTreatment(TreatmentRequestDto request)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var tx = conn.BeginTransaction();

            try
            {
                // Insertar el tratamiento vinculado al historial
                const string insertTreatmentSql = @"
                    INSERT INTO TRATAMIENTO (DESCRIP_TRATA, ID_HISTORIAL)
                    OUTPUT INSERTED.ID_TRATAMIENTO
                    VALUES (@Desc, @IdHist);";

                int idTreatment;
                using (var cmd = new SqlCommand(insertTreatmentSql, conn, tx))
                {
                    cmd.Parameters.AddWithValue("@Desc", (object?)request.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IdHist", request.IdHistory);
                    idTreatment = (int)cmd.ExecuteScalar();
                }

                // Asociar medicamentos
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

                // Asociar exámenes
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

                // Finalizar la consulta 
                const string updateConsultaSql = @"
                    UPDATE CONSULTA
                    SET FECHA_FIN_CONSULTA = GETDATE(),
                        ID_ESTADO = 2
                    WHERE ID_HISTORIAL = @IdHist;";

                using (var cmd = new SqlCommand(updateConsultaSql, conn, tx))
                {
                    cmd.Parameters.AddWithValue("@IdHist", request.IdHistory);
                    cmd.ExecuteNonQuery();
                }

                //Confirmar la transacción
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
