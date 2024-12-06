using System.ComponentModel.DataAnnotations;

namespace UltraProxy.Models;

public sealed class DownloadModel
{
    [Required]
    [Url]
    [MaxLength(2048)]
    public required string Url { get; init; }
}
