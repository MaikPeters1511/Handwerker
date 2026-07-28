using System.Text.Json.Serialization;

namespace Handwerker.Application.Services.Keycloak.Models;

public class KcUserDto
{
    [JsonPropertyName("username")]
    public string UserName { get; set; } = string.Empty;
    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;
    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;
    [JsonPropertyName("attributes")]
    public Dictionary<string, List<string>>? Attributes { get; set; }
}