using triage_backend.Dtos;

namespace triage_backend.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(AutenticationDto user);
    }
}
