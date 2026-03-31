using CapstoneProject.Application.Interface.IService;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CapstoneProject.Application.Service
{
    public class TopicAiService : ITopicAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;

        public TopicAiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Groq:ApiKey"]?.Trim() ?? "";
            _model = configuration["Groq:Model"] ?? "llama-3.3-70b-versatile";
        }

        public async Task<string> GenerateTopicsForSubmissionAsync(string message)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new Exception("Configuration Error: Groq API Key is missing.");
            }

            string[] topicKeywords = { "topic", "suggest", "project", "idea", "thesis", "capstone", "đề tài", "gợi ý" };
            bool isAskingForTopic = topicKeywords.Any(k => message.ToLower().Contains(k));

            string systemPrompt = isAskingForTopic
                ? "You are an IT Advisor. Suggest 3 projects. Return ONLY a JSON Array [{\"title\":\"...\",\"description\":\"...\",\"tags\":[\"...\"]}] in English. No prose, no explanation, no markdown."
                : "You are ThesisPro AI, a helpful assistant. Respond friendly and concisely in English.";

            var requestBody = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = message }
                },
                temperature = 0.7
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.SendAsync(request);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Groq Provider Error: {responseString}");
                }

                using JsonDocument doc = JsonDocument.Parse(responseString);
                var aiText = doc.RootElement
                                .GetProperty("choices")[0]
                                .GetProperty("message")
                                .GetProperty("content")
                                .GetString();

                return aiText?.Replace("```json", "").Replace("```", "").Trim() ?? "[]";
            }
            catch (Exception ex)
            {
                throw new Exception($"System Error: {ex.Message}");
            }
        }
    }
}