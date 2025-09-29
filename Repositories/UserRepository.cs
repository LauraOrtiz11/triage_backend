using triage_backend.Utilities;
using triage_backend.Dtos;
using Microsoft.Data.SqlClient;

namespace triage_backend.Repositories
{
    public class UserRepository(ContextDB context)
    {
        private readonly ContextDB _context = context;


        // CREAR USUARIO 
        public int CreateUser(UserDto user, string passwordHash)
        {
            using (SqlConnection conn = (SqlConnection)_context.OpenConnection())
            {
                string query = @"
                    INSERT INTO USUARIO 
                    (Nombre_Us, Apellido_Us, Correo_Us, Contrasena_Us, Telefono_Us, Cedula_Us, 
                     Fecha_Nac_Us, Sexo_Us, Contacto_Emer, Direccion_Us, ID_Rol, ID_Estado)
                    VALUES 
                    (@FirstName, @LastName, @Email, @Password, @Phone, @Identification, 
                     @BirthDate, @Gender, @EmergencyContact, @Address, @RoleId, @StateId);
                    SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FirstName", user.FirstNameUs);
                    cmd.Parameters.AddWithValue("@LastName", user.LastNameUs);
                    cmd.Parameters.AddWithValue("@Email", user.EmailUs);
                    cmd.Parameters.AddWithValue("@Password", passwordHash);
                    cmd.Parameters.AddWithValue("@Phone", string.IsNullOrEmpty(user.PhoneUs) ? (object)DBNull.Value : user.PhoneUs);
                    cmd.Parameters.AddWithValue("@Identification", user.IdentificationUs);
                    cmd.Parameters.AddWithValue("@BirthDate", user.BirthDateUs == default ? (object)DBNull.Value : user.BirthDateUs);
                    cmd.Parameters.AddWithValue("@Gender", user.GenderUs);
                    cmd.Parameters.AddWithValue("@EmergencyContact", string.IsNullOrEmpty(user.EmergencyContactUs) ? (object)DBNull.Value : user.EmergencyContactUs);
                    cmd.Parameters.AddWithValue("@Address", string.IsNullOrEmpty(user.AddressUs) ? (object)DBNull.Value : user.AddressUs);
                    cmd.Parameters.AddWithValue("@RoleId", user.RoleIdUs);
                    cmd.Parameters.AddWithValue("@StateId", user.StateIdUs);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }


        // REVISAR EXISTENCIA DE CEDÚLA O CORREO 
        public bool ExistsByIdentificationOrEmail(string identification, string email)
        {
            using SqlConnection conn = (SqlConnection)_context.OpenConnection();
            string query = "SELECT COUNT(1) FROM USUARIO WHERE Cedula_Us = @Identification OR Correo_Us = @Email";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Identification", identification);
                cmd.Parameters.AddWithValue("@Email", email);

                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }



        // LISTAR USUARIOS 
        public IEnumerable<UserListDto> GetUsers(string? searchTerm = null)
        {
            var users = new List<UserListDto>();
            
            string query = @"
                SELECT 
                    U.ID_Usuario AS UserId,
                    (U.Nombre_Us + ' ' + U.Apellido_Us) AS FullName,
                    U.Cedula_Us AS IdentificationUs,
                    U.Correo_Us AS EmailUs,
                    R.Nombre_Rol AS RoleName,
                    E.Nombre_Est AS StateName,
                    U.Fecha_Creacion AS CreationDateUs,
                    CASE WHEN U.Sexo_Us = 1 THEN 'Masculino' ELSE 'Femenino' END AS GenderName
                FROM USUARIO U
                INNER JOIN ROL R ON U.ID_Rol = R.ID_Rol
                INNER JOIN ESTADO E ON U.ID_Estado = E.ID_Estado
                WHERE (@SearchTerm IS NULL 
                       OR U.Cedula_Us LIKE '%' + @SearchTerm + '%'
                       OR U.Nombre_Us LIKE '%' + @SearchTerm + '%'
                       OR U.Apellido_Us LIKE '%' + @SearchTerm + '%')
                ORDER BY U.Fecha_Creacion DESC;
                ";

            using (SqlConnection conn = (SqlConnection)_context.OpenConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", string.IsNullOrEmpty(searchTerm) ? "" : searchTerm);


                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    users.Add(new UserListDto
                    {
                        UserId = Convert.ToInt32(reader["UserId"]),
                        FullName = reader["FullName"] as string ?? string.Empty,
                        IdentificationUs = reader["IdentificationUs"] as string ?? string.Empty,
                        EmailUs = reader["EmailUs"] as string ?? string.Empty,
                        RoleName = reader["RoleName"] as string ?? string.Empty,
                        StateName = reader["StateName"] as string ?? string.Empty,
                        CreationDateUs = reader["CreationDateUs"] == DBNull.Value
                        ? DateTime.MinValue
                        : Convert.ToDateTime(reader["CreationDateUs"]),

                    });
                }
            }

            return users;
        }



        // OBTENER USUARIO POR ID (PRECARGA PARA EDITAR)
        public UserDto? GetUserById(int userId)
        {
            using (SqlConnection conn = (SqlConnection)_context.OpenConnection())
            {
                string query = @"
            SELECT ID_Usuario, Nombre_Us, Apellido_Us, Telefono_Us,Cedula_Us, Correo_Us, Sexo_Us, 
                Contacto_Emer, Direccion_Us, Fecha_Nac_Us, ID_Rol, ID_Estado
            FROM USUARIO
            WHERE ID_Usuario = @UserId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new UserDto
                            {
                                UserId = reader["ID_Usuario"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ID_Usuario"]),
                                FirstNameUs = reader["Nombre_Us"] as string ?? string.Empty,
                                LastNameUs = reader["Apellido_Us"] as string ?? string.Empty,
                                PhoneUs = reader["Telefono_US"] as string ?? string.Empty,
                                IdentificationUs = reader["Cedula_Us"] as string ?? string.Empty,
                                EmailUs = reader["Correo_Us"] as string ?? string.Empty,
                                GenderUs = reader["Sexo_Us"] == DBNull.Value ? false : Convert.ToBoolean(reader["Sexo_Us"]),
                                EmergencyContactUs = reader["Contacto_Emer"] as string?? string.Empty,
                                AddressUs = reader["Direccion_Us"] as string?? string.Empty,
                                BirthDateUs = reader["Fecha_Nac_Us"] == DBNull.Value ? default : Convert.ToDateTime(reader["Fecha_Nac_Us"]),
                                RoleIdUs = reader["ID_Rol"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ID_Rol"]),
                                StateIdUs = reader["ID_Estado"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ID_Estado"])


                            };
                        }
                    }
                }
            }
            return null;
        }



        // VERIFICAR PROCESOS ACTIVOS ANTES DE DESHABILITAR
        public bool HasActiveProcesses(int userId)
        {
            using SqlConnection conn = (SqlConnection)_context.OpenConnection();

            string query = @"
                SELECT COUNT(1)
                FROM TRIAGE T
                INNER JOIN USUARIO U ON U.ID_Usuario = T.ID_Medico OR U.ID_Usuario = T.ID_Paciente
                WHERE U.ID_Usuario = @UserId AND T.ID_Estado = 1; -- Activo
            ";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);

            int count = (int)cmd.ExecuteScalar();
            return count > 0;
        }



        // ACTUALIZAR USUARIO 
        public (bool Success, string Message) UpdateUser(UserDto user)
        {
            using (SqlConnection conn = (SqlConnection)_context.OpenConnection())
            {
                // Verificar duplicados excluyendo al mismo usuario
                string checkQuery = @"
            SELECT COUNT(1)
            FROM USUARIO
            WHERE (Cedula_Us = @Identification OR Correo_Us = @Email)
              AND ID_Usuario <> @UserId";

                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@Identification", user.IdentificationUs);
                    checkCmd.Parameters.AddWithValue("@Email", user.EmailUs);
                    checkCmd.Parameters.AddWithValue("@UserId", user.UserId);

                    int duplicates = (int)checkCmd.ExecuteScalar();
                    if (duplicates > 0)
                    {
                        return (false, "Ya existe un usuario con el mismo correo o cédula.");
                    }
                }

                // Actualizar datos
                string updateQuery = @"
            UPDATE USUARIO
            SET Nombre_Us = @FirstName,
                Apellido_Us = @LastName,
                Cedula_Us = @Identification,
                Correo_Us = @Email,
                Sexo_Us = @Gender,
                Fecha_Nac_Us = @BirthDate,
                ID_Rol = @RoleId,
                ID_Estado = @StateId
            WHERE ID_Usuario = @UserId";

                using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                {
                    updateCmd.Parameters.AddWithValue("@FirstName", user.FirstNameUs);
                    updateCmd.Parameters.AddWithValue("@LastName", user.LastNameUs);
                    updateCmd.Parameters.AddWithValue("@Identification", user.IdentificationUs);
                    updateCmd.Parameters.AddWithValue("@Email", user.EmailUs);
                    updateCmd.Parameters.AddWithValue("@Gender", user.GenderUs);
                    updateCmd.Parameters.AddWithValue("@BirthDate", user.BirthDateUs == default ? (object)DBNull.Value : user.BirthDateUs);
                    updateCmd.Parameters.AddWithValue("@RoleId", user.RoleIdUs);
                    updateCmd.Parameters.AddWithValue("@StateId", user.StateIdUs);
                    updateCmd.Parameters.AddWithValue("@UserId", user.UserId);

                    int rowsAffected = updateCmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                        return (true, "La información del usuario se actualizó exitosamente.");
                    else
                        return (false, "No se encontró el usuario o no se realizaron cambios.");
                }
            }
        }



        // CAMBIAR EL ESTDO DEL USUARIO  (0 = Inactivo, 1 = Activo)
        public bool ChangeUserStatus(int userId, int stateId)
        {
            using (SqlConnection conn = (SqlConnection)_context.OpenConnection())
            {
                string query = @"UPDATE USUARIO 
                         SET ID_Estado = @StateId
                         WHERE ID_Usuario = @UserId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@StateId", stateId);
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }
    }
}
