namespace triage_backend.Dtos
{
    public class UserListDto
    {
        public int UserId { get; set; }

        public string FullName { get; set; } = string.Empty;     // Nombre + Apellido
        public string IdentificationUs { get; set; } = string.Empty; // Cédula
        public string EmailUs { get; set; } = string.Empty;          // Correo
        public string RoleName { get; set; } = string.Empty;         // Nombre del rol
        public string StateName { get; set; } = string.Empty;        // Nombre del estado
        public DateTime CreationDateUs { get; set; }                 // Fecha de creación
    }
}
