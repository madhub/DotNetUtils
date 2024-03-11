namespace Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

public class AsyncHttpRequester
{
    public record ApiResult()
    {
        public HttpStatusCode StatusCode { get; init; } = HttpStatusCode.ServiceUnavailable;
        public String? Result { get; init; } = String.Empty;
        public Exception? Exception { get; init; } = default;
    }

    private readonly HttpClient _client;
    private readonly TimeSpan _timeout;

    public AsyncHttpRequester(HttpClient client, TimeSpan timeout)
    {
        _client = client;
        _timeout = timeout;
    }

    public async Task<Dictionary<string, ApiResult>> GetStringResultsAsync(IEnumerable<string> urls)
    {
        var tasks = new List<Task<KeyValuePair<string, ApiResult>>>();

        foreach (var url in urls)
        {
            tasks.Add(GetStringResultAsync(url));
        }

        var results = new Dictionary<string, ApiResult>();
        await Task.WhenAll(tasks);

        foreach (var task in tasks)
        {
            var result = await task;
            results.Add(result.Key, result.Value);
        }

        return results;
    }

    private async Task<KeyValuePair<string, ApiResult>> GetStringResultAsync(string url)
    {
        using (var cts = new CancellationTokenSource(_timeout))
        {
            try
            {
                var response = await _client.GetAsync(url, cts.Token);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return new KeyValuePair<string, ApiResult>(url, new ApiResult() { StatusCode = response.StatusCode });
            }
            catch (OperationCanceledException exp)
            {

                return new KeyValuePair<string, ApiResult>(url,
                    new ApiResult() { Result = $"Request timed out after {_timeout.TotalMilliseconds}ms", Exception = exp });
            }
            catch (HttpRequestException httpException)
            {
                return new KeyValuePair<string, ApiResult>(url,
                    new ApiResult() { StatusCode = httpException.StatusCode.Value, Exception = httpException });
            }
            catch (Exception ex)
            {
                return new KeyValuePair<string, ApiResult>(url,
                    new ApiResult() { Exception = ex });
            }
        }
    }

    public async Task<Dictionary<string, string>> GetStringResultsAsyncUsingLinq(IEnumerable<string> urls)
    {
        var tasks = urls.Select(async url =>
        {
            using (var cts = new CancellationTokenSource(_timeout))
            {
                try
                {
                    var response = await _client.GetAsync(url, cts.Token);
                    response.EnsureSuccessStatusCode();
                    return new KeyValuePair<string, string>(url, await response.Content.ReadAsStringAsync());
                }
                catch (OperationCanceledException)
                {
                    return new KeyValuePair<string, string>(url, $"Request to {url} timed out.");
                }
                catch (Exception ex)
                {
                    return new KeyValuePair<string, string>(url, $"Error: {ex.Message}");
                }
            }
        });

        return (await Task.WhenAll(tasks)).ToDictionary(x => x.Key, x => x.Value);
    }
}

