using triage_backend.Dtos;

namespace triage_backend.Services
{
    public interface IPatientService
    {
        object CreatePatient(PatientDto patientDto);
    }
}
