using System.Text.Json.Serialization;

namespace Handwerker.Application.Services.Keycloak.Models;

public class KcRole
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("description")]
    public string Description { get; set; }
}