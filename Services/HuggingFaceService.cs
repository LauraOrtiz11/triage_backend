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
        private readonly string _apiUrl = "https://api-inference.huggingface.co/models/facebook/bart-large-mnli";

        public HuggingFaceService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiToken = configuration["HuggingFace:ApiToken"]
                        ?? throw new System.Exception("HuggingFace API Token not configured.");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiToken}");
        }

        public async Task<TriageResponseDto> GetTriagePredictionAsync(TriageRequestDto request)
        {
            // Valores normales de referencia para orientar al modelo
            var normalRanges = "Normal ranges: Heart Rate 60-100 bpm, Respiratory Rate 12-20 rpm, " +
                               "Blood Pressure around 120/80 mmHg, Temperature 36.0-37.5 °C, Oxygen Saturation ≥ 95%.";

            // Prompt enriquecido con contexto y reglas
            var inputText =
            "You are an experienced triage nurse in an emergency department. " +
            "Classify the urgency level strictly as one of: blue, green, yellow, orange, or red. " +
            "Guidelines: " +
            "Blue = mild conditions like common cold, mild headache, slight sore throat. " +
            "Green = moderate conditions that need attention but not life-threatening, like stable fractures or mild asthma. " +
            "Yellow = conditions requiring rapid treatment to avoid complications, like chest infection with fever. " +
            "Orange = urgent conditions that could escalate quickly, like severe chest pain, severe shortness of breath. " +
            "Red = critical emergencies requiring immediate intervention, like cardiac arrest, shock, respiratory failure. " +
            "Always prioritize the SYMPTOMS, and use vital signs as supporting evidence. " +
            "Normal ranges: Heart Rate 60-100 bpm, Respiratory Rate 12-20 rpm, Blood Pressure ~120/80 mmHg, Temperature 36.0-37.5 °C, Oxygen Saturation ≥ 95%. " +
            $"Patient case: Symptoms: {request.Symptoms}. " +
            $"Vital Signs: Heart Rate {request.VitalSigns.HeartRate}, " +
            $"Respiratory Rate {request.VitalSigns.RespiratoryRate}, " +
            $"Blood Pressure {request.VitalSigns.BloodPressure}, " +
            $"Temperature {request.VitalSigns.Temperature}, " +
            $"Oxygen Saturation {request.VitalSigns.OxygenSaturation}.";

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
            var response = await _httpClient.PostAsync(_apiUrl, content);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(json);

            var label = document.RootElement.GetProperty("labels")[0].GetString();
            var score = document.RootElement.GetProperty("scores")[0].GetDecimal();

            return new TriageResponseDto
            {
                SuggestedLevel = label ?? "undetermined",
                Confidence = System.Math.Round(score, 2),
                Message = GetTriageMessage(label)
            };
        }


        private string GetTriageMessage(string level)
        {
            return level switch
            {
                "blue" => "Urgencias que no comprometen el estado general ni amenazan la vida.",
                "green" => "Urgencias sin compromiso vital inmediato, pero podrían complicarse.",
                "yellow" => "Urgencias que requieren exámenes y/o tratamiento rápido para evitar complicaciones.",
                "orange" => "Urgencias donde la condición puede escalar rápidamente a compromiso vital.",
                "red" => "Urgencia crítica: atención inmediata y posible reanimación.",
                _ => "Nivel no determinado."
            };
        }
    }
}
