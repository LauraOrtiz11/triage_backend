namespace triage_backend.Interfaces
{
    public interface ITriageRepository
    {
        Task<int> InsertTriageAsync(
         TriageRequestDto request,
         string suggestedLevel,
         int idPaciente,
         int idMedico,
         int idEnfermero,
         int idPrioridad,
         int idEstado);

    }
}