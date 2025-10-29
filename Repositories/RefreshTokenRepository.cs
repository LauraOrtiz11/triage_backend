using System;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using triage_backend.Utilities;

namespace triage_backend.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ContextDB _context;

        public RefreshTokenRepository(ContextDB context)
        {
            _context = context;
        }

        private string HashToken(string token)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public async Task SaveAsync(int userId, string token, DateTime expiresAt)
        {
            var tokenHash = HashToken(token);

            using var conn = (SqlConnection)_context.OpenConnection();
            var sql = @"INSERT INTO RefreshTokens (UserId, TokenHash, CreatedAt, ExpiresAt, Revoked)
                        VALUES (@userId, @tokenHash, SYSUTCDATETIME(), @expiresAt, 0)";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@tokenHash", tokenHash);
            cmd.Parameters.AddWithValue("@expiresAt", expiresAt);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> ValidateAsync(int userId, string token)
        {
            var tokenHash = HashToken(token);
            using var conn = (SqlConnection)_context.OpenConnection();
            var sql = @"SELECT COUNT(1) FROM RefreshTokens 
                        WHERE UserId = @userId AND TokenHash = @tokenHash AND Revoked = 0 AND ExpiresAt > SYSUTCDATETIME()";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@tokenHash", tokenHash);

            var result = (int)await cmd.ExecuteScalarAsync();
            return result > 0;
        }

        public async Task RevokeAsync(string token)
        {
            var tokenHash = HashToken(token);
            using var conn = (SqlConnection)_context.OpenConnection();
            var sql = @"UPDATE RefreshTokens SET Revoked = 1 WHERE TokenHash = @tokenHash";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tokenHash", tokenHash);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task RevokeAllForUserAsync(int userId)
        {
            using var conn = (SqlConnection)_context.OpenConnection();
            var sql = @"UPDATE RefreshTokens SET Revoked = 1 WHERE UserId = @userId";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            await cmd.ExecuteNonQueryAsync();
        }
        public async Task<int?> GetUserIdIfValidAsync(string token)
        {
            var tokenHash = HashToken(token);
            using var conn = (SqlConnection)_context.OpenConnection();
            var sql = @"SELECT UserId FROM RefreshTokens WHERE TokenHash = @tokenHash AND Revoked = 0 AND ExpiresAt > SYSUTCDATETIME()";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tokenHash", tokenHash);
            var result = await cmd.ExecuteScalarAsync();
            return result == null ? (int?)null : Convert.ToInt32(result);
        }
    }
}
