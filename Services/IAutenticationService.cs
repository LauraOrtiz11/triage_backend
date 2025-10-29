using triage_backend.Dtos;


namespace triage_backend.Services
{
    public interface IAutenticationService
    {

        //obtener usuario por email (necesario para login)
        AutenticationDto? GetByEmail(string email);

        AutenticationDto? GetById(int id);
    }
}
