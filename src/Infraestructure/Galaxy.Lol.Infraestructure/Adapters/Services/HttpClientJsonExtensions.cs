using System.Text.Json;

namespace Galaxy.Lol.Infraestructure.Adapters.Services
{

    internal static class HttpClientJsonExtensions
    {
        public static async Task<T?> GetFromJsonSafeAsync<T>(this HttpClient client, string url,
            JsonSerializerOptions options, CancellationToken cancellationToken)
        {
            using var response = await client.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return await JsonSerializer.DeserializeAsync<T>(stream, options, cancellationToken);
        }
    }
}
