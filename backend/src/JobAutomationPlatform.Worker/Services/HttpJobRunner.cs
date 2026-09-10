using System.Net.Http.Json;
using JobAutomationPlatform.Application.Interfaces;
using JobAutomationPlatform.Domain.Entities;

namespace JobAutomationPlatform.Worker.Services;

public sealed class HttpJobRunner : IJobRunner
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HttpJobRunner> _logger;

    public HttpJobRunner(IHttpClientFactory httpClientFactory, ILogger<HttpJobRunner> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<JobRunResult> RunAsync(Job job, CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient(nameof(HttpJobRunner));
        httpClient.Timeout = TimeSpan.FromSeconds(60);

        using var requestMessage = new HttpRequestMessage(new HttpMethod(job.HttpMethod), job.TargetUrl);

        if (!string.IsNullOrWhiteSpace(job.PayloadJson))
        {
            requestMessage.Content = new StringContent(job.PayloadJson, System.Text.Encoding.UTF8, "application/json");
        }

        try
        {
            var response = await httpClient.SendAsync(requestMessage, cancellationToken);
            var output = $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}";

            if (response.IsSuccessStatusCode)
            {
                return new JobRunResult(true, output, null, null);
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return new JobRunResult(false, output, $"Job returned {(int)response.StatusCode}", string.IsNullOrWhiteSpace(body) ? null : body);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Job execution failed for {JobId}", job.Id);
            return new JobRunResult(false, null, ex.Message, ex.ToString());
        }
    }
}
