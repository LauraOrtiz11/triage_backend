using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using triage_backend.Dtos;
using triage_backend.Interfaces;

namespace triage_backend.Services
{
    public class HuggingFaceService : IHuggingFaceService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiToken;
        private readonly string _apiUrlClassifier = "https://api-inference.huggingface.co/models/facebook/bart-large-mnli";
        private readonly string _apiUrlTranslator = "https://api-inference.huggingface.co/models/Helsinki-NLP/opus-mt-es-en";

        public HuggingFaceService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiToken = configuration["HuggingFace:ApiToken"]
                        ?? throw new System.Exception("HuggingFace API Token not configured.");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiToken}");
        }

        public async Task<TriageResponseDto> GetTriagePredictionAsync(TriageRequestDto request)
        {
            // 1. Traducir los síntomas a inglés antes de construir el prompt
            var translatedSymptoms = await TranslateToEnglishAsync(request.Symptoms);

            // 2. Construir el prompt en inglés con síntomas traducidos
            var inputText =
            "You are an experienced triage nurse in an emergency department. " +
            "You must classify urgency strictly as one of: blue, green, yellow, orange, red. " +
            "Guidelines: " +
            "Blue = mild conditions like cold, mild sore throat, headache. " +
            "Green = moderate, non-life-threatening but need attention (e.g. stable fracture, mild asthma). " +
            "Yellow = needs rapid treatment, may complicate (e.g. pneumonia with fever). " +
            "Orange = urgent, can escalate quickly (e.g. severe chest pain, shortness of breath). " +
            "Red = critical, immediate intervention (e.g. cardiac arrest, shock). " +
            "Always prioritize SYMPTOMS but escalate if VITAL SIGNS are abnormal. " +
            "Examples: " +
            "Case: sore throat, vitals normal → blue. " +
            "Case: ankle sprain, vitals normal → green. " +
            "Case: pneumonia + fever 38.5, SpO2 93% → yellow. " +
            "Case: chest pain severe, HR 120, BP 150/95 → orange. " +
            "Case: cardiac arrest, no pulse → red. " +
            $"Patient case: Symptoms: {translatedSymptoms}. " +
            $"Vital Signs: HR {request.VitalSigns.HeartRate}, " +
            $"RR {request.VitalSigns.RespiratoryRate}, " +
            $"BP {request.VitalSigns.BloodPressure}, " +
            $"Temp {request.VitalSigns.Temperature}, " +
            $"SpO₂ {request.VitalSigns.OxygenSaturation}.";

            var candidateLabels = new[] { "blue", "green", "yellow", "orange", "red" };

            var payload = new
            {
                inputs = inputText,
                parameters = new
                {
                    candidate_labels = candidateLabels
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_apiUrlClassifier, content);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(json);

            var label = document.RootElement.GetProperty("labels")[0].GetString() ?? "undetermined";

            var score = document.RootElement.GetProperty("scores")[0].GetDecimal();

            // Reglas médicas adicionales (post-procesamiento)
            label = ApplyMedicalRules(label, request);

            return new TriageResponseDto
            {
                SuggestedLevel = label ?? "undetermined",
                Confidence = System.Math.Round(score, 2),
                Message = GetTriageMessage(label)
            };
        }

        /// <summary>
        /// Llama a HuggingFace para traducir español → inglés.
        /// </summary>
        private async Task<string> TranslateToEnglishAsync(string spanishText)
        {
            var payload = new { inputs = spanishText };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_apiUrlTranslator, content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(json);

            return document.RootElement[0].GetProperty("translation_text").GetString();
        }

        private string ApplyMedicalRules(string predictedLabel, TriageRequestDto request)
        {
            var hr = request.VitalSigns.HeartRate;
            var rr = request.VitalSigns.RespiratoryRate;
            var temp = request.VitalSigns.Temperature;
            var spo2 = request.VitalSigns.OxygenSaturation;

            // Emergencia vital
            if (spo2 < 85 || hr < 40 || hr > 150 || rr > 35)
                return "red";

            // Urgencia severa
            if (spo2 < 90 || temp > 39.5 || hr > 130 || rr > 28)
                return "orange";

            // Urgencia moderada
            if ((temp >= 38 && temp <= 39.5) || (hr > 110 && hr <= 130) || (rr > 22 && rr <= 28))
                return "yellow";

            // Normal / Asintomático: todo en rango óptimo
            if ((hr >= 70 && hr <= 100) && (rr >= 14 && rr <= 18) && (temp >= 36.5 && temp <= 37.5) && (spo2 >= 97 && spo2 <= 100))
                return "blue";

            // Estable con parámetros ligeramente alterados pero seguros
            if ((hr >= 60 && hr <= 110) && (rr >= 12 && rr <= 22) && (temp >= 36 && temp <= 37.9) && (spo2 >= 94 && spo2 <= 100))
                return "green";


            // Si no entra en ninguna regla clara, dejamos el label del modelo
            return predictedLabel;
        }


        private string GetTriageMessage(string level)
        {
            return level switch
            {
                "blue" => "Urgencias leves que no comprometen el estado general ni amenazan la vida.",
                "green" => "Urgencias sin compromiso vital inmediato, pero podrían complicarse si no hay atención.",
                "yellow" => "Urgencias que requieren exámenes y/o tratamiento rápido para evitar complicaciones.",
                "orange" => "Urgencias graves que pueden escalar rápidamente a compromiso vital.",
                "red" => "Urgencia crítica: atención inmediata y posible reanimación.",
                _ => "Nivel no determinado."
            };
        }
    }
}
