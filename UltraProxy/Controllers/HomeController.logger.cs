using System.Net;

namespace UltraProxy.Controllers;

public static partial class HomeControllerEx
{
    [LoggerMessage(LogLevel.Information, "Downloading from {Url}")]
    public static partial void LogDownloadingFromUrl(this ILogger<HomeController> logger, string url);
    
    [LoggerMessage(LogLevel.Information, "Completed with {StatusCode}")]
    public static partial void LogCompletedWithStatuscode(this ILogger<HomeController> logger, HttpStatusCode statusCode);
    
    [LoggerMessage(LogLevel.Warning, "Failed with {StatusCode}, error {Error}")]
    public static partial void LogFailedWithStatuscodeErrorError(this ILogger<HomeController> logger, HttpStatusCode statusCode, string? error);
    
    [LoggerMessage(LogLevel.Information, "Copying response")]
    public static partial void LogCopyingResponse(this ILogger<HomeController> logger);
    
    [LoggerMessage(LogLevel.Information, "Proxy finished")]
    public static partial void LogProxyFinished(this ILogger<HomeController> logger);
}