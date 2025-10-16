namespace triage_backend.Interfaces
{
    public interface ITriageRepository
    {
        Task<int> InsertTriageAsync(
         TriageRequestDto request,
         string suggestedLevel,
         int ID_Patient,
         int ID_Doctor,
         int ID_Nurse,
         int ID_Priority,
         int ID_State,
         int PatientAge);

    }
}