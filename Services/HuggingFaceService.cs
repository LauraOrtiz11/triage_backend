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
            // Unir síntomas + signos vitales en un texto clínico
            var inputText = $"Symptoms: {request.Symptoms}. " +
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
