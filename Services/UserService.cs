using triage_backend.Dtos;
using triage_backend.Repositories;
using triage_backend.Utilities;

namespace triage_backend.Services
{
    public class UserService : IUserService
    {
        private readonly UserRepository _userRepository;

        public UserService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public object CreateUser(UserDto userDto)
        {
            // 1. Validar duplicados 
            bool exists = _userRepository.ExistsByIdentificationOrEmail(userDto.IdentificationUs, userDto.EmailUs);
            if (exists)
            {
                return new
                {
                    Success = false,
                    Message = "La Identificación o el correo ya esta identificado."
                };
            }

            // 2. Encriptar contraseña 
            string passwordHash = EncryptUtility.HashPassword(userDto.PasswordUs);


            // 3. Insertar Usuario
            int newId = _userRepository.CreateUser(userDto, passwordHash);

            // 4. Respuesta
            return new
            {
                Success = true,
                Message = "Usuario registrado exitosamente.",
                UserId = newId
            };
        }
        // método para login (busca usuario por email y retorna un User simple) 
        public UserDto? GetByEmail(string email)
        {
            return _userRepository.GetByEmail(email);
        }
    }
}

