using FS.Keycloak.RestApiClient.Authentication.ClientFactory;
using FS.Keycloak.RestApiClient.Authentication.Flow;
using FS.Keycloak.RestApiClient.Client;
using Handwerker.Application.Services.Keycloak.Interfaces;
using Microsoft.Extensions.Options;

namespace Handwerker.Application.Services.Keycloak.Models;


public sealed class KeycloakAdminApiFactory : IKeycloakAdminApiFactory
{
    private readonly KeycloakSettings _settings;

    public KeycloakAdminApiFactory(IOptions<KeycloakSettings> options)
    {
        _settings = options.Value;
    }

    public T Create<T>() where T : class, IApiAccessor
    {
        var credentials = new ClientCredentialsFlow
        {
            KeycloakUrl = _settings.BaseURL,
            Realm = _settings.Realm,
            ClientId = _settings.ClientId,
            ClientSecret = _settings.ClientSecret
        };

        var httpClient = AuthenticationHttpClientFactory.Create(credentials);

        return ApiClientFactory.Create<T>(httpClient);
    }
}

