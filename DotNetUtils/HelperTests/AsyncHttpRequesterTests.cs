using Helpers;
using JustEat.HttpClientInterception;
using System.Net;
using static Helpers.AsyncHttpRequester;

namespace HelperTests;

[TestClass]
public class AsyncHttpRequesterTests
{
    [TestMethod]
    public void GetStringResultsAsync_UsingJustEatHttpInterceptor()
    {
        List<string> Urls = new List<string>()
        {
             "http://localhost1:5051/health", "http://localhost2:5052/health",
            "http://localhost3:5053/health" 
        };
        Dictionary<string,ApiResult> expectedResults = new Dictionary<string, ApiResult>()
        {
            {
                "http://localhost1:5051/health",
                new ApiResult() { StatusCode=HttpStatusCode.ServiceUnavailable}
            },
             {
                "http://localhost2:5052/health",
                new ApiResult() { StatusCode=HttpStatusCode.OK}
            }
            ,
             {
                "http://localhost3:5053/health",
                new ApiResult() { StatusCode=HttpStatusCode.InternalServerError}
            }
        };


        // Arrange
        var options = new HttpClientInterceptorOptions();
        var latency = TimeSpan.FromMilliseconds(2000);
        var serviceA = new HttpRequestInterceptionBuilder()
                .ForHttp()
                .ForHost("localhost1")
                .ForPath("5051")
                .ForPath("health")
                .WithInterceptionCallback((_) => Task.Delay(latency))
                .WithJsonContent(new { name = "servicea", description = "Service A Is Healthy" });
        var serviceB = new HttpRequestInterceptionBuilder()
                        .ForHttp()
                        .ForHost("localhost2")
                        .ForPath("5052")
                        .ForPath("health")
                        .WithJsonContent(new { name = "serviceb", description = "Service B Is Healthy" });
        var serviceC = new HttpRequestInterceptionBuilder()
                        .ForHttp()
                        .ForHost("localhost3")
                        .ForPath("5053")
                        .ForPath("health")
                        .WithStatus(HttpStatusCode.InternalServerError);


        options.Register(serviceA, serviceB, serviceC);
        var httpClient = options.CreateHttpClient();

        // Arrange
        var timeout = TimeSpan.FromSeconds(1);
      
        var requester = new AsyncHttpRequester(httpClient, timeout);

        // Act
        var executedResults = requester.GetStringResultsAsync(Urls).GetAwaiter().GetResult();
        Console.WriteLine(executedResults);
        foreach ( var result in executedResults )
        {
            var expectedResponse = expectedResults[result.Key];
            Assert.IsTrue(result.Value.StatusCode == expectedResponse.StatusCode);
        }

        // Assert
        //Assert.IsTrue(results.ContainsKey(urls[0]));
        ///StringAssert.Contains(results[urls[0]], "Error:"); // Check for any error message
    }
}