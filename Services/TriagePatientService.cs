using System;
using System.Collections.Generic;
using triage_backend.Dtos;
using triage_backend.Repositories;
using triage_backend.Utilities;

namespace triage_backend.Services
{
    public class TriageService : ITriagePatientService
    {
        private readonly TriagePatientRepository _repository;

        public TriageService(ContextDB context)
        {
            _repository = new TriagePatientRepository(context);
        }

        public List<TriagePatientDto> GetTriagePatients(string? color)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(color))
                    color = color.ToLower().Trim();

                return _repository.GetTriagePatients(color);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while retrieving triage data: {ex.Message}");
            }
        }
    }
}
