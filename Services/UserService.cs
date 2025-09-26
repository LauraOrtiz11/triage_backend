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

        // Crear Usuario
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

            if (string.IsNullOrWhiteSpace(userDto.PasswordUs))
                return new { Success = false, Message = "La contraseña es obligatoria." };

            if (userDto.RoleIdUs <= 0)
                return new { Success = false, Message = "Debe seleccionar un rol válido." };

            if (userDto.StateIdUs <= 0)
                return new { Success = false, Message = "Debe seleccionar un estado válido." };

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
            string passwordHash = EncryptUtility.HashPassword(userDto.IdentificationUs);


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

        // Habilitar o deshabilitar usuario
        public object ChangeUserStatus(int userId, int newState)
        {
            // Validar dependencias activas
            bool hasProcesses = _userRepository.HasActiveProcesses(userId);

            if (hasProcesses && newState == 0) // Deshabilitar
            {
                return new
                {
                    Success = false,
                    Message = "No se puede deshabilitar al usuario porque tiene procesos activos asociados (ej: triages en curso)."
                };
            }

            // Cambiar estado
            bool updated = _userRepository.ChangeUserStatus(userId, newState);

            return updated
                ? new { Success = true, Message = newState == 1 ? "Usuario habilitado correctamente." : "Usuario deshabilitado correctamente." }
                : new { Success = false, Message = "No se pudo actualizar el estado del usuario." };
        }
    }
}
