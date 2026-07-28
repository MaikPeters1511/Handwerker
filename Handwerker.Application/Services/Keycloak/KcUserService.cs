using System.Security.Claims;
using AutoMapper;
using FS.Keycloak.RestApiClient.Api;
using FS.Keycloak.RestApiClient.Model;
using Handwerker.Application.Services.Keycloak.Interfaces;
using Handwerker.Application.Services.Keycloak.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Handwerker.Application.Services.Keycloak;

public class KcUserService : IKcUserService
{
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IKeycloakAdminApiFactory _keycloakAdminApiFactory;
    private readonly string _realm;
    public KcUserService(IHttpContextAccessor httpContextAccessor, 
        IMapper mapper, 
        IKeycloakAdminApiFactory keycloakAdminApiFactory,
        IOptions<KeycloakSettings> options)
    {
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
        _keycloakAdminApiFactory = keycloakAdminApiFactory;
        _realm =  options.Value.Realm;
    }
    public async Task<IEnumerable<KcUser>> GetUsersAsync()
    {
        var usersApi = _keycloakAdminApiFactory.Create<UsersApi>();
        var result = await usersApi.GetUsersAsync(_realm);

        return _mapper.Map<IEnumerable<KcUser>>(result); 
    }

    public async Task<KcUser> GetUserByIdAsync(string id)
    {
        var usersApi = _keycloakAdminApiFactory.Create<UsersApi>();
        var result = await usersApi.GetUsersByUserIdAsync(_realm, id);
        
        // Debug-Logging: Was kommt von Keycloak?
        Console.WriteLine($"Retrieved user {id} from Keycloak:");
        Console.WriteLine($"  FirstName: {result.FirstName}");
        Console.WriteLine($"  LastName: {result.LastName}");
        Console.WriteLine($"  Email: {result.Email}");
        Console.WriteLine($"  Attributes is null: {result.Attributes == null}");
        Console.WriteLine($"  Attributes count: {result.Attributes?.Count ?? 0}");
        Console.WriteLine($"  Attributes JSON: {System.Text.Json.JsonSerializer.Serialize(result.Attributes)}");
        
        var mapped = _mapper.Map<KcUser>(result);
        Console.WriteLine($"After AutoMapper:");
        Console.WriteLine($"  Mapped.Attributes is null: {mapped.Attributes == null}");
        Console.WriteLine($"  Mapped.Attributes count: {mapped.Attributes?.Count ?? 0}");
        Console.WriteLine($"  Mapped.Attributes JSON: {System.Text.Json.JsonSerializer.Serialize(mapped.Attributes)}");
        
        return mapped; 
    }

    public string? GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        var userId =
            user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
            user?.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(userId))
            throw new UnauthorizedAccessException("User ID not found in token");

        return userId;
    }
    public async Task<int> GetUsersCountAsync()
    {
        var usersApi = _keycloakAdminApiFactory.Create<UsersApi>();
        return await usersApi.GetUsersCountAsync(_realm);
    }

    public async Task DeleteUserAsync(string id)
    {
        var usersApi = _keycloakAdminApiFactory.Create<UsersApi>();
        await usersApi.DeleteUsersByUserIdAsync(_realm, id);
    }

    public async Task CreateUserAsync(KcUserDto user)
    {
        var usersApi = _keycloakAdminApiFactory.Create<UsersApi>();
        var userRepresentation = _mapper.Map<UserRepresentation>(user);
        await usersApi.PostUsersAsync(_realm, userRepresentation);
    }

    public async Task UpdateUserAsync(string userId, KcUserDto user)
    {
        var usersApi = _keycloakAdminApiFactory.Create<UsersApi>();
        var userRepresentation = _mapper.Map<UserRepresentation>(user);
        
        // Debug-Logging: Was wird an Keycloak gesendet?
        Console.WriteLine($"Updating user {userId} with UserRepresentation:");
        Console.WriteLine($"  FirstName: {userRepresentation.FirstName}");
        Console.WriteLine($"  LastName: {userRepresentation.LastName}");
        Console.WriteLine($"  Email: {userRepresentation.Email}");
        Console.WriteLine($"  Attributes: {System.Text.Json.JsonSerializer.Serialize(userRepresentation.Attributes)}");
        
        await usersApi.PutUsersByUserIdAsync(_realm, userId, userRepresentation);
    }

    public async Task<IEnumerable<KcRole>> GetRolesAsync()
    {
        var roleApi = _keycloakAdminApiFactory.Create<RolesApi>();
        var result = await roleApi.GetRolesAsync(_realm);
        return _mapper.Map<List<KcRole>>(result);
    }

    public async Task<KcRole> GetRoleByNameAsync(string name)
    {
        var roleApi = _keycloakAdminApiFactory.Create<RolesApi>();
        var result = await roleApi.GetRolesByRoleNameAsync(_realm, name);
        return _mapper.Map<KcRole>(result); 
    }

    public async Task<IEnumerable<KcRole>> GetUserRolesAsync(string userId)
    {
        var roleMapperApi = _keycloakAdminApiFactory.Create<RoleMapperApi>();
        var result = await roleMapperApi.GetUsersRoleMappingsRealmByUserIdAsync(_realm, userId);
        return _mapper.Map<List<KcRole>>(result); 
    }

    public async Task<IEnumerable<KcRole>> GetAllUserRolesAvailableAsync(string userId)
    {
        var roleMapperApi = _keycloakAdminApiFactory.Create<RoleMapperApi>();
        var result = await roleMapperApi.GetUsersRoleMappingsRealmAvailableByUserIdAsync(_realm, userId);
        return _mapper.Map<List<KcRole>>(result); 
    }

    public async Task CreateUserRoleMappingsAsync(string userId, List<KcRole> kcRoles)
    {
        var roleMapperApi = _keycloakAdminApiFactory.Create<RoleMapperApi>();
        var roleRepresentation = _mapper.Map<List<RoleRepresentation>>(kcRoles);
        await roleMapperApi.PostUsersRoleMappingsRealmByUserIdAsync(_realm, userId, roleRepresentation);
    }

    public async Task DeleteUserRoleMappingsAsync(string userId, List<KcRole> kcRoles)
    {
        var roleMapperApi = _keycloakAdminApiFactory.Create<RoleMapperApi>();
        var roleRepresentation = _mapper.Map<List<RoleRepresentation>>(kcRoles);
        await roleMapperApi.DeleteUsersRoleMappingsRealmByUserIdAsync(_realm, userId, roleRepresentation);
    }
}

