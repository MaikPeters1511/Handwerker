using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Handwerker.Application.Services;
using Handwerker.Application.Services.Keycloak;
using Handwerker.Domain.Entities;

namespace Handwerker.ApiService.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProvidersController(ProviderService providerService, IKcUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProviders(int page = 1, int pageSize = 50)
    {
        var result = await providerService.GetAsync(page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProvider(int id)
    {
        var provider = await providerService.GetByIdAsync(id);

        if (provider == null)
        {
            return NotFound();
        }

        return Ok(provider);
    }

    [HttpPost]
    public async Task<ActionResult<Provider>> PostProvider([FromBody] Provider provider)
    {
        try
        {
            var created = await providerService.CreateAsync(provider);
            return CreatedAtAction("GetProvider", new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Fehler beim Erstellen des Lieferanten: {ex.Message}" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutProvider(int id, [FromBody] Provider provider)
    {
        if (id != provider.Id)
        {
            return BadRequest("ID stimmt nicht überein");
        }

        try
        {
            await providerService.UpdateAsync(provider);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Fehler beim Aktualisieren des Lieferanten: {ex.Message}" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProvider(int id)
    {
        try
        {
            await providerService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Fehler beim Löschen des Lieferanten: {ex.Message}" });
        }
    }
}