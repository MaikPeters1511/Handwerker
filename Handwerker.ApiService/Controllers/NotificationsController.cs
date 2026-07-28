using Handwerker.Application.Services;
using Handwerker.Application.Services.Keycloak;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Handwerker.ApiService.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class NotificationsController(
    NotificationService notificationService, 
    IKcUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        [FromQuery] bool? isRead = null)
    {
        var userId = userService.GetCurrentUserId();

        if (userId == null)
        {
            return BadRequest();
        }
        
        var result = await notificationService.GetNotificationsAsync(userId, skip, take, isRead);
        return Ok(result);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount()
    {
        var userId = userService.GetCurrentUserId();

        if (userId == null)
        {
            return BadRequest();
        }

        var result = await notificationService.CountUnreadNotificationsAsync(userId);
        return Ok(result);
    }

    [HttpPut("{id}/mark-read")]
    public async Task<IActionResult> MarkAsRead(int notiId)
    {
        var userId = userService.GetCurrentUserId();

        if (userId == null || notiId == 0)
        {
            return BadRequest();
        }

        try
        {
            await notificationService.MarkAsReadAsync(notiId, userId);
            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

    }

    [HttpPut("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = userService.GetCurrentUserId();

        if (userId == null)
        {
            return BadRequest();
        }

        try
        {
            await notificationService.MarkAllAsReadAsync(userId);
            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(int notiId)
    {
        var userId = userService.GetCurrentUserId();

        if (userId == null || notiId == 0)
        {
            return BadRequest();
        }

        try
        {
            await notificationService.DeleteNotificationAsync(notiId, userId);
            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpDelete("clear-all")]
    public async Task<IActionResult> ClearAll()
    {
        var userId = userService.GetCurrentUserId();

        if (userId == null)
        {
            return BadRequest();
        }
        
        try
        {
            await notificationService.DeleteAllNotificationAsync(userId);
            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
