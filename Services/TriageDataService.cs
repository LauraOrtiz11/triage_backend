using triage_backend.Dtos;
using triage_backend.Interfaces;
using triage_backend.Repositories;

namespace triage_backend.Services
{
    public class TriageDataService
    {
        private readonly HuggingFaceService _huggingFaceService;
        private readonly ITriageRepository _triageRepository; // ✅ usar la interfaz aquí

        public TriageDataService(HuggingFaceService huggingFaceService, ITriageRepository triageRepository)
        {
            _huggingFaceService = huggingFaceService;
            _triageRepository = triageRepository;
        }

        public async Task<TriageResponseDto> ProcessTriageAsync(TriageRequestDto request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // Llamar al modelo de IA para obtener el nivel sugerido
            var prediction = await _huggingFaceService.GetTriagePredictionAsync(new TriageRequestDto
            {
                Symptoms = request.Symptoms,
                VitalSigns = request.VitalSigns
            });

            // 🧩 Mapear la prioridad a partir del color sugerido por la IA
            int idPrioridad = MapPriorityToId(prediction.SuggestedLevel);

            // 🟢 Estado fijo: 1
            int idEstado = 1;

            // Guardar el registro en la base de datos
            var triageId = await _triageRepository.InsertTriageAsync(
                request,
                prediction.SuggestedLevel,
                request.IdPaciente,
                request.IdMedico ?? 0,
                request.IdEnfermero,
                idPrioridad,
                idEstado
            );

            // Retornar resultado
            return new TriageResponseDto
            {
                IdTriage = triageId,
                SuggestedLevel = prediction.SuggestedLevel,
                Confidence = prediction.Confidence,
                Message = prediction.Message
            };
        }

        private int MapPriorityToId(string suggestedLevel)
        {
            if (string.IsNullOrWhiteSpace(suggestedLevel)) return 1;

            var level = suggestedLevel.Trim().ToLowerInvariant();

            return level switch
            {
                "azul" or "blue" => 1,
                "verde" or "green" => 2,
                "amarillo" or "yellow" => 3,
                "naranja" or "orange" => 4,
                "rojo" or "red" => 5,
                _ => 1
            };
        }

    }
}
