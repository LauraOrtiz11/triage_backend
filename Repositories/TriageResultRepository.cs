using Microsoft.Data.SqlClient;
using System.Data;
using triage_backend.Utilities;
using triage_backend.Dtos;

namespace triage_backend.Repositories
{
    public class TriageResultRepository
    {
        private readonly ContextDB _context;

        public TriageResultRepository(ContextDB context)
        {
            _context = context;
        }

        public bool SaveTriageResult(TriageResultDto result)
        {
            
            using (var connection = (SqlConnection)_context.OpenConnection())
            {

                const string query = @"
                    INSERT INTO TRIAGE_RESULTADO (ID_Triage, ID_Prioridad, ID_Usuario, Es_Prioridad_Final)
                    VALUES (@TriageId, @PriorityId, @UserId, @IsFinalPriority);
                ";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TriageId", result.TriageId);
                    command.Parameters.AddWithValue("@PriorityId", result.PriorityId);
                    command.Parameters.AddWithValue("@UserId", result.NurseId);
                    command.Parameters.AddWithValue("@IsFinalPriority", result.IsFinalPriority);

                    int rows = command.ExecuteNonQuery();
                    return rows > 0;
                }
            } 
        }
    }
}
