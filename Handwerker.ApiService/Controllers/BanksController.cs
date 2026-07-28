using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Handwerker.Application.Services;
using Handwerker.Application.Services.Keycloak;
using Handwerker.Domain.Entities;

namespace Handwerker.ApiService.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class BanksController(BankService bankService, IKcUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetBanks()
    {
        var banks = await bankService.GetAsync();
        return Ok(banks);
    }
    
    [HttpGet("search")]
    public async Task<IActionResult> SearchBanks(string searchInput)
    {
        var banks = await bankService.SearchAsync(searchInput);
        return Ok(banks);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var bank = await bankService.GetByIdAsync(id);

        if (bank == null)
        {
            return NotFound();
        }

        return Ok(bank);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBank(Bank bank)
    {
        var result = await bankService.CreateAsync(bank);
        if (result == 0)
        {
            var userId = userService.GetCurrentUserId();
            //await notificationService.NotifyBankCreatedAsync(userId, bank.Id, bank.Name);

            return CreatedAtAction("GetBank", new { id = bank.Id }, bank);
        }
        else
        {
            var userId = userService.GetCurrentUserId();
            return NotFound();
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutBank(int id, [FromBody] Bank bank)
    {
        if (id != bank.Id)
        {
            return BadRequest("ID stimmt nicht überein");
        }

        try
        {
            await bankService.UpdateAsync(bank);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Fehler beim Aktualisieren der Bank: {ex.Message}" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBank(int id)
    {
        try
        {
            await bankService.DeleteAsync(id);
            var userId = userService.GetCurrentUserId();
            // await notificationService.NotifyBankDeletedAsync(userId, bankName);

            return Ok(); 
        }
        catch (Exception e)
        {
            return NotFound(); 
        }
    }
}

