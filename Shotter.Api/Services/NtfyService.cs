using Microsoft.Extensions.Options;

namespace Shotter.Services;

public class NtfyService(IHttpClientFactory httpClientFactory, ILogger<NtfyService> logger, IOptions<NtfyOptions> options) : INotificationService
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
    private readonly string? _ntfyServiceUrl = options.Value.Url;
    private readonly string? _ntfyApiToken = options.Value.AccessToken;
    private readonly string? _ntfyTopic = options.Value.Topic;

    public async Task SendNotification(string message, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(_ntfyServiceUrl) || string.IsNullOrEmpty(_ntfyTopic))
            {
                throw new Exception("Url or topic missing.");
            }
            
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                _ntfyServiceUrl + "/" + _ntfyTopic);
            
            request.Content = new StringContent(message);

            if (!string.IsNullOrEmpty(_ntfyApiToken))
                request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {_ntfyApiToken}");

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return;
            }

            await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var error = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new Exception(error);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to publish notification to ntfy.");
        }

    }
}