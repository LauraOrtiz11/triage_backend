namespace triage_backend.Dtos
{
    public class TriagePatientDto
    {
        public long PatientId { get; set; } // USUARIO.ID_Usuario
        public string Identification { get; set; } = string.Empty; // USUARIO.Cedula_Us
        public string FirstName { get; set; } = string.Empty; // USUARIO.Nombre_Us
        public string LastName { get; set; } = string.Empty; // USUARIO.Apellido_Us
        public string Gender { get; set; } = string.Empty; // USUARIO.Sexo_Us
        public int Age { get; set; } // Calculado desde USUARIO.Fecha_Nac_Us
        public long TriageId { get; set; } // TRIAGE.ID_Triage
        public DateTime RegistrationDate { get; set; } // TRIAGE.Fecha_Registro
        public string Symptoms { get; set; } = string.Empty; // TRIAGE.Sintomas
        public decimal Temperature { get; set; } // TRIAGE.Temperatura
        public int HeartRate { get; set; } // TRIAGE.Frecuencia_Card
        public string BloodPressure { get; set; } = string.Empty; // TRIAGE.Presion_Arterial
        public int RespiratoryRate { get; set; } // TRIAGE.Frecuencia_Res
        public int OxygenSaturation { get; set; } // TRIAGE.Oxigenacion
        public string PriorityName { get; set; } = string.Empty; // PRIORIDAD.Nombre_Prio
        public string PriorityColor { get; set; } = string.Empty; // PRIORIDAD.Color_Prio
        public string AssignedDoctorName { get; set; } = string.Empty; // USUARIO.Nombre_Us (TRIAGE.ID_Medico)
    }
}
