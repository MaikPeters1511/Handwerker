using FS.Keycloak.RestApiClient.Client;

namespace Handwerker.Application.Services.Keycloak.Interfaces;

public interface IKeycloakAdminApiFactory
{
    T Create<T>() where T : class, IApiAccessor;
}