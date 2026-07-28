using Handwerker.Application.Services.Keycloak.Models;

namespace Handwerker.Application.Services.Keycloak;

public interface IKcUserService
{
    public string? GetCurrentUserId();
    Task<IEnumerable<KcUser>> GetUsersAsync();
    Task<KcUser> GetUserByIdAsync(string id);
    Task<int> GetUsersCountAsync();
    
    Task DeleteUserAsync(string id);
    Task CreateUserAsync(KcUserDto user);
    Task UpdateUserAsync(string userId, KcUserDto user);

    #region Users-Roles

    Task<IEnumerable<KcRole>> GetRolesAsync();
    Task<KcRole> GetRoleByNameAsync(string name);
    Task<IEnumerable<KcRole>> GetUserRolesAsync(string userId);
    Task<IEnumerable<KcRole>> GetAllUserRolesAvailableAsync(string userId);
    
    Task CreateUserRoleMappingsAsync(string userId, List<KcRole> kcRoles);
    Task DeleteUserRoleMappingsAsync(string userId, List<KcRole> kcRoles);

    #endregion
}