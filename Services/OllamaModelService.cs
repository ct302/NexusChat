using System.Text.Json;
using System.Text.Json.Serialization;

namespace OllamaBlazorWasm.Services
{
    public class OllamaModelService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OllamaModelService> _logger;
        private readonly string _modelsApiUrl = "http://localhost:11434/api/tags";

        public OllamaModelService(HttpClient httpClient, ILogger<OllamaModelService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<string>> GetAvailableModels()
        {
            try
            {
                // Default models in case the API call fails
                var defaultModels = new List<string> { "phi4", "deepseek-r1:7b", "qwen2:7b" };
                
                var response = await _httpClient.GetAsync(_modelsApiUrl);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Failed to get models from Ollama API. Status code: {response.StatusCode}");
                    return defaultModels;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var modelsResponse = JsonSerializer.Deserialize<OllamaModelsResponse>(jsonResponse);
                
                if (modelsResponse?.Models == null || !modelsResponse.Models.Any())
                {
                    _logger.LogWarning("No models returned from Ollama API");
                    return defaultModels;
                }

                // Extract model names from the response
                return modelsResponse.Models
                    .Select(m => m.Name)
                    .Distinct()
                    .OrderBy(m => m)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching models: {ex.Message}");
                // Return default models if there's an error
                return new List<string> { "phi4", "deepseek-r1:7b", "qwen2:7b" };
            }
        }
    }

    public class OllamaModelsResponse
    {
        [JsonPropertyName("models")]
        public List<OllamaModel> Models { get; set; } = new List<OllamaModel>();
    }

    public class OllamaModel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("size")]
        public long Size { get; set; }

        [JsonPropertyName("modified_at")]
        public string ModifiedAt { get; set; } = string.Empty;
    }
}