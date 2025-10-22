using triage_backend.Dtos;

namespace triage_backend.Interfaces
{
    public interface IMedicationService
    {
        /// <summary>
        /// Retorna la lista completa de medicamentos disponibles.
        /// </summary>
        IEnumerable<MedicationDto> GetAllMedications();

        /// <summary>
        /// Retorna la información de un medicamento según su ID.
        /// </summary>
        MedicationDto? GetMedicationById(int id);
    }
}
