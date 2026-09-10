using System.Net.Http.Json;
using JobAutomationPlatform.Application.Interfaces;
using JobAutomationPlatform.Domain.Entities;
using JobAutomationPlatform.Infrastructure.Security;

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

    public async Task<JobRunResult> RunAsync(Job job, QueuedExecutionClaim claim, CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient(nameof(HttpJobRunner));
        httpClient.Timeout = TimeSpan.FromSeconds(Math.Max(5, job.TimeoutSeconds));
        httpClient.DefaultRequestHeaders.ExpectContinue = false;

        await OutboundUrlValidator.EnsureSafeAsync(new Uri(job.TargetUrl), cancellationToken);

        using var requestMessage = new HttpRequestMessage(new HttpMethod(job.HttpMethod), job.TargetUrl);
        requestMessage.Headers.TryAddWithoutValidation("X-Enrichly-Execution-Id", claim.ExecutionRequestId.ToString());
        requestMessage.Headers.TryAddWithoutValidation("X-Enrichly-Attempt-Number", claim.AttemptNumber.ToString());
        requestMessage.Headers.TryAddWithoutValidation("X-Enrichly-Job-Id", claim.JobId.ToString());
        requestMessage.Headers.TryAddWithoutValidation("X-Enrichly-Owner-User-Id", claim.OwnerUserId.ToString());

        foreach (var header in HttpRequestHeaderParser.Parse(job.RequestHeadersJson))
        {
            requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (!string.IsNullOrWhiteSpace(job.PayloadJson))
        {
            requestMessage.Content = new StringContent(job.PayloadJson, System.Text.Encoding.UTF8, "application/json");
        }

        try
        {
            var started = DateTimeOffset.UtcNow;
            var response = await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            var completed = DateTimeOffset.UtcNow;
            var output = $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}";
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseHeaders = response.Headers.ToDictionary(x => x.Key, x => string.Join(",", x.Value), StringComparer.OrdinalIgnoreCase);
            var resultJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                statusCode = (int)response.StatusCode,
                durationMilliseconds = (long)(completed - started).TotalMilliseconds,
                responseHeaders,
                responseBody,
            });

            if (response.IsSuccessStatusCode)
            {
                return new JobRunResult(true, output, null, resultJson, (int)response.StatusCode, (long)(completed - started).TotalMilliseconds, System.Text.Json.JsonSerializer.Serialize(responseHeaders), responseBody, false);
            }

            return new JobRunResult(false, output, $"Job returned {(int)response.StatusCode}", resultJson, (int)response.StatusCode, (long)(completed - started).TotalMilliseconds, System.Text.Json.JsonSerializer.Serialize(responseHeaders), responseBody, false);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            var resultJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                timeoutSeconds = job.TimeoutSeconds,
                error = ex.Message,
            });

            return new JobRunResult(false, null, "Job execution timed out.", resultJson, null, null, null, null, true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Job execution failed for {JobId}", job.Id);
            var resultJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                error = ex.Message,
                exception = ex.ToString(),
            });

            return new JobRunResult(false, null, ex.Message, resultJson, null, null, null, null, false);
        }
    }
}
