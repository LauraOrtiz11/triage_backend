using triage_backend.Utilities;
using triage_backend.Dtos;
using Microsoft.Data.SqlClient;

namespace triage_backend.Repositories
{
    public class AutenticationRepository(ContextDB context)
    {
        private readonly ContextDB _context = context;

        // Revisar si ya existe cedúla y correo
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

        // Crear Usuario 
        public int CreateUser(AutenticationDto user, string passwordHash)
        {
            using (SqlConnection conn = (SqlConnection)_context.OpenConnection())
            {
                string query = @"
                    INSERT INTO USUARIO 
                    (Nombre_Us, Apellido_Us, Correo_Us, Contrasena_Us, Telefono_Us, Fecha_Creacion, Cedula_Us, 
                     Fecha_Nac_Us, Sexo_Us, Contacto_Emer, Direccion_Us, ID_Rol, ID_Estado)
                    VALUES 
                    (@FirstName, @LastName, @Email, @Password, @Phone, GETDATE(), @Identification, 
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



        public AutenticationDto? GetByEmail(string email)
        {
            using SqlConnection conn = (SqlConnection)_context.OpenConnection();

            string query = @"
    SELECT 
        u.ID_USUARIO,
        u.CORREO_US,
        u.CONTRASENA_US,
        u.ID_ROL,
        u.NOMBRE_US,
        u.APELLIDO_US,
        r.NOMBRE_ROL
    FROM USUARIO u
    LEFT JOIN ROL r ON r.ID_Rol = u.ID_ROL
    WHERE u.CORREO_US = @Email
    ";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Email", email);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var dto = new AutenticationDto();

                        // uso GetOrdinal para evitar depender de índices
                        int ordId = reader.GetOrdinal("ID_USUARIO");
                        int ordEmail = reader.GetOrdinal("CORREO_US");
                        int ordPass = reader.GetOrdinal("CONTRASENA_US");
                        int ordRole = reader.GetOrdinal("ID_ROL");
                        int ordName = reader.GetOrdinal("NOMBRE_US");
                        int ordLast = reader.GetOrdinal("APELLIDO_US");
                        int ordRoleName = reader.GetOrdinal("NOMBRE_ROL");

                        dto.IdUs = reader.IsDBNull(ordId) ? (int?)null : reader.GetInt32(ordId);
                        dto.EmailUs = reader.IsDBNull(ordEmail) ? string.Empty : reader.GetString(ordEmail);
                        dto.PasswordHashUs = reader.IsDBNull(ordPass) ? string.Empty : reader.GetString(ordPass);
                        dto.RoleIdUs = reader.IsDBNull(ordRole) ? 0 : reader.GetInt32(ordRole);
                        dto.FirstNameUs = reader.IsDBNull(ordName) ? string.Empty : reader.GetString(ordName);
                        dto.LastNameUs = reader.IsDBNull(ordLast) ? string.Empty : reader.GetString(ordLast);
                        dto.RoleNameUs = reader.IsDBNull(ordRoleName) ? null : reader.GetString(ordRoleName);

                        // Rellenar Roles con el nombre de rol (si existe) — así token tendrá ClaimTypes.Role con nombre.
                        if (!string.IsNullOrEmpty(dto.RoleNameUs))
                        {
                            dto.Roles = new List<string> { dto.RoleNameUs };
                        }
                        else
                        {
                            // fallback: si no hay nombre, usar id como string (temporal)
                            dto.Roles = new List<string> { dto.RoleIdUs.ToString() };
                        }


                        return dto;
                    }
                }
            }

            return null;
        }




    }


}

