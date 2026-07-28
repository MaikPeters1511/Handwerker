namespace Handwerker.ApiService.Models;

public class SendTestEmailRequest
{
    public required string To { get; init; }
    public required string Subject { get; init; }
    public required string Body { get; init; }
}