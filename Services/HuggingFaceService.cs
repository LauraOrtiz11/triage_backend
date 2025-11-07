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
        private readonly string _apiUrlClassifier = "https://router.huggingface.co/hf-inference/models/facebook/bart-large-mnli";
        private readonly string _apiUrlTranslator = "https://router.huggingface.co/hf-inference/models/Helsinki-NLP/opus-mt-es-en";

        public HuggingFaceService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiToken = configuration["HuggingFace:ApiToken"]
                        ?? throw new System.Exception("HuggingFace API Token not configured.");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiToken}");
        }

        public async Task<TriageResponseDto> GetTriagePredictionAsync(TriageRequestDto request)
        {
            // 1️⃣ Traducir los síntomas
            var translatedSymptoms = await TranslateToEnglishAsync(request.Symptoms);

            // 2️⃣ Construir el prompt
            var inputText =
                "You are an experienced triage nurse in an emergency department. " +
                "Classify urgency strictly as one of: blue, green, yellow, orange, red. " +
                "Guidelines: " +
                "Blue = mild conditions like cold, mild sore throat, headache. " +
                "Green = moderate, non-life-threatening (e.g. stable fracture, mild asthma). " +
                "Yellow = needs rapid treatment (e.g. pneumonia with fever). " +
                "Orange = urgent, can escalate quickly (e.g. severe chest pain). " +
                "Red = critical, immediate intervention (e.g. cardiac arrest). " +
                $"Patient case: Symptoms: {translatedSymptoms}. " +
                $"Vital Signs: HR {request.VitalSigns.HeartRate}, " +
                $"RR {request.VitalSigns.RespiratoryRate}, " +
                $"BP {request.VitalSigns.BloodPressure}, " +
                $"Temp {request.VitalSigns.Temperature}, " +
                $"SpO₂ {request.VitalSigns.OxygenSaturation}.";

            var payload = new
            {
                inputs = inputText,
                parameters = new
                {
                    candidate_labels = new[] { "blue", "green", "yellow", "orange", "red" }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_apiUrlClassifier, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                throw new System.Exception($"Error llamando a HuggingFace: {response.StatusCode} - {errorText}");
            }

            var json = await response.Content.ReadAsStringAsync();


            using var document = JsonDocument.Parse(json);

            if (document.RootElement.ValueKind != JsonValueKind.Array || document.RootElement.GetArrayLength() == 0)
                throw new System.Exception($"Respuesta inesperada de HuggingFace: {json}");

            var result = document.RootElement[0];

            
            // Nuevo formato: array de objetos { label, score }
            if (document.RootElement.ValueKind != JsonValueKind.Array || document.RootElement.GetArrayLength() == 0)
                throw new System.Exception($"Respuesta inesperada de HuggingFace: {json}");

            // Tomar el label con mayor score
            var bestResult = document.RootElement[0];
            var label = bestResult.GetProperty("label").GetString() ?? "undetermined";
            var score = bestResult.GetProperty("score").GetDecimal();


            // 3️⃣ Aplicar reglas médicas adicionales
            label = ApplyMedicalRules(label, request);

            return new TriageResponseDto
            {
                SuggestedLevel = label,
                Confidence = System.Math.Round(score, 2),
                Message = GetTriageMessage(label)
            };
        }

        /// <summary>
        /// Traduce texto de español a inglés usando HuggingFace.
        /// </summary>
        private async Task<string> TranslateToEnglishAsync(string spanishText)
        {
            var payload = new { inputs = spanishText };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_apiUrlTranslator, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                throw new System.Exception($"Error traduciendo texto: {response.StatusCode} - {errorText}");
            }

            var json = await response.Content.ReadAsStringAsync();

           

            using var document = JsonDocument.Parse(json);

            if (document.RootElement.ValueKind != JsonValueKind.Array || document.RootElement.GetArrayLength() == 0)
                throw new System.Exception($"Respuesta inesperada del traductor: {json}");

            return document.RootElement[0].GetProperty("translation_text").GetString();
        }

        private string ApplyMedicalRules(string predictedLabel, TriageRequestDto request)
        {
            var hr = request.VitalSigns.HeartRate;
            var rr = request.VitalSigns.RespiratoryRate;
            var temp = request.VitalSigns.Temperature;
            var spo2 = request.VitalSigns.OxygenSaturation;

            if (spo2 < 85 || hr < 40 || hr > 150 || rr > 35)
                return "red";
            if (spo2 < 90 || temp > 39.5 || hr > 130 || rr > 28)
                return "orange";
            if ((temp >= 38 && temp <= 39.5) || (hr > 110 && hr <= 130) || (rr > 22 && rr <= 28))
                return "yellow";
            if ((hr >= 70 && hr <= 100) && (rr >= 14 && rr <= 18) && (temp >= 36.5 && temp <= 37.5) && (spo2 >= 97 && spo2 <= 100))
                return "blue";
            if ((hr >= 60 && hr <= 110) && (rr >= 12 && rr <= 22) && (temp >= 36 && temp <= 37.9) && (spo2 >= 94 && spo2 <= 100))
                return "green";

            return predictedLabel;
        }

        private string GetTriageMessage(string level) => level switch
        {
            "blue" => "Urgencias leves que no comprometen el estado general ni amenazan la vida.",
            "green" => "Urgencias sin compromiso vital inmediato, pero podrían complicarse si no hay atención.",
            "yellow" => "Urgencias que requieren tratamiento rápido para evitar complicaciones.",
            "orange" => "Urgencias graves que pueden escalar rápidamente a compromiso vital.",
            "red" => "Urgencia crítica: atención inmediata y posible reanimación.",
            _ => "Nivel no determinado."
        };
    }
}
