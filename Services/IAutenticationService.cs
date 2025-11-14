using triage_backend.Dtos;

namespace triage_backend.Interfaces
{
    public interface IAutenticationService
    {
        /// <summary>
        /// Obtiene un usuario mediante su correo electrónico.
        /// </summary>
        AutenticationDto? GetByEmail(string email);
    }
}
