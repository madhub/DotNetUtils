using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Helpers;

public enum HealthStatus
{
    Unhealthy,
    Degraded,
    Healthy
}
public enum Description
{
    Success,
    Timedout,
    RequestCancelled,
    FailedWithException
}

public record class HealthCheckResult(string RequestedUrl,HealthStatus HealthStatus,Description Description);
public class HealthcheckUtils
{
    public static async Task<List<HealthCheckResult>> Check(HttpClient httpClient,List<string> urls, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        var tasks = new List<Task<HealthCheckResult>>();
        foreach (var url in urls)
        {
            tasks.Add(_InvokeRequest(httpClient,url, cts.Token));
        }

        try
        {
            var responses = await Task.WhenAll(tasks);
            return responses.ToList();
        }
        catch (Exception ex) // Catch any other unexpected exceptions
        {
            return urls.Select(url => 
            new HealthCheckResult(url, HealthStatus.Unhealthy, Description.FailedWithException)).ToList();
        }

        return new List<HealthCheckResult>();
    }
    private static async Task<HealthCheckResult> _InvokeRequest(HttpClient httpClient,string url, CancellationToken cancellationToken)
    {
        try
        {
            using (var requestMessage = new HttpRequestMessage())
            {
                requestMessage.Method = HttpMethod.Get;
                requestMessage.RequestUri = new Uri(url);


                using (var response = await httpClient.SendAsync(requestMessage, cancellationToken))
                {
                    response.EnsureSuccessStatusCode();
                    var result = await response.Content.ReadAsStringAsync(cancellationToken);
                    return new HealthCheckResult(url,HealthStatus.Healthy, Description.Success);
                }
            }
        }
        // Filter by InnerException.
        catch (TaskCanceledException taskCancelled) when (taskCancelled.InnerException is TimeoutException)
        {
            // Handle timeout.
            return new HealthCheckResult(url, HealthStatus.Unhealthy, Description.Timedout);
        }
        catch (TaskCanceledException taskCancelled)
        {
            // Handle cancellation.
            return new HealthCheckResult(url, HealthStatus.Unhealthy, Description.RequestCancelled);
        }
        catch (HttpRequestException httpRequestExp)
        {
            return new HealthCheckResult(url, HealthStatus.Unhealthy, Description.FailedWithException);
        }
        catch (Exception exception)
        {
            return new HealthCheckResult(url, HealthStatus.Unhealthy, Description.FailedWithException);
        }
    }
}
