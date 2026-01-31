//using Microsoft.Extensions.Logging;
//using ModularPipelines.Http;
//using ModularPipelines.Logging;
//using System.Net;

//namespace Kemartec.Checkliste.Build.Http;

//public class MyHttpLogger : IHttpLogger
//{
//    private MyHttpRequestFormatter _requestFormatter = new();
//    private MyHttpResponseFormatter _responseFormatter = new();

//    public async Task PrintRequest(HttpRequestMessage request, IModuleLogger logger)
//    {
//        var formattedRequest = await _requestFormatter.FormatAsync(request);
//        logger.LogInformation("HTTP Request:\n{Request}", formattedRequest);
//    }

//    public async Task PrintResponse(HttpResponseMessage response, IModuleLogger logger)
//    {
//        var formattedResponse = await _responseFormatter.FormatAsync(response);
//        logger.LogInformation("HTTP Response:\n{Response}", formattedResponse);
//    }

//    public void PrintStatusCode(HttpStatusCode? httpStatusCode, IModuleLogger logger)
//    {
//        var statusCode = httpStatusCode == null ? null as int? : (int) httpStatusCode;
//        var icon = statusCode is >= 200 and < 300 ? "✓" : "✗";

//        logger.LogInformation("{Icon} HTTP Status: {StatusCode} {HttpStatusCode}", icon, statusCode, httpStatusCode);
//    }

//    public void PrintDuration(TimeSpan duration, IModuleLogger logger)
//    {
//        logger.LogInformation("Duration: {Duration}", duration);
//    }
//}
