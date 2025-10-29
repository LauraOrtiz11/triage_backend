using triage_backend.Dtos;
using triage_backend.Repositories;
using triage_backend.Utilities;

namespace triage_backend.Services
{
    public class AutenticationService : IAutenticationService
    {
        private readonly AutenticationRepository _autenticationRepository;

        public AutenticationService(AutenticationRepository autenticationRepository)
        {
            _autenticationRepository = autenticationRepository;
        }

        // método para login (busca usuario por email y retorna un User simple) 
        public AutenticationDto? GetByEmail(string email)
        {
            return _autenticationRepository.GetByEmail(email);
        }

        // 👇 ESTE método te falta (por eso el error)
        public AutenticationDto? GetById(int id)
        {
            return _autenticationRepository.GetById(id);
        }
    }
}

