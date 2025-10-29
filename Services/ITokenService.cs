using System;
using System.Collections.Generic;

namespace triage_backend.Services
{
    public interface ITokenService
    {

        // Access token (short-lived)
        string CreateAccessToken(string userId, string email, IEnumerable<string>? roles = null);
        DateTime GetAccessExpiry();

        // Refresh token (random string)
        string CreateRefreshToken();
        DateTime GetRefreshExpiry();

        // (Opcional) helper legacy - no hace falta si migras
        // string CreateToken(string userId, string email, IEnumerable<string>? roles = null);
        // DateTime GetExpiry();
    }
}
