using triage_backend.Dtos;

namespace triage_backend.Services
{
    public interface IUserService
    {
        object CreateUser(UserDto userDto);

        ResponseDto ChangeUserStatus(int userId, int stateId);


        IEnumerable<UserListDto> GetUsers(string? searchTerm = null);

        (bool Success, string Message) UpdateUser(UserDto user);

        UserDto? GetUserById(int userId);

    }
}
