namespace triage_backend.Dtos
{
    public class AutenticationDto
    {
        public int? IdUs { get; set; }
        public string FirstNameUs { get; set; } = string.Empty;
        public string LastNameUs { get; set; } = string.Empty;
        public string EmailUs { get; set; } = string.Empty;

        public string PasswordUs { get; set; } = string.Empty;
        public string? PasswordHashUs { get; set; }

        public string PhoneUs { get; set; } = string.Empty;
        public DateTime CreationDateUs { get; set; }
        public string IdentificationUs { get; set; } = string.Empty;
        public DateTime BirthDateUs { get; set; }
        public bool GenderUs { get; set; }
        public string EmergencyContactUs { get; set; } = string.Empty;
        public string AddressUs { get; set; } = string.Empty;

        public int RoleIdUs { get; set; }
        public int StateIdUs { get; set; }

        public string? RoleNameUs { get; set; }
        public List<string>? Roles { get; set; }

        public string RealRoleName =>
            RoleIdUs switch
            {
                1 => "Administrador",
                2 => "Enfermero",
                3 => "Paciente",
                4 => "Médico",
                _ => "Desconocido"
            };
    }
}
