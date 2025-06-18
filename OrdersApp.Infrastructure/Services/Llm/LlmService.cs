using OrdersApp.Application.DTOs;
using OrdersApp.Application.Interfaces;
using OrdersApp.Infrastructure.Config;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace OrdersApp.Infrastructure.Services.Llm
{
    public class LlmService : ILlmService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public LlmService(IOptions<OpenAiSettings> options)
        {
            _apiKey = options.Value.ApiKey ?? throw new ArgumentNullException("OpenAI:ApiKey is missing in config");

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.openai.com/v1/")
            };

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
        }

        private static readonly Dictionary<string, List<OrderItemDto>> _cache = new();
        private static readonly SemaphoreSlim _rateLimiter = new(1, 1);

        public async Task<List<OrderItemDto>> ExtractOrderItemsAsync(string mailBody)
        {
            Console.WriteLine("Start przetwarzania maila...");

            var trimmed = mailBody.Trim();

            // 🔍 1. Lokalna próba parsowania jako JSON
            if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
            {
                try
                {
                    var localResult = JsonSerializer.Deserialize<List<OrderItemDto>>(trimmed);
                    if (localResult != null && localResult.Count > 0)
                    {
                        Console.WriteLine("Wykryto gotowy JSON – parsowanie lokalne bez GPT.");
                        return localResult;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Błąd lokalnego parsowania JSON: {ex.Message}");
                }
            }

            // 🔁 2. Użycie GPT (jeśli lokalne się nie udało)
            var cacheKey = trimmed.GetHashCode().ToString();
            if (_cache.TryGetValue(cacheKey, out var cached))
            {
                Console.WriteLine("Zwrócono z cache.");
                return cached;
            }

            var prompt = """
Wyodrębnij wyłącznie listę zamówionych produktów z poniższego maila. 
Odpowiedz tylko czystym JSON, bez komentarzy i tekstu wokół. Format:
[
  { "productName": "nazwa", "quantity": liczba, "price": liczba }
]

Treść maila:
""" + mailBody;

            var requestBody = new
            {
                model = "gpt-4o",
                messages = new[] { new { role = "user", content = prompt } },
                temperature = 0.2
            };

            try
            {
                await _rateLimiter.WaitAsync();
                await Task.Delay(1000); // dodatkowe opóźnienie

                HttpResponseMessage response = null!;
                int retry = 0;

                while (retry < 3)
                {
                    response = await _httpClient.PostAsJsonAsync("chat/completions", requestBody);
                    if ((int)response.StatusCode != 429)
                        break;

                    Console.WriteLine("Przekroczono limit – czekam 2s...");
                    await Task.Delay(2000);
                    retry++;
                }

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"OpenAI API error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");

                var raw = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(raw);

                var content = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                Console.WriteLine($"Odpowiedź GPT:\n{content}");

                var start = content.IndexOf('[');
                var end = content.LastIndexOf(']');
                if (start == -1 || end == -1 || end <= start)
                    throw new Exception("Nie znaleziono poprawnego JSON-a w odpowiedzi GPT.");

                var jsonOnly = content.Substring(start, end - start + 1);
                var result = JsonSerializer.Deserialize<List<OrderItemDto>>(jsonOnly) ?? new();

                _cache[cacheKey] = result;

                Console.WriteLine($"Sparsowano {result.Count} pozycji z JSON.");
                return result;
            }
            finally
            {
                _rateLimiter.Release();
            }
        }
    }
}
