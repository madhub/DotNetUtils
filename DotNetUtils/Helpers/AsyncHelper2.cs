using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class ApiRequest
{
    public string Url { get; set; }
    public Dictionary<string, string> ? Headers { get; set; }
}

public record ApiResponse()
{
    public ApiRequest Request { get;  init; }
    public HttpStatusCode ? StatusCode { get; init; } = HttpStatusCode.ServiceUnavailable;
    public String? Result { get; init; } = String.Empty;
    public Exception? Exception { get; init; } = default;
    public bool IsSuccess()
    {
        return Exception == null;
    }
}


public class ApiRequestHelper
{
    private readonly HttpClient _httpClient;

    public ApiRequestHelper(HttpClient ? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<List<ApiResponse>> InvokeRequestsConcurrently(List<ApiRequest> apiRequests,TimeSpan timeout )
    {
        using var cts = new CancellationTokenSource(timeout);
        var tasks = new List<Task<ApiResponse>>();
        foreach (var request in apiRequests)
        {
            tasks.Add(_InvokeRequest(request, cts.Token));
        }

        try
        {
            var responses = await Task.WhenAll(tasks);
            return responses.ToList();
        }
        catch (Exception ex) // Catch any other unexpected exceptions
        {
            return apiRequests.Select(request => new ApiResponse() { Request = request, Exception = ex }).ToList();
        }
    }
    private async Task<ApiResponse> _InvokeRequest(ApiRequest request, CancellationToken cancellationToken)
    {
        try
        {
            using (var requestMessage = new HttpRequestMessage())
            {
                requestMessage.Method = HttpMethod.Get;
                requestMessage.RequestUri = new Uri(request.Url);

                if (request.Headers != null)
                {
                    foreach (var (key, value) in request.Headers)
                    {
                        requestMessage.Headers.Add(key, value);
                    }
                }

                using (var response = await _httpClient.SendAsync(requestMessage, cancellationToken))
                {
                    response.EnsureSuccessStatusCode();
                    var result = await response.Content.ReadAsStringAsync(cancellationToken);
                    return new ApiResponse() { Request = request, Result = result,StatusCode = response.StatusCode };
                }
            }
        }
        // Filter by InnerException.
        catch (TaskCanceledException taskCancelled) when (taskCancelled.InnerException is TimeoutException)
        {
            // Handle timeout.
            return new ApiResponse() { Request = request, Exception = taskCancelled.InnerException};
        }
        catch (TaskCanceledException taskCancelled)
        {
            // Handle cancellation.
            return new ApiResponse() { Request = request, Exception = taskCancelled };
        }
        catch (HttpRequestException httpRequestExp)
        {
            return new ApiResponse() { Request = request,StatusCode=httpRequestExp.StatusCode, Exception = httpRequestExp };
        }
        catch (Exception exception)
        {
            return new ApiResponse() {Request=request,Exception=exception };
        }
    }


    //public class ApiResponse
    //{
    //    public ApiRequest Request { get; }
    //    public string? Result { get; }
    //    public int? StatusCode { get; }
    //    public Exception? Exception { get; }

    //    public ApiResponse(ApiRequest request, string? result = default, int? statusCode = 0, Exception? exception = default)
    //    {
    //        Request = request;
    //        Result = result;
    //        StatusCode = statusCode;
    //        Exception = exception;
    //    }
    //    public static ApiResponse SuccessApiResponse(ApiRequest request, string? result, int? statusCode = 0)
    //        => new ApiResponse(request, result, 200, null);
    //}

    //public async Task<List<ApiResponse>> InvokeRequestsConcurrently(List<ApiRequest> apiRequests, 
    //    CancellationToken cancellationToken = default)
    //{
    //    var tasks = new List<Task<ApiResponse>>();
    //    foreach (var request in apiRequests)
    //    {
    //        tasks.Add(_InvokeRequest(request, cancellationToken));
    //    }

    //    try
    //    {
    //        var responses = await Task.WhenAll(tasks);
    //        return responses.ToList();
    //    }
    //    catch (Exception ex) // Catch any other unexpected exceptions
    //    {
    //        return apiRequests.Select(request => new ApiResponse(request, exception: ex)).ToList();
    //    }
    //}


    //private async Task<ApiResponse> _InvokeRequest(ApiRequest request, CancellationToken cancellationToken)
    //{
    //    try
    //    {
    //        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    //        cts.CancelAfter(TimeSpan.FromSeconds(10)); // Adjust timeout as needed

    //        using (var requestMessage = new HttpRequestMessage())
    //        {
    //            requestMessage.Method = HttpMethod.Get;
    //            requestMessage.RequestUri = new Uri(request.Url);

    //            if (request.Headers != null)
    //            {
    //                foreach (var (key, value) in request.Headers)
    //                {
    //                    requestMessage.Headers.Add(key, value);
    //                }
    //            }

    //            using (var response = await _httpClient.SendAsync(requestMessage, cts.Token))
    //            {
    //                response.EnsureSuccessStatusCode();
    //                var result = await response.Content.ReadAsStringAsync(cts.Token);
    //                return ApiResponse.SuccessApiResponse(request, result);
    //            }
    //        }
    //    }
    //    // Filter by InnerException.
    //    catch (TaskCanceledException taskCancelled) when (taskCancelled.InnerException is TimeoutException)
    //    {
    //        // Handle timeout.
    //        return new ApiResponse(request, exception: taskCancelled.InnerException);
    //    }
    //    catch(TaskCanceledException taskCancelled)
    //    {
    //        // Handle cancellation.
    //        return new ApiResponse(request, exception: taskCancelled);
    //    }        
    //    catch (HttpRequestException httpRequestExp)
    //    {
    //        return new ApiResponse(request, exception: httpRequestExp);
    //    }
    //    catch (Exception exception)
    //    {
    //        return new ApiResponse(request, exception: exception); 
    //    }
    //}

    // Additional methods for cancellation and cleanup (if needed):
    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
