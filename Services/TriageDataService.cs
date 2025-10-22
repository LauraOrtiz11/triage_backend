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
            int ID_Priority = MapPriorityToId(prediction.SuggestedLevel);

            // 🟢 Estado fijo: 1
            int ID_State = 1;

            // Guardar el registro en la base de datos
            var triageId = await _triageRepository.InsertTriageAsync(
                request,
                prediction.SuggestedLevel,
                request.ID_Patient,
                request.ID_Doctor ?? 0,
                request.ID_Nurse,
                ID_Priority,   
                ID_State,      
                request.PatientAge 
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
