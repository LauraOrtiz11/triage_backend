using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace triage_backend.Repositories
{
    public interface IRevokedTokenRepository
    {
        Task AddAsync(string jti, DateTime? expiresAt = null);
        Task<bool> IsRevokedAsync(string jti);
    }
}
    