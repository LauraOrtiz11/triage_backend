using System;
using System.Threading.Tasks;

namespace triage_backend.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task SaveAsync(int userId, string tokenHash, DateTime expiresAt);
        Task<bool> ValidateAsync(int userId, string tokenHash);
        Task RevokeAsync(string tokenHash);
        Task RevokeAllForUserAsync(int userId);
        Task<int?> GetUserIdIfValidAsync(string token);

    }
}
