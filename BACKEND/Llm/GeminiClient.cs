using ProjectName.Models.DTOs;
using ProjectName.Models;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProjectName.Llm
{
    public class Part
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    public class Content
    {
        [JsonPropertyName("parts")]
        public List<Part> Parts { get; set; } = new List<Part>();
    }

    public class GenerateContentRequest
    {
        [JsonPropertyName("contents")]
        public List<Content> Contents { get; set; } = new List<Content>();
    }
    
    public class GenerateContentResponse
    {
        [JsonPropertyName("candidates")]
        public List<Candidate> Candidates { get; set; } = new List<Candidate>();
    }

    public class Candidate
    {
        [JsonPropertyName("content")]
        public Content Content { get; set; } = new Content();
    }
    
    public class GeminiClient : IGeminiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _modelName;
        private readonly string _apiKey;

        public GeminiClient(HttpClient httpClient, IOptions<GeminiConfig> config)
        {
            _httpClient = httpClient;
            _modelName = config.Value.ModelName;
            _apiKey = config.Value.ApiKey;
            
            if (string.IsNullOrEmpty(_modelName))
            {
                throw new InvalidOperationException("Gemini ModelName nincs beállítva az appsettings.json-ban.");
            }
             if (string.IsNullOrEmpty(_apiKey))
            {
                throw new InvalidOperationException("Gemini ApiKey nincs beállítva az appsettings.json-ban.");
            }
        }

        public async Task<string> GenerateTestAsync(string prompt)
        {
            return await SendRequestAsync(prompt);
        }

        public async Task<CorrectionResultDto> CorrectSolutionAsync(string prompt)
        {
            var jsonResponse = await SendRequestAsync(prompt);

            try
            {
                return JsonSerializer.Deserialize<CorrectionResultDto>(jsonResponse) 
                    ?? throw new JsonException("A modell válasza nem volt deszerializálható CorrectionResultDto-ra.");
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Hiba a kiértékelési válasz feldolgozása során: {ex.Message}. Fogadott JSON: {jsonResponse.Substring(0, Math.Min(500, jsonResponse.Length))}", ex);
            }
        }
        
        private async Task<string> SendRequestAsync(string prompt)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_modelName}:generateContent?key={_apiKey}";

            var requestBody = new GenerateContentRequest
            {
                Contents = new List<Content>
                {
                    new Content
                    {
                        Parts = new List<Part> { new Part { Text = prompt } }
                    }
                }
            };
            
            var options = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
            var json = JsonSerializer.Serialize(requestBody, options);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Response status code does not indicate success: {(int)response.StatusCode} ({response.StatusCode}). Hiba részletei: {errorContent}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            
            var geminiResponse = JsonSerializer.Deserialize<GenerateContentResponse>(responseJson);

            var resultText = geminiResponse?.Candidates
                .FirstOrDefault()?
                .Content?
                .Parts
                .FirstOrDefault()?
                .Text;

            if (string.IsNullOrEmpty(resultText))
            {
                throw new InvalidOperationException($"Hiba a Gemini válasz feldolgozása során. A modell üres választ adott, vagy hiba történt. Teljes válasz: {responseJson}");
            }
            
            resultText = resultText
                .Trim()
                .Replace("```json", string.Empty)
                .Replace("```", string.Empty)
                .Trim();

            return resultText;
        }
    }
}