using triage_backend.Dtos;

namespace triage_backend.Interfaces
{
    public interface IPriorityUpdateService
    {
        
        PatientStatusDto? GetPatientStatusByPatient(int patientId);

    }
}
