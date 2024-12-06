using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UltraProxy.Models;

namespace UltraProxy.Controllers;

public sealed class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index() => View();

    public IActionResult Privacy() => View();

    [HttpPost]
    public async Task<IActionResult> Download(DownloadModel model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        _logger.LogInformation("Downloading from {Url}", model.Url);

        using var http = new HttpClient();
        using var result = await http.GetAsync(model.Url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        
        if(result.IsSuccessStatusCode)
            _logger.LogInformation("Completed with {StatusCode}", result.StatusCode);
        else
            _logger.LogWarning("Failed with {StatusCode}, error {Error}", result.StatusCode, result.ReasonPhrase);

        foreach (var header in result.Headers)
            Response.Headers[header.Key] = header.Value.First();
        
        foreach (var header in result.Content.Headers)
            Response.Headers[header.Key] = header.Value.First();
        
        _logger.LogInformation("Copying response");

        await result.Content.CopyToAsync(Response.Body, cancellationToken);
        await Response.Body.FlushAsync(cancellationToken);

        return Ok();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
