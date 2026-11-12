using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SafetyActuation.Domain.Services;
using SharedKernel.Application;

namespace SafetyActuation.Infrastructure;

public sealed class HttpPlantOverrideLink : IPlantOverrideLink
{
    public const string HttpClientName = "plant-override";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly string? _url;
    private readonly string _plantKey;
    private readonly IHttpClientFactory _http;
    private readonly ILogger<HttpPlantOverrideLink> _logger;

    public HttpPlantOverrideLink(
        string? url,
        string plantKey,
        IHttpClientFactory http,
        ILogger<HttpPlantOverrideLink> logger)
    {
        _url = string.IsNullOrWhiteSpace(url) ? null : url.Trim();
        _plantKey = plantKey;
        _http = http;
        _logger = logger;
    }

    public async Task SendAsync(
        Guid areaId,
        string actuatorType,
        string runState,
        CancellationToken cancellationToken)
    {
        if (_url is null || string.IsNullOrEmpty(_plantKey))
        {
            throw new AppException("edge_unreachable", 503);
        }

        try
        {
            var http = _http.CreateClient(HttpClientName);
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                new Uri(new Uri(_url), "/internal/v1/overrides"));
            request.Headers.TryAddWithoutValidation("X-Plant-Key", _plantKey);
            request.Content = JsonContent.Create(
                new { areaId, actuatorType, runState },
                options: JsonOptions);
            using var response = await http.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return;
            }

            _logger.LogWarning(
                "Plant override was not accepted. Status {StatusCode}.",
                (int)response.StatusCode);
            throw new AppException("edge_unreachable", 503);
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Plant override could not reach the edge.");
            throw new AppException("edge_unreachable", 503);
        }
    }
}
