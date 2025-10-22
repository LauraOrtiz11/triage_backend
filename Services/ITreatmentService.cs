using triage_backend.Dtos;

namespace triage_backend.Interfaces
{
    public interface ITreatmentService
    {
        /// <summary>
        /// Registra un tratamiento y lo asocia al diagnóstico indicado.
        /// </summary>
        /// <returns>true si todo salió bien; de lo contrario false.</returns>
        bool RegisterTreatment(TreatmentRequestDto request);
    }
}
