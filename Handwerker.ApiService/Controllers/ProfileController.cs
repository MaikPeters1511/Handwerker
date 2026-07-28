using Handwerker.Application.Services.Keycloak;
using Handwerker.Application.Services.Keycloak.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace Handwerker.ApiService.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProfileController(IKcUserService userService, ILogger<ProfileController> logger) : ControllerBase
{
    /// <summary>
    /// Get the current user's profile
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<KcUser>> GetCurrentUserProfile()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "User ID not found in token" });
        }

        try
        {
            var user = await userService.GetUserByIdAsync(userId);
            logger.LogInformation("Retrieved profile for user {UserId} with attributes: {@Attributes}", userId, user.Attributes);
            return Ok(user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get profile for user {UserId}", userId);
            return NotFound(new { message = "User profile not found", error = ex.Message });
        }
    }

    /// <summary>
    /// Update the current user's profile
    /// </summary>
    [HttpPut]
    public async Task<ActionResult<KcUser>> UpdateCurrentUserProfile([FromBody] ProfileUpdateDto profileUpdate)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            logger.LogWarning("Update profile failed: User ID not found in token");
            return Unauthorized(new { message = "User ID not found in token" });
        }

        logger.LogInformation("Updating profile for user {UserId}", userId);
        logger.LogInformation("Received profile update: {@ProfileUpdate}", profileUpdate);

        try
        {
            // Hole das aktuelle Profil, um den Username zu erhalten
            var currentUser = await userService.GetUserByIdAsync(userId);
            logger.LogInformation("Current user attributes: {@Attributes}", currentUser.Attributes);
            
            // Merge Attributes: Verwende neue Werte, behalte aber bestehende bei wenn nicht überschrieben
            var mergedAttributes = currentUser.Attributes ?? new Dictionary<string, List<string>>();
            
            if (profileUpdate.Attributes != null)
            {
                logger.LogInformation("Processing {Count} attributes from update", profileUpdate.Attributes.Count);
                
                foreach (var attr in profileUpdate.Attributes)
                {
                    logger.LogInformation("  Attribute {Key}: {Value}", attr.Key, string.Join(", ", attr.Value ?? new List<string>()));
                    
                    // Nur hinzufügen/aktualisieren wenn der Wert nicht leer ist
                    if (attr.Value != null && attr.Value.Count > 0 && !string.IsNullOrWhiteSpace(attr.Value[0]))
                    {
                        mergedAttributes[attr.Key] = attr.Value;
                        logger.LogInformation("    → Added/Updated");
                    }
                    else
                    {
                        logger.LogInformation("    → Skipped (empty or null)");
                        // NICHT entfernen - bestehende Werte behalten!
                        // Das war der Fehler: Wir haben bestehende Attributes gelöscht
                    }
                }
            }
            
            // Konvertiere Frontend-Format in KcUserDto
            var userDto = new KcUserDto
            {
                UserName = profileUpdate.Username ?? currentUser.UserName,
                FirstName = profileUpdate.FirstName ?? currentUser.FirstName,
                LastName = profileUpdate.LastName ?? currentUser.LastName,
                Email = profileUpdate.Email ?? currentUser.Email,
                Enabled = true,
                Attributes = mergedAttributes
            };

            logger.LogInformation("Updating user {UserId} with attributes: {@Attributes}", userId, mergedAttributes);
            
            await userService.UpdateUserAsync(userId, userDto);
            var updatedUser = await userService.GetUserByIdAsync(userId);
            
            logger.LogInformation("Profile updated successfully for user {UserId}", userId);
            return Ok(updatedUser);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update profile for user {UserId}", userId);
            return BadRequest(new { message = "Failed to update user profile", error = ex.Message });
        }
    }

    /// <summary>
    /// DTO for profile updates from frontend
    /// </summary>
    public class ProfileUpdateDto
    {
        [JsonPropertyName("username")]
        public string? Username { get; set; }
        
        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }
        
        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }
        
        [JsonPropertyName("email")]
        public string? Email { get; set; }
        
        [JsonPropertyName("attributes")]
        public Dictionary<string, List<string>>? Attributes { get; set; }
    }

    /// <summary>
    /// Upload profile image for current user
    /// </summary>
    [HttpPost("image")]
    public async Task<ActionResult<object>> UploadProfileImage([FromForm] IFormFile? image)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "User ID not found in token" });
        }

        if (image == null || image.Length == 0)
        {
            return BadRequest(new { message = "No image file provided" });
        }

        // Validate file type
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(image.ContentType.ToLower()))
        {
            return BadRequest(new { message = "Invalid image type. Only JPEG, PNG, GIF, and WebP are allowed." });
        }

        // Validate file size (max 5MB)
        if (image.Length > 5 * 1024 * 1024)
        {
            return BadRequest(new { message = "Image file is too large. Maximum size is 5MB." });
        }

        try
        {
            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
            Directory.CreateDirectory(uploadsPath);

            // Generate unique filename
            var fileExtension = Path.GetExtension(image.FileName);
            var fileName = $"{userId}{fileExtension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Delete old image if exists
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            // Save new image
            await using var stream = new FileStream(filePath, FileMode.Create);
            await image.CopyToAsync(stream);

            // Return image URL
            var imageUrl = $"/uploads/profiles/{fileName}";
            return Ok(new { imageUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to upload image", error = ex.Message });
        }
    }

    /// <summary>
    /// Get profile image URL for current user
    /// </summary>
    [HttpGet("image")]
    public ActionResult<object> GetProfileImage()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "User ID not found in token" });
        }

        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
        var extensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        foreach (var ext in extensions)
        {
            var fileName = $"{userId}{ext}";
            var filePath = Path.Combine(uploadsPath, fileName);
            
            if (System.IO.File.Exists(filePath))
            {
                var imageUrl = $"/uploads/profiles/{fileName}";
                return Ok(new { imageUrl });
            }
        }

        return Ok(new { imageUrl = (string?)null });
    }

    /// <summary>
    /// Delete profile image for current user
    /// </summary>
    [HttpDelete("image")]
    public IActionResult DeleteProfileImage()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "User ID not found in token" });
        }

        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
        var extensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        var deleted = false;
        foreach (var ext in extensions)
        {
            var fileName = $"{userId}{ext}";
            var filePath = Path.Combine(uploadsPath, fileName);
            
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                deleted = true;
            }
        }

        if (!deleted)
        {
            return NotFound(new { message = "No profile image found" });
        }

        return Ok(new { message = "Profile image deleted successfully" });
    }

    /// <summary>
    /// Get the current user's ID from the JWT token
    /// </summary>
    private string? GetCurrentUserId()
    {
        // Keycloak setzt die User-ID im "sub" (subject) Claim
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
               ?? User.FindFirst("sub")?.Value;
    }
}


