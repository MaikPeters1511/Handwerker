using Handwerker.Application.Services;
using Handwerker.Application.Services.Keycloak;
using Handwerker.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Handwerker.ApiService.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class RecipientsController(RecipientService recipientService, IKcUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Recipient>>> GetRecipients()
    {
        var recipients = await recipientService.GetAllAsync();
        return Ok(recipients);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Recipient>> GetRecipient(int id)
    {
        var recipient = await recipientService.GetByIdAsync(id);

        if (recipient == null)
        {
            return NotFound();
        }

        return Ok(recipient);
    }

    [HttpPost]
    public async Task<ActionResult<Recipient>> PostRecipient([FromBody] Recipient recipient)
    {
        try
        {
            var created = await recipientService.CreateAsync(recipient);
            return CreatedAtAction("GetRecipient", new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Fehler beim Erstellen des Empfängers: {ex.Message}" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutRecipient(int id, [FromBody] Recipient recipient)
    {
        if (id != recipient.Id)
        {
            return BadRequest("ID stimmt nicht überein");
        }

        try
        {
            await recipientService.UpdateAsync(recipient);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Fehler beim Aktualisieren des Empfängers: {ex.Message}" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRecipient(int id)
    {
        try
        {
            await recipientService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Fehler beim Löschen des Empfängers: {ex.Message}" });
        }
    }
}
