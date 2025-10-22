using Microsoft.Data.SqlClient;
using triage_backend.Dtos;
using triage_backend.Utilities;

namespace triage_backend.Repositories
{
    public class PatientRepository
    {
        private readonly ContextDB _context;

        public PatientRepository(ContextDB context)
        {
            _context = context;
        }

        public bool ExistsByIdentificationOrEmail(string identification, string email)
        {
            using SqlConnection conn = (SqlConnection)_context.OpenConnection();
            string query = "SELECT COUNT(1) FROM USUARIO WHERE CEDULA_US = @Identification OR CORREO_US = @Email";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Identification", identification);
                cmd.Parameters.AddWithValue("@Email", email);

                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        public int CreatePatient(PatientDto patient, string passwordHash)
        {
            using SqlConnection conn = (SqlConnection)_context.OpenConnection();
            string query = @"
                INSERT INTO USUARIO
                (NOMBRE_US, APELLIDO_US, CORREO_US, CONTRASENA_US, TELEFONO_US, 
                 FECHA_CREACION, CEDULA_US, FECHA_NAC_US, SEXO_US, 
                 CONTACTO_EMER, DIRECCION_US, ID_ROL, ID_ESTADO)
                VALUES
                (@FirstName, @LastName, @Email, @Password, @Phone, 
                 GETDATE(), @Identification, @BirthDate, @Gender, 
                 @EmergencyContact, @Address, 3, 1);
                SELECT SCOPE_IDENTITY();";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@FirstName", patient.FirstNamePt);
                cmd.Parameters.AddWithValue("@LastName", patient.LastNamePt);
                cmd.Parameters.AddWithValue("@Email", patient.EmailPt);
                cmd.Parameters.AddWithValue("@Password", passwordHash);
                cmd.Parameters.AddWithValue("@Phone", patient.PhonePt);
                cmd.Parameters.AddWithValue("@Identification", patient.DocumentIdPt);
                cmd.Parameters.AddWithValue("@BirthDate", patient.BirthDatePt);
                cmd.Parameters.AddWithValue("@Gender", patient.GenderPt ? 1 : 0);
                cmd.Parameters.AddWithValue("@EmergencyContact", string.IsNullOrEmpty(patient.EmergencyContactPt) ? (object)DBNull.Value : patient.EmergencyContactPt);
                cmd.Parameters.AddWithValue("@Address", string.IsNullOrEmpty(patient.AddressPt) ? (object)DBNull.Value : patient.AddressPt);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        //Nuevo método para buscar por cédula
        public object? GetPatientByDocument(string documentId)
        {
            using SqlConnection conn = (SqlConnection)_context.OpenConnection();
            string query = @"
        SELECT 
            ID_USUARIO AS Id,
            NOMBRE_US AS Nombre,
            APELLIDO_US AS Apellido,
            FECHA_NAC_US AS FechaNacimiento
        FROM USUARIO
        WHERE CEDULA_US = @DocumentId";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@DocumentId", documentId);

                using SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    DateTime fechaNacimiento = Convert.ToDateTime(reader["FechaNacimiento"]);
                    int edad = DateTime.Today.Year - fechaNacimiento.Year;
                    if (fechaNacimiento.Date > DateTime.Today.AddYears(-edad))
                        edad--;

                    return new
                    {
                        Id = reader["Id"],
                        Nombre = reader["Nombre"].ToString(),
                        Apellido = reader["Apellido"].ToString(),
                        Edad = edad
                    };
                }
            }
            return null;
        }

    }
}
