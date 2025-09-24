using triage_backend.Utilities;
using triage_backend.Dtos;
using Microsoft.Data.SqlClient;

namespace triage_backend.Repositories
{
    public class UserRepository(ContextDB context)
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
        public int CreateUser(UserDto user, string passwordHash)
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


    }
}
