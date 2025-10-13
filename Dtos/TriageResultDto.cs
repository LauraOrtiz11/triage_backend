namespace triage_backend.Dtos
{
    public class TriageResultPatientInfoDto
    {
        public string FullName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Symptoms { get; set; } = string.Empty;
        public string VitalSigns { get; set; } = string.Empty;
        public string PriorityName { get; set; } = string.Empty;
    }

    public class TriageResultPatientRequestDto
    {
        public int triageId { get; set; }
    }

    public class TriageResultDto
    {
        public int TriageId { get; set; }          // ID del triage del paciente
        public int PriorityId { get; set; }        // Prioridad seleccionada por el enfermero
        public int NurseId { get; set; }           // ID del enfermero que realizó la validación
        public bool IsFinalPriority { get; set; }  // Indica si esta es la prioridad final
    }

    public class TriagePriorityInfoDto
    {
        public string PriorityName { get; set; } = string.Empty;
        public string PriorityDescription { get; set; } = string.Empty;
    }

    public class PriorityInfoDto
    {
        public int PriorityId { get; set; }
        public string PriorityName { get; set; } = string.Empty;
        public string PriorityDescription { get; set; } = string.Empty;
    }

}
