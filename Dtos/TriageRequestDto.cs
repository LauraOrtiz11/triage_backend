public class TriageRequestDto
{
    public VitalSignsDto VitalSigns { get; set; } = new VitalSignsDto();
    public string Symptoms { get; set; } = string.Empty;
    public int ID_Patient { get; set; }
    public int? ID_Doctor { get; set; }
    public int ID_Nurse { get; set; }
    public int PatientAge { get; set; }

}

public class VitalSignsDto
{
    public int HeartRate { get; set; }
    public int RespiratoryRate { get; set; }
    public string BloodPressure { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public int OxygenSaturation { get; set; }
}
