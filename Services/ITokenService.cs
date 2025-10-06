using System;
using System.Collections.Generic;

namespace triage_backend.Services
{
    public interface ITokenService
    {
       
        /// <param name="userId">Id del usuario (string)</param>
        /// <param name="email">Email del usuario</param>
        /// <param name="roles">Lista de roles</param>
        
        string CreateToken(string userId, string email, IEnumerable<string>? roles = null);

        DateTime GetExpiry();
    }
}
