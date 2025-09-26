using triage_backend.Dtos;

namespace triage_backend.Services
{
    public interface IUserService
    {
        object CreateUser(UserDto userDto);

        object ChangeUserStatus(int userId, int stateId);

    }
}
