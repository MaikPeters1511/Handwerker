using Handwerker.Application.Services;
using Handwerker.Application.Services.Keycloak;
using Handwerker.Application.Services.Keycloak.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Handwerker.ApiService.Controllers;

[ApiController]
[Authorize(Roles = "admin")]
[Route("api/[controller]")]
public class UsersController(IKcUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUsersAsync()
    {
        var result = await userService.GetUsersAsync();
        return Ok(result);
    }

    [HttpGet("count")]
    public async Task<int> GetUserCountAsync()
    {
        var result = await userService.GetUsersCountAsync();
        return result;
    }

    [HttpGet("{userId}")]
    public async Task<KcUser> GetUserAsync(string userId)
    {
        var result = await userService.GetUserByIdAsync(userId);
        return result;
    }

    [ServiceFilter(typeof(FluentValidationFilter))]
    [HttpPost]
    public async Task<IActionResult> CreateUserAsync([FromBody] KcUserDto user)
    {
        try
        {
            await userService.CreateUserAsync(user); 
            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    [ServiceFilter(typeof(FluentValidationFilter))]
    [HttpPut("{userId}")]
    
    public async Task<IActionResult> UpdateUserAsync(string userId, [FromBody] KcUserDto user)
    {
        try
        {
            await userService.UpdateUserAsync(userId, user); 
            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteUserAsync(string userId)
    {
        try
        {
            await userService.DeleteUserAsync(userId); 
            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    [HttpGet("roles")]
    public async Task<IActionResult> GetRolesAsync()
    {
        var result = await userService.GetRolesAsync();
        return Ok(result);
    }
    
    [HttpGet("{userId}/roles")]
    public async Task<IActionResult> GetUserRolesAsync(string userId)
    {
        var result = await userService.GetUserRolesAsync(userId);
        return Ok(result);
    }
    
    [HttpGet("{userId}/roles/available")]
    public async Task<IActionResult> GetUserRolesAvailableAsync(string userId)
    {
        var result = await userService.GetAllUserRolesAvailableAsync(userId);
        return Ok(result);
    }

    [HttpPost("{userId}/roles/create")]
    public async Task<IActionResult> CreateUserRoleMappingsAsync(string userId, [FromBody] List<KcRole> kcRoles)
    {
        try
        {
            await userService.CreateUserRoleMappingsAsync(userId, kcRoles); 
            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    [HttpDelete("{userId}/roles/delete")]
    public async Task<IActionResult> DeleteUserRoleMappingsAsync(string userId, [FromBody] List<KcRole> kcRoles)
    {
        try
        {
            await userService.DeleteUserRoleMappingsAsync(userId, kcRoles); 
            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}