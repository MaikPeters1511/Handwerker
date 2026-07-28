using Handwerker.Application.Services;
using Handwerker.ApiService.Models;
using Handwerker.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Handwerker.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OffersController(
    OfferService offerService,
    NotificationService notificationService) : ControllerBase
{
    /// <summary>
    /// Lädt alle Angebote
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetOffers(CancellationToken cancellationToken = default)
    {
        try
        {
            var offers = await offerService.GetAllAsync(cancellationToken);
            var dtos = offers.Select(MapToDto);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Lädt ein spezifisches Angebot mit Details
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOffer(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var offer = await offerService.GetByIdAsync(id, cancellationToken);
            if (offer == null)
            {
                return NotFound(new { error = "Angebot nicht gefunden." });
            }

            var dto = MapToDetailDto(offer);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Lädt alle geschriebenen Angebote
    /// </summary>
    [HttpGet("sent")]
    public async Task<IActionResult> GetSentOffers(CancellationToken cancellationToken = default)
    {
        try
        {
            var offers = await offerService.GetSentOffersAsync(cancellationToken);
            var dtos = offers.Select(MapToDto);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Lädt alle empfangenen Angebote
    /// </summary>
    [HttpGet("received")]
    public async Task<IActionResult> GetReceivedOffers(CancellationToken cancellationToken = default)
    {
        try
        {
            var offers = await offerService.GetReceivedOffersAsync(cancellationToken);
            var dtos = offers.Select(MapToDto);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Erstellt ein neues Angebot
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateOffer(
        [FromBody] CreateOfferRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var offer = new Offer
            {
                OfferDate = request.OfferDate,
                ValidUntil = request.ValidUntil,
                CustomerNumber = request.CustomerNumber,
                Recipient = request.Recipient,
                Provider = request.Provider,
                Products = request.Products,
                TotalNet = request.TotalNet,
                TotalTaxAmount = request.TotalTaxAmount,
                TotalGross = request.TotalGross,
                Status = request.Status,
                IntroText = request.IntroText,
                OutroText = request.OutroText,
                Notes = request.Notes,
                IsReceived = request.IsReceived
            };

            offerService.ValidateOffer(offer);
            var created = await offerService.CreateAsync(offer, cancellationToken);

            // Notification erstellen
            var userId = User.FindFirst("sub")?.Value ?? "system";
            await notificationService.NotifyOfferCreatedAsync(userId, created.Id, created.OfferNumber);

            return CreatedAtAction(nameof(GetOffer), new { id = created.Id }, MapToDetailDto(created));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            var userId = User.FindFirst("sub")?.Value ?? "system";
            await notificationService.NotifyErrorAsync(userId, $"Fehler beim Erstellen des Angebots: {ex.Message}");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Aktualisiert ein bestehendes Angebot
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOffer(
        int id,
        [FromBody] UpdateOfferRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id != request.Id)
        {
            return BadRequest(new { error = "ID in URL und Body stimmen nicht überein." });
        }

        try
        {
            var existing = await offerService.GetByIdAsync(id, cancellationToken);
            if (existing == null)
            {
                return NotFound(new { error = "Angebot nicht gefunden." });
            }

            var offer = new Offer
            {
                Id = request.Id,
                OfferNumber = request.OfferNumber,
                OfferDate = request.OfferDate,
                ValidUntil = request.ValidUntil,
                CustomerNumber = request.CustomerNumber,
                Recipient = request.Recipient,
                Provider = request.Provider,
                Products = request.Products,
                TotalNet = request.TotalNet,
                TotalTaxAmount = request.TotalTaxAmount,
                TotalGross = request.TotalGross,
                Status = request.Status,
                IntroText = request.IntroText,
                OutroText = request.OutroText,
                Notes = request.Notes,
                IsReceived = request.IsReceived,
                ConvertedToOrderId = request.ConvertedToOrderId
            };

            offerService.ValidateOffer(offer);
            await offerService.UpdateAsync(offer, cancellationToken);

            // Notification
            var userId = User.FindFirst("sub")?.Value ?? "system";
            await notificationService.NotifyOfferUpdatedAsync(userId, offer.Id, offer.OfferNumber);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Löscht ein Angebot
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOffer(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await offerService.GetByIdAsync(id, cancellationToken);
            if (existing == null)
            {
                return NotFound(new { error = "Angebot nicht gefunden." });
            }

            await offerService.DeleteAsync(id, cancellationToken);

            // Notification
            var userId = User.FindFirst("sub")?.Value ?? "system";
            await notificationService.NotifyOfferDeletedAsync(userId, existing.OfferNumber);

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Wandelt ein Angebot in einen Auftrag um
    /// </summary>
    [HttpPost("{id}/convert-to-order")]
    public async Task<IActionResult> ConvertToOrder(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var orderId = await offerService.ConvertToOrderAsync(id, cancellationToken);

            // Notification
            var userId = User.FindFirst("sub")?.Value ?? "system";
            await notificationService.NotifyOfferConvertedAsync(userId, id);

            return Ok(new { orderId, message = "Angebot wurde erfolgreich in einen Auftrag umgewandelt." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Helper: Map Offer to OfferDto
    private static OfferDto MapToDto(Offer offer) => new()
    {
        Id = offer.Id,
        OfferNumber = offer.OfferNumber,
        OfferDate = offer.OfferDate,
        ValidUntil = offer.ValidUntil,
        CustomerNumber = offer.CustomerNumber,
        CustomerName = offer.Recipient.Name,
        TotalNet = offer.TotalNet,
        TotalGross = offer.TotalGross,
        Status = offer.Status,
        IsReceived = offer.IsReceived,
        ConvertedToOrderId = offer.ConvertedToOrderId
    };

    // Helper: Map Offer to OfferDetailDto
    private static OfferDetailDto MapToDetailDto(Offer offer) => new()
    {
        Id = offer.Id,
        OfferNumber = offer.OfferNumber,
        OfferDate = offer.OfferDate,
        ValidUntil = offer.ValidUntil,
        CustomerNumber = offer.CustomerNumber,
        Recipient = offer.Recipient,
        Provider = offer.Provider,
        Products = offer.Products,
        TotalNet = offer.TotalNet,
        TotalTaxAmount = offer.TotalTaxAmount,
        TotalGross = offer.TotalGross,
        Status = offer.Status,
        IntroText = offer.IntroText,
        OutroText = offer.OutroText,
        Notes = offer.Notes,
        IsReceived = offer.IsReceived,
        ConvertedToOrderId = offer.ConvertedToOrderId
    };
}
