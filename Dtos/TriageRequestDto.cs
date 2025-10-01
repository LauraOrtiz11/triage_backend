public class TriageRequestDto
{
    public VitalSignsDto VitalSigns { get; set; } = new VitalSignsDto();
    public string Symptoms { get; set; } = string.Empty;
}

public class VitalSignsDto
{
    public int HeartRate { get; set; }
    public int RespiratoryRate { get; set; }
    public string BloodPressure { get; set; } = string.Empty;
    public decimal Temperature { get; set; }
    public int OxygenSaturation { get; set; }
}
