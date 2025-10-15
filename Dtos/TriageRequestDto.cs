public class TriageRequestDto
{
    public VitalSignsDto VitalSigns { get; set; } = new VitalSignsDto();
    public string Symptoms { get; set; } = string.Empty;
    public int IdPaciente { get; set; }
    public int? IdMedico { get; set; }
    public int IdEnfermero { get; set; }
    
}

public class VitalSignsDto
{
    public int HeartRate { get; set; }
    public int RespiratoryRate { get; set; }
    public string BloodPressure { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public int OxygenSaturation { get; set; }
}
