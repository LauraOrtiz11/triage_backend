namespace triage_backend.Dtos
{
    public class TriageResultDto
    {
        public int TriageId { get; set; }          // ID del triage del paciente
        public int PriorityId { get; set; }        // Prioridad seleccionada por el enfermero
        public int NurseId { get; set; }           // ID del enfermero que realizó la validación
        public bool IsFinalPriority { get; set; }  // Indica si esta es la prioridad final
    }
}
