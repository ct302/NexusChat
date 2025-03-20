using Newtonsoft.Json;
using OllamaBlazorWasm.Models;
using System.Net.Http.Json;
using System.Text;

namespace OllamaBlazorWasm.Services
{
    public class OllamaService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OllamaService> _logger;
        private readonly string _apiUrl = "http://localhost:11434/v1/chat/completions";

        public OllamaService(HttpClient httpClient, ILogger<OllamaService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            
            // Remove timeout for LLM responses
            _httpClient.Timeout = System.Threading.Timeout.InfiniteTimeSpan;
        }

        public async Task<string> GetChatResponse(string prompt, string model, List<ChatMessage> conversationContext)
        {
            try
            {
                // Convert the conversation context to the format expected by Ollama API
                var messages = conversationContext
                    .Where(m => !m.IsProcessing)
                    .Select(m => new { role = m.Role.ToLower(), content = m.Content })
                    .ToList();
                
                // Add the current prompt if it's not already in context
                if (!string.IsNullOrEmpty(prompt) && 
                    (messages.Count == 0 || messages.Last().content != prompt))
                {
                    messages.Add(new { role = "user", content = prompt });
                }
                
                var requestData = new
                {
                    model = model,
                    messages = messages,
                    stream = false
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(requestData),
                    Encoding.UTF8,
                    "application/json"
                );

                _logger.LogInformation($"Sending request to Ollama API for model: {model}");
                var response = await _httpClient.PostAsync(_apiUrl, content);
                
                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error response from Ollama API: {errorContent}");
                    return $"Error: The API returned status code {response.StatusCode}. Please make sure Ollama is running at http://localhost:11434.";
                }
                
                var jsonResponse = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Received response from Ollama API");
                
                try
                {
                    dynamic result = JsonConvert.DeserializeObject(jsonResponse);
                    return result.choices[0].message.content;
                }
                catch (Exception jsonEx)
                {
                    _logger.LogError($"JSON parse error: {jsonEx.Message}");
                    return $"Error parsing API response: {jsonEx.Message}";
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HTTP request error: {ex.Message}");
                return $"Error connecting to Ollama: {ex.Message}. Please ensure Ollama is running.";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }
    }
}