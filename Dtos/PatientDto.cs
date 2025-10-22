namespace triage_backend.Dtos
{
    public class PatientDto
    {
        public string DocumentIdPt { get; set; } = string.Empty;
        public string FirstNamePt { get; set; } = string.Empty;
        public string LastNamePt { get; set; } = string.Empty;
        public DateTime BirthDatePt { get; set; }
        public bool GenderPt { get; set; }
        public string EmailPt { get; set; } = string.Empty;
        public string PhonePt { get; set; } = string.Empty;
        public string? EmergencyContactPt { get; set; }
        public string? AddressPt { get; set; }

        //  Propiedad calculada
        public int Age
        {
            get
            {
                var today = DateTime.Today;
                var age = today.Year - BirthDatePt.Year;
                if (BirthDatePt.Date > today.AddYears(-age)) age--;
                return age;
            }
        }
    }
}
