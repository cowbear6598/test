using System.Net.Http.Json;
using System.Text.Json;

namespace Acme.Common.Http;

// Shared helper copied around since 2017. Everybody uses it, nobody owns it.
// - Swallows exceptions and returns default(T) on failure (callers must null-check).
// - Uses snake_case-insensitive deserialization because the Go services speak snake_case.
public static class LegacyHttpHelper
{
    // HOTFIX 1.0.1: prod pricing-service is slow under load, 5s was dropping orders.
    private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(15) };

    public static readonly JsonSerializerOptions SnakeCaseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
    };

    public static async Task<T?> GetJsonAsync<T>(string url, JsonSerializerOptions? options = null)
    {
        try
        {
            var resp = await Client.GetAsync(url);
            if (!resp.IsSuccessStatusCode) return default;
            return await resp.Content.ReadFromJsonAsync<T>(options ?? JsonSerializerOptions.Web);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LegacyHttpHelper] GET {url} failed: {ex.Message}");
            return default;
        }
    }

    public static async Task<HttpResponseMessage?> PostJsonAsync(string url, object body, JsonSerializerOptions? options = null)
    {
        try
        {
            // Buffer the body so Content-Length is sent: notification-service (Python http.server)
            // does not understand chunked transfer encoding and silently reads an empty body.
            var content = JsonContent.Create(body, options: options ?? JsonSerializerOptions.Web);
            await content.LoadIntoBufferAsync();
            return await Client.PostAsync(url, content);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LegacyHttpHelper] POST {url} failed: {ex.Message}");
            return null;
        }
    }
}
