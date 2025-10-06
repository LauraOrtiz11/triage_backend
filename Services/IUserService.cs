using triage_backend.Dtos;


namespace triage_backend.Services
{
    public interface IUserService
    {
        object CreateUser(UserDto userDto);

        //obtener usuario por email (necesario para login)
        UserDto? GetByEmail(string email);
    }
}
