using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace Handwerker.ApiService;

/// <summary>
/// Transformiert Keycloak-Rollen aus dem <c>realm_access.roles</c> Claim
/// in Standard-ASP.NET-Core-Rollen-Claims, sodass <c>[Authorize(Roles = "admin")]</c>
/// und <c>User.IsInRole("admin")</c> korrekt funktionieren.
/// </summary>
public sealed class KeycloakRolesClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // Nur transformieren, wenn der Nutzer authentifiziert ist
        if (principal.Identity is not { IsAuthenticated: true })
            return Task.FromResult(principal);

        // Prüfen, ob bereits Rollen-Claims vorhanden sind (verhindert Doppel-Transformation)
        if (principal.HasClaim(c => c.Type == ClaimTypes.Role))
            return Task.FromResult(principal);

        var identity = (ClaimsIdentity)principal.Identity!;

        // realm_access Claim auslesen
        var realmAccessClaim = principal.FindFirst("realm_access");
        if (realmAccessClaim is null)
            return Task.FromResult(principal);

        try
        {
            using var doc = JsonDocument.Parse(realmAccessClaim.Value);
            if (!doc.RootElement.TryGetProperty("roles", out var rolesElement))
                return Task.FromResult(principal);

            foreach (var role in rolesElement.EnumerateArray())
            {
                var roleName = role.GetString();
                if (!string.IsNullOrWhiteSpace(roleName))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                }
            }
        }
        catch (JsonException)
        {
            // Ungültiger JSON-Claim – ignorieren
        }

        return Task.FromResult(principal);
    }
}

