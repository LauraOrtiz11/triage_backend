using triage_backend.Dtos;
using triage_backend.Interfaces;
using triage_backend.Repositories;

namespace triage_backend.Services
{
    public class AutenticationService : IAutenticationService
    {
        private readonly AutenticationRepository _repository;

        public AutenticationService(AutenticationRepository repository)
        {
            _repository = repository;
        }

        public AutenticationDto? GetByEmail(string email)
        {
            return _repository.GetByEmail(email);
        }
    }
}
