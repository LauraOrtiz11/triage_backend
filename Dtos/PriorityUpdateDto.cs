namespace triage_backend.Dtos
{
    public class PriorityUpdateDto
    {
        public int IdTriage { get; set; }
        public string? Turno { get; set; }
        public string? Prioridad { get; set; }
        public string? Color { get; set; }
        public string? Estado { get; set; }
        public DateTime? FechaRegistro { get; set; }
    }
}
