using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using triage_backend.Utilities;

namespace triage_backend.Repositories
{
    public class RevokedTokenRepository : IRevokedTokenRepository
    {
        private readonly ContextDB _context;
        public RevokedTokenRepository(ContextDB context)
        {
            _context = context;
        }

        public async Task AddAsync(string jti, DateTime? expiresAt = null)
        {
            using SqlConnection conn = (SqlConnection)_context.OpenConnection();
            using SqlCommand cmd = new SqlCommand(
                "INSERT INTO RevokedTokens (Jti, RevokedAt, ExpiresAt) VALUES (@Jti, GETUTCDATE(), @ExpiresAt)",
                conn);
            cmd.Parameters.AddWithValue("@Jti", jti);
            cmd.Parameters.AddWithValue("@ExpiresAt", (object?)expiresAt ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> IsRevokedAsync(string jti)
        {
            using SqlConnection conn = (SqlConnection)_context.OpenConnection();
            using SqlCommand cmd = new SqlCommand("SELECT COUNT(1) FROM RevokedTokens WHERE Jti = @Jti", conn);
            cmd.Parameters.AddWithValue("@Jti", jti);
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }
    }
}
