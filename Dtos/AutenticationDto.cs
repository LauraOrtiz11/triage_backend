namespace triage_backend.Dtos
{
    public class AutenticationDto
    {
        public int? IdUs { get; set; }                      // ID_Us (nullable: sólo existe después de crear)
        public string FirstNameUs { get; set; } = string.Empty;       // Nombre_Us
        public string LastNameUs { get; set; } = string.Empty;        // Apellido_Us
        public string EmailUs { get; set; } = string.Empty;           // Correo_Us

        // Para creación: PasswordUs (plain) -- NO sobrescribir con el hash
        public string PasswordUs { get; set; } = string.Empty;        // Contrasena_Us (entrada)

        // Para autenticación / lectura desde BD: PasswordHashUs (hash de bd)

        public string? PasswordHashUs { get; set; }                   // Contrasena_Us (hash guardado en BD)
        public string PhoneUs { get; set; } = string.Empty;           // Telefono_Us
        public DateTime CreationDateUs { get; set; }                  // Fecha_Creacion
        public string IdentificationUs { get; set; } = string.Empty;  // Cedula_Us
        public DateTime BirthDateUs { get; set; }                     // Fecha_Nac_Us
        public bool GenderUs { get; set; }                            // Sexo_Us
        public string EmergencyContactUs { get; set; } = string.Empty; // Contacto_Emer
        public string AddressUs { get; set; } = string.Empty;         // Direccion_Us
        public int RoleIdUs { get; set; }                             // (FK)ID_Rol
        public int StateIdUs { get; set; }                            // (FK)ID_Estado

        // Nombre del rol (este es el que vamos a usar en los claims)
        public string? RoleNameUs { get; set; }

        // Si soportas multiples roles
        public List<string>? Roles { get; set; }

    }
}
