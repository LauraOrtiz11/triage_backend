using triage_backend.Dtos;

namespace triage_backend.Services
{
    public interface IConsultationService
    {
        /// <summary>
        /// Inicia una consulta médica y devuelve el ID de la consulta creada.
        /// </summary>
        int StartConsultation(StartConsultationDto model);
    }
}
