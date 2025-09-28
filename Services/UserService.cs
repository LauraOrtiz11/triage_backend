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

        // CREAR USUARIO
        public object CreateUser(UserDto userDto)
        {

            // Validaciones
            if (string.IsNullOrWhiteSpace(userDto.FirstNameUs))
                return new { Success = false, Message = "El nombre es obligatorio." };

            if (string.IsNullOrWhiteSpace(userDto.LastNameUs))
                return new { Success = false, Message = "El apellido es obligatorio." };

            if (string.IsNullOrWhiteSpace(userDto.IdentificationUs))
                return new { Success = false, Message = "La cédula es obligatoria." };

            if (string.IsNullOrWhiteSpace(userDto.EmailUs))
                return new { Success = false, Message = "El correo es obligatorio." };

            if (!userDto.EmailUs.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
            {
                return new
                {
                    Success = false,
                    Message = "Solo se permiten correos con dominio @gmail.com."
                };
            }

            if (string.IsNullOrWhiteSpace(userDto.PasswordUs))
                return new { Success = false, Message = "La contraseña es obligatoria." };

            if (userDto.RoleIdUs <= 0)
                return new { Success = false, Message = "Debe seleccionar un rol válido." };

            if (userDto.StateIdUs < 0)
                return new { Success = false, Message = "Debe seleccionar un estado válido." };

            // Validar duplicados 
            bool exists = _userRepository.ExistsByIdentificationOrEmail(userDto.IdentificationUs, userDto.EmailUs);
            if (exists)
            {
                return new
                {
                    Success = false,
                    Message = "La Identificación o el correo ya esta identificado."
                };
            }

            // Encriptar contraseña 
            string passwordHash = EncryptUtility.HashPassword(userDto.IdentificationUs);


            // Insertar Usuario
            int newId = _userRepository.CreateUser(userDto, passwordHash);

            // Respuesta
            return new
            {
                Success = true,
                Message = "Usuario registrado exitosamente.",
                UserId = newId
            };
        }



        // OBTENER LISTA DE USUARIOS CON POR DEFECTO O POR FILTRO POR NOMBRE O CEDÚLA 
        public IEnumerable<UserListDto> GetUsers(string? searchTerm = null)
        {
            return _userRepository.GetUsers(searchTerm);
        }



        // LLAMAR DATOS PARA EDICIÓN 
        public UserDto? GetUserById(int userId) => _userRepository.GetUserById(userId);



        // ACTUALIZAR USUARIO
        public (bool Success, string Message) UpdateUser(UserDto user)
        {
            // Validaciones de negocio antes de actualizar
            if (string.IsNullOrWhiteSpace(user.FirstNameUs) || string.IsNullOrWhiteSpace(user.LastNameUs))
                return (false, "El nombre y apellido son obligatorios.");

            if (string.IsNullOrWhiteSpace(user.EmailUs))
                return (false, "El correo electrónico es obligatorio.");

            if (string.IsNullOrWhiteSpace(user.IdentificationUs))
                return (false, "La cédula es obligatoria.");

            // Llamar al repositorio que maneja duplicidad y actualización
            return _userRepository.UpdateUser(user);
        }



        // HABILITAR O DESHABILITAR USUARIO
        public ResponseDto ChangeUserStatus(int userId, int newState)
        {
            // Validar dependencias activas
            bool hasProcesses = _userRepository.HasActiveProcesses(userId);

            if (hasProcesses && newState == 0) // Deshabilitar
            {
                return new ResponseDto
                {
                    Success = false,
                    Message = "No se puede deshabilitar al usuario porque tiene procesos activos asociados."
                };
            }

            // Cambiar estado
            bool updated = _userRepository.ChangeUserStatus(userId, newState);

            return updated
                ? new ResponseDto
                {
                    Success = true,
                    Message = newState == 1
                        ? "Usuario habilitado correctamente."
                        : "Usuario deshabilitado correctamente."
                }
                : new ResponseDto
                {
                    Success = false,
                    Message = "No se pudo actualizar el estado del usuario."
                };
        }

    }
}
