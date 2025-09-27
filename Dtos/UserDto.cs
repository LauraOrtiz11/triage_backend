namespace triage_backend.Dtos
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string FirstNameUs { get; set; } = string.Empty;       // Nombre_Us
        public string LastNameUs { get; set; } = string.Empty;        // Apellido_Us
        public string EmailUs { get; set; } = string.Empty;           // Correo_Us
        public string PasswordUs { get; set; } = string.Empty;        // Contrasena_Us
        public string PhoneUs { get; set; } = string.Empty;           // Telefono_Us
        public DateTime CreationDateUs { get; set; }

        public string IdentificationUs { get; set; } = string.Empty;  // Cedula_Us
        public DateTime BirthDateUs { get; set; }                     // Fecha_Nac_Us
        public bool GenderUs { get; set; }                            // Sexo_Us
        public string EmergencyContactUs { get; set; } = string.Empty; // Contacto_Emer
        public string AddressUs { get; set; } = string.Empty;         // Direccion_Us
        public int RoleIdUs { get; set; }                             // (FK)ID_Rol
        public int StateIdUs { get; set; }                            // (FK)ID_Estado
    }
}
