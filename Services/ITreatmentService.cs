using triage_backend.Dtos;

namespace triage_backend.Interfaces
{
    public interface ITreatmentService
    {
        /// <summary>
        /// Registra un nuevo tratamiento asociado a un historial médico.
        /// </summary>
        /// <param name="request">Datos del tratamiento</param>
        /// <returns>ID del tratamiento creado, o 0 si ocurre un error</returns>
        int RegisterTreatment(TreatmentRequestDto request);
    }
}