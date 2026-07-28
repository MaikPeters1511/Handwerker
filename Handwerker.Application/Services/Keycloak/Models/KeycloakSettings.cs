namespace Handwerker.Application.Services.Keycloak.Models;

public class KeycloakSettings
{
    public string BaseURL { get; set; }
    public string Authority { get; set; }
    public string Realm { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
}