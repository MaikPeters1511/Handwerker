using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Handwerker.ApiService.Controllers;

/// <summary>
/// Gemeinsame Basisklasse für alle API-Controller.
/// Stellt wiederverwendbare Hilfsmethoden bereit (UserId, UserName).
/// </summary>
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected string GetUserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("sub")?.Value
        ?? "system";

    protected string GetUserName() =>
        User.FindFirst(ClaimTypes.Name)?.Value
        ?? User.FindFirst("name")?.Value
        ?? GetUserId();
}

