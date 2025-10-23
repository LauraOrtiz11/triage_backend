using triage_backend.Dtos;

namespace triage_backend.Interfaces
{
    public interface IExamService
    {
        /// <summary>
        /// Obtiene todos los exámenes disponibles en el sistema.
        /// </summary>
        IEnumerable<ExamDto> GetAllExams();

        /// <summary>
        /// Obtiene la información de un examen según su ID.
        /// </summary>
        ExamDto? GetExamById(int id);
    }
}
