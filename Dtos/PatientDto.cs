namespace triage_backend.Dtos
{
    public class PatientDto
    {
        public string DocumentIdPt { get; set; } = string.Empty;   // obligatorio
        public string FirstNamePt { get; set; } = string.Empty;    // obligatorio
        public string LastNamePt { get; set; } = string.Empty;     // obligatorio
        public DateTime BirthDatePt { get; set; }                  // obligatorio
        public bool GenderPt { get; set; }                         // obligatorio (false=Femenino, true=Masculino)
        public string EmailPt { get; set; } = string.Empty;        // obligatorio
        public string PhonePt { get; set; } = string.Empty;        // obligatorio

        // Opcionales
        public string? EmergencyContactPt { get; set; }
        public string? AddressPt { get; set; }
    }
}
