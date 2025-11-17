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

    // Langfuse configuration
    public class LangfuseConfig
    {
        public string PublicKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string Host { get; set; } = "https://cloud.langfuse.com";
        public bool Enabled { get; set; } = false;
    }

    // Langfuse request models
    public class LangfuseTraceCreate
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("userId")]
        public string UserId { get; set; } = "VEMIS_SYSTEM";
        
        [JsonPropertyName("metadata")]
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    public class LangfuseGenerationCreate
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        
        [JsonPropertyName("traceId")]
        public string TraceId { get; set; } = string.Empty;
        
        [JsonPropertyName("name")]
        public string Name { get; set; } = "gemini-generation";
        
        [JsonPropertyName("startTime")]
        public string StartTime { get; set; } = string.Empty;
        
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;
        
        [JsonPropertyName("modelParameters")]
        public Dictionary<string, object> ModelParameters { get; set; } = new Dictionary<string, object>();
        
        [JsonPropertyName("input")]
        public object Input { get; set; } = new object();
        
        [JsonPropertyName("metadata")]
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    public class LangfuseGenerationUpdate
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        
        [JsonPropertyName("traceId")]
        public string TraceId { get; set; } = string.Empty;
        
        [JsonPropertyName("output")]
        public object Output { get; set; } = new object();
        
        [JsonPropertyName("endTime")]
        public string EndTime { get; set; } = string.Empty;
        
        [JsonPropertyName("completionStartTime")]
        public string CompletionStartTime { get; set; } = string.Empty;
        
        [JsonPropertyName("usage")]
        public LangfuseUsage Usage { get; set; } = new LangfuseUsage();
        
        [JsonPropertyName("metadata")]
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    public class LangfuseUsage
    {
        [JsonPropertyName("input")]
        public int Input { get; set; }
        
        [JsonPropertyName("output")]
        public int Output { get; set; }
        
        [JsonPropertyName("total")]
        public int Total { get; set; }
    }

    public class LangfuseBatchRequest
    {
        [JsonPropertyName("batch")]
        public List<LangfuseBatchItem> Batch { get; set; } = new List<LangfuseBatchItem>();
    }

    public class LangfuseBatchItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
        
        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; } = DateTime.UtcNow.ToString("o");
        
        [JsonPropertyName("body")]
        public object Body { get; set; } = new object();
    }
    
    public class GeminiClient : IGeminiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _modelName;
        private readonly string _apiKey;
        private readonly LangfuseConfig _langfuseConfig;
        private readonly ILogger<GeminiClient> _logger;

        public GeminiClient(HttpClient httpClient, IOptions<GeminiConfig> config, IOptions<LangfuseConfig> langfuseConfig, ILogger<GeminiClient> logger)
        {
            _httpClient = httpClient;
            _modelName = config.Value.ModelName;
            _apiKey = config.Value.ApiKey;
            _langfuseConfig = langfuseConfig.Value;
            _logger = logger;
            
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
            return await SendRequestAsync(prompt, "test_generation");
        }

        public async Task<CorrectionResultDto> CorrectSolutionAsync(string prompt)
        {
            var jsonResponse = await SendRequestAsync(prompt, "correction");

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
        
        private async Task<string> SendRequestAsync(string prompt, string operationType)
        {
            var traceId = Guid.NewGuid().ToString();
            var generationId = Guid.NewGuid().ToString();
            var startTime = DateTime.UtcNow;

            try
            {
                // 1. Langfuse trace létrehozása
                await CreateLangfuseTrace(traceId, $"gemini-{operationType}");

                // 2. Langfuse generation létrehozása
                await CreateLangfuseGeneration(traceId, generationId, prompt, startTime, operationType);

                // 3. Gemini API hívás
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

                var endTime = DateTime.UtcNow;

                // 4. Langfuse generation frissítése
                await UpdateLangfuseGeneration(traceId, generationId, resultText, startTime, endTime);

                return resultText;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hiba történt a Gemini API hívása során");
                var endTime = DateTime.UtcNow;
                
                // 5. Hiba esetén Langfuse generation frissítése
                await UpdateLangfuseGenerationWithError(traceId, generationId, ex.Message, startTime, endTime);
                
                throw;
            }
        }

        private async Task CreateLangfuseTrace(string traceId, string traceName)
        {
            if (!_langfuseConfig.Enabled || string.IsNullOrWhiteSpace(_langfuseConfig.PublicKey))
                return;

            try
            {
                var traceData = new LangfuseBatchRequest
                {
                    Batch = new List<LangfuseBatchItem>
                    {
                        new LangfuseBatchItem
                        {
                            Type = "trace-create",
                            Body = new LangfuseTraceCreate
                            {
                                Id = traceId,
                                Name = traceName,
                                UserId = "VEMIS_SYSTEM",
                                Metadata = new Dictionary<string, object>
                                {
                                    { "application", "VEMIS_BACKEND" },
                                    { "environment", "production" }
                                }
                            }
                        }
                    }
                };

                await SendToLangfuse(traceData);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Langfuse trace létrehozása sikertelen");
            }
        }

        private async Task CreateLangfuseGeneration(string traceId, string generationId, string input, DateTime startTime, string operationType)
        {
            if (!_langfuseConfig.Enabled || string.IsNullOrWhiteSpace(_langfuseConfig.PublicKey))
                return;

            try
            {
                var metadata = new Dictionary<string, object>
                {
                    { "operation_type", operationType },
                    { "model", _modelName }
                };

                var generationData = new LangfuseBatchRequest
                {
                    Batch = new List<LangfuseBatchItem>
                    {
                        new LangfuseBatchItem
                        {
                            Type = "generation-create",
                            Timestamp = startTime.ToString("o"),
                            Body = new LangfuseGenerationCreate
                            {
                                Id = generationId,
                                TraceId = traceId,
                                Name = "gemini-generation",
                                StartTime = startTime.ToString("o"),
                                Model = _modelName,
                                ModelParameters = new Dictionary<string, object>
                                {
                                    { "temperature", 0.7 },
                                    { "maxOutputTokens", 8000 }
                                },
                                Input = new { prompt = input },
                                Metadata = metadata
                            }
                        }
                    }
                };

                await SendToLangfuse(generationData);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Langfuse generation létrehozása sikertelen");
            }
        }

        private async Task UpdateLangfuseGeneration(string traceId, string generationId, string output, DateTime startTime, DateTime endTime)
        {
            if (!_langfuseConfig.Enabled || string.IsNullOrWhiteSpace(_langfuseConfig.PublicKey))
                return;

            try
            {
                // Egyszerű token számítás
                int inputTokens = output.Length / 4;
                int outputTokens = output.Length / 4;
                int totalTokens = inputTokens + outputTokens;

                var updateData = new LangfuseBatchRequest
                {
                    Batch = new List<LangfuseBatchItem>
                    {
                        new LangfuseBatchItem
                        {
                            Type = "generation-update",
                            Timestamp = endTime.ToString("o"),
                            Body = new LangfuseGenerationUpdate
                            {
                                Id = generationId,
                                TraceId = traceId,
                                Output = new { result = output },
                                EndTime = endTime.ToString("o"),
                                CompletionStartTime = endTime.ToString("o"),
                                Usage = new LangfuseUsage
                                {
                                    Input = inputTokens,
                                    Output = outputTokens,
                                    Total = totalTokens
                                },
                                Metadata = new Dictionary<string, object>
                                {
                                    { "status", "success" },
                                    { "duration_ms", (endTime - startTime).TotalMilliseconds }
                                }
                            }
                        }
                    }
                };

                await SendToLangfuse(updateData);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Langfuse generation frissítése sikertelen");
            }
        }

        private async Task UpdateLangfuseGenerationWithError(string traceId, string generationId, string error, DateTime startTime, DateTime endTime)
        {
            if (!_langfuseConfig.Enabled || string.IsNullOrWhiteSpace(_langfuseConfig.PublicKey))
                return;

            try
            {
                var updateData = new LangfuseBatchRequest
                {
                    Batch = new List<LangfuseBatchItem>
                    {
                        new LangfuseBatchItem
                        {
                            Type = "generation-update",
                            Timestamp = endTime.ToString("o"),
                            Body = new LangfuseGenerationUpdate
                            {
                                Id = generationId,
                                TraceId = traceId,
                                Output = new { error = error },
                                EndTime = endTime.ToString("o"),
                                CompletionStartTime = endTime.ToString("o"),
                                Usage = new LangfuseUsage
                                {
                                    Input = 0,
                                    Output = 0,
                                    Total = 0
                                },
                                Metadata = new Dictionary<string, object>
                                {
                                    { "status", "error" },
                                    { "duration_ms", (endTime - startTime).TotalMilliseconds },
                                    { "error_message", error }
                                }
                            }
                        }
                    }
                };

                await SendToLangfuse(updateData);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Langfuse error generation frissítése sikertelen");
            }
        }

        private async Task SendToLangfuse(LangfuseBatchRequest data)
        {
            try
            {
                var url = $"{_langfuseConfig.Host}/api/public/ingestion";
                var jsonRequest = JsonSerializer.Serialize(data, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                });
                
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                using var request = new HttpRequestMessage(HttpMethod.Post, url);
                string auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_langfuseConfig.PublicKey}:{_langfuseConfig.SecretKey}"));
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", auth);
                request.Content = content;

                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Langfuse API hiba: {StatusCode} - {ResponseBody}", response.StatusCode, responseBody);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Langfuse API hívás sikertelen");
            }
        }
    }
}