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

                // Insertar medicamentos (si existen)
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

                tx.Commit();
                return idTreatment; //  Devuelve el ID generado
            }
            catch
            {
                tx.Rollback();
                return 0; // Error
            }
        }
    }
}