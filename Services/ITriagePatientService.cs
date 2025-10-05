using System.Collections.Generic;
using triage_backend.Dtos;

namespace triage_backend.Services
{
    public interface ITriagePatientService
    {
        List<TriagePatientDto> GetTriagePatients(string? color);
    }
}
