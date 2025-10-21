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

        public bool RegisterTreatment(TreatmentRequestDto request)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        // 1️⃣ Insertar el tratamiento
                        const string insertTreatmentSql = @"
                            INSERT INTO TRATAMIENTO (DESCRIP_TRATA)
                            OUTPUT INSERTED.ID_TRATAMIENTO
                            VALUES (@Desc);";

                        int idTreatment;
                        using (var cmd = new SqlCommand(insertTreatmentSql, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@Desc", request.Description ?? (object)DBNull.Value);
                            idTreatment = (int)cmd.ExecuteScalar();
                        }

                        // 2️⃣ Asociar tratamiento con diagnóstico
                        const string insertDiagTreatSql = @"
                            INSERT INTO DIAGNOSTICO_TRATAMIENTO (ID_TRATAMIENTO, ID_DIAGNOSTICO)
                            VALUES (@TreatId, @DiagId);";

                        using (var cmd = new SqlCommand(insertDiagTreatSql, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@TreatId", idTreatment);
                            cmd.Parameters.AddWithValue("@DiagId", request.IdDiagnosis);
                            cmd.ExecuteNonQuery();
                        }

                        // 3️⃣ Asociar medicamentos (si existen)
                        if (request.MedicationIds != null && request.MedicationIds.Any())
                        {
                            const string insertTreatMedSql = @"
                                INSERT INTO TRATAMIENTO_MEDICAMENTO (ID_TRATAMIENTO, ID_MEDICAMENTO)
                                VALUES (@TreatId, @MedId);";

                            foreach (var medId in request.MedicationIds)
                            {
                                using (var cmd = new SqlCommand(insertTreatMedSql, conn, tx))
                                {
                                    cmd.Parameters.AddWithValue("@TreatId", idTreatment);
                                    cmd.Parameters.AddWithValue("@MedId", medId);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        tx.Commit();
                        return true;
                    }
                    catch
                    {
                        tx.Rollback();
                        return false;
                    }
                }
            }
        }
    }
}
