using triage_backend.Dtos;
using triage_backend.Interfaces;
using triage_backend.Repositories;

namespace triage_backend.Services
{
    public class ExamService : IExamService
    {
        private readonly ExamRepository _repository;

        public ExamService(ExamRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<ExamDto> GetAllExams()
        {
            return _repository.GetAllExams();
        }

        public ExamDto? GetExamById(int id)
        {
            return _repository.GetExamById(id);
        }
    }
}
