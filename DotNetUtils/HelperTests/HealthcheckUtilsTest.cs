using Helpers;
using JustEat.HttpClientInterception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Helpers.AsyncHttpRequester;

namespace HelperTests;
[TestClass]
public class HealthcheckUtilsTest
{
    [TestMethod]
    public async Task  MixOfHealthyAndUnhealthyResults()
    {
        List<string> Urls = new List<string>()
        {
            "http://localhost1:5051/health", 
            "http://localhost2:5052/health",
            "http://localhost3:5053/health"
        };
        //Dictionary<string, ApiResult> expectedResults = new Dictionary<string, ApiResult>()
        //{
        //    {
        //        "http://localhost1:5051/health",
        //        new ApiResult() { StatusCode=HttpStatusCode.ServiceUnavailable}
        //    },
        //     {
        //        "http://localhost2:5052/health",
        //        new ApiResult() { StatusCode=HttpStatusCode.OK}
        //    }
        //    ,
        //     {
        //        "http://localhost3:5053/health",
        //        new ApiResult() { StatusCode=HttpStatusCode.InternalServerError}
        //    }
        //};


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

        
        var results  = await HealthcheckUtils.Check(httpClient, Urls, timeout);

        // Act
        foreach (var result in results)
        {
            Console.WriteLine(result);
        }

        //var executedResults = requester.GetStringResultsAsync(Urls).GetAwaiter().GetResult();
        //Console.WriteLine(executedResults);
        //foreach (var result in executedResults)
        //{
        //    var expectedResponse = expectedResults[result.Key];
        //    Assert.IsTrue(result.Value.StatusCode == expectedResponse.StatusCode);
        //}

        // Assert
        var requestCancelledResult = results[0];
        var successResult = results[1];
        var otherFailureResult = results[2];
        Assert.AreEqual(HealthStatus.Unhealthy,requestCancelledResult.HealthStatus );
        Assert.AreEqual(Description.RequestCancelled,requestCancelledResult.Description );

        Assert.AreEqual(HealthStatus.Healthy,successResult.HealthStatus );
        Assert.AreEqual( Description.Success,successResult.Description);

        Assert.AreEqual( HealthStatus.Unhealthy,otherFailureResult.HealthStatus);
        Assert.AreEqual(Description.FailedWithException,otherFailureResult.Description );

        //Assert.IsTrue(results.ContainsKey(urls[0]));
        ///StringAssert.Contains(results[urls[0]], "Error:"); // Check for any error message
    }

    [TestMethod]
    public async Task AllSucessResults()
    {
        List<string> Urls = new List<string>()
        {
            "http://localhost1:5051/health",
            "http://localhost2:5052/health",
            "http://localhost3:5053/health"
        };
       
        // Arrange
        var options = new HttpClientInterceptorOptions();
        var serviceA = new HttpRequestInterceptionBuilder()
                .ForHttp()
                .ForHost("localhost1")
                .ForPath("5051")
                .ForPath("health")
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
                        .WithJsonContent(new { name = "serviceb", description = "Service C Is Healthy" });


        options.Register(serviceA, serviceB, serviceC);
        var httpClient = options.CreateHttpClient();

        // Arrange
        var timeout = TimeSpan.FromSeconds(1);


        var results = await HealthcheckUtils.Check(httpClient, Urls, timeout);

        // Act
        foreach (var result in results)
        {
            Assert.AreEqual(HealthStatus.Healthy, result.HealthStatus);
            Assert.AreEqual(Description.Success, result.Description);
        }
                
    }
}
