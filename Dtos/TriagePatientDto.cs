namespace triage_backend.Dtos
{
    public class TriagePatientDto
    {
        public int TriageId { get; set; }
        public long PatientId { get; set; } // USUARIO.ID_Usuario
        public string Identification { get; set; } = string.Empty; // USUARIO.Cedula_Us
        public string FullName { get; set; } = string.Empty;     // Nombre + Apellido

        public string Gender { get; set; } = string.Empty; // USUARIO.Sexo_Us
        public int Age { get; set; } // Calculado desde USUARIO.Fecha_Nac_Us

        public string Symptoms { get; set; } = string.Empty; // TRIAGE.Sintomas
        public decimal Temperature { get; set; } // TRIAGE.Temperatura
        public int HeartRate { get; set; } // TRIAGE.Frecuencia_Card
        public string BloodPressure { get; set; } = string.Empty; // TRIAGE.Presion_Arterial
        public int RespiratoryRate { get; set; } // TRIAGE.Frecuencia_Res
        public int OxygenSaturation { get; set; } // TRIAGE.Oxigenacion
        public string PriorityName { get; set; } = string.Empty; // PRIORIDAD.Nombre_Prio

    }
}
