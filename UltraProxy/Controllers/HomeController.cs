using System.Diagnostics;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using UltraProxy.Models;

namespace UltraProxy.Controllers;

public sealed class HomeController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IHttpClientFactory httpClientFactory, ILogger<HomeController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public IActionResult Index() => View();

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

    [HttpPost]
    public async Task<IActionResult> Download(DownloadModel model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        _logger.LogDownloadingFromUrl(model.Url);

        var http = _httpClientFactory.CreateClient();
        using var result = await http.GetAsync(model.Url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        
        if(result.IsSuccessStatusCode)
            _logger.LogCompletedWithStatuscode(result.StatusCode);
        else
            _logger.LogFailedWithStatuscodeErrorError(result.StatusCode, result.ReasonPhrase);

        Response.ContentType = MediaTypeNames.Application.Octet;
        var upstreamDisposition = result.Content.Headers.ContentDisposition;
        var fileName = GetSafeFileName(upstreamDisposition?.FileNameStar)
                       ?? GetSafeFileName(upstreamDisposition?.FileName)
                       ?? GetSafeFileName(Uri.UnescapeDataString(new Uri(model.Url).Segments[^1]))
                       ?? "download.bin";
        var attachment = new Microsoft.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
        attachment.SetHttpFileName(fileName);
        Response.GetTypedHeaders().ContentDisposition = attachment;
        Response.Headers.XContentTypeOptions = "nosniff";
        
        _logger.LogCopyingResponse();

        await result.Content.CopyToAsync(Response.Body, cancellationToken);
        await Response.Body.FlushAsync(cancellationToken);
        
        _logger.LogProxyFinished();

        return new EmptyResult();
    }

    private static string? GetSafeFileName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var name = Path.GetFileName(value.Trim('"').Replace('\\', '/')).Trim();
        if (name.Length is 0 or > 255 || name is "." or ".." ||
            name.Any(c => char.IsControl(c) || char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.Format))
            return null;

        return name;
    }
}
