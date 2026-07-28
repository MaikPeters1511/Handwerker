using Handwerker.Application.Abstractions;
using Handwerker.Application.Features.Invoices.Commands;
using Handwerker.Application.Features.Invoices.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Handwerker.ApiService.Controllers;

[Authorize]
[Route("api/[controller]")]
public class InvoicesController(
    IQueryDispatcher queryDispatcher,
    ICommandDispatcher commandDispatcher) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetInvoices(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await queryDispatcher.DispatchAsync<GetInvoicesQuery, IReadOnlyList<InvoiceListItemDto>>(
            new GetInvoicesQuery(page, pageSize, search, status), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetInvoice(int id, CancellationToken cancellationToken = default)
    {
        var result = await queryDispatcher.DispatchAsync<GetInvoiceByIdQuery, InvoiceDetailDto?>(
            new GetInvoiceByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("next-invoice-number")]
    public async Task<IActionResult> GetNextInvoiceNumber(CancellationToken cancellationToken = default)
    {
        var number = await queryDispatcher.DispatchAsync<GetNextInvoiceNumberQuery, string>(
            new GetNextInvoiceNumberQuery(GetUserId()), cancellationToken);
        return Ok(number);
    }

    [HttpPost]
    public async Task<IActionResult> PostInvoice(
        [FromBody] CreateInvoiceRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.RecipientId <= 0) return BadRequest(new { error = "RecipientId muss gesetzt sein." });
        if (request.ProviderId <= 0)  return BadRequest(new { error = "ProviderId muss gesetzt sein." });

        var id = await commandDispatcher.DispatchAsync<CreateInvoiceCommand, int>(
            new CreateInvoiceCommand(
                GetUserId(), request.RecipientId, request.ProviderId,
                request.InvoiceNumber, request.InvoiceDate, request.DueDate,
                request.ServicePeriod, request.CustomerNumber, request.PaymentTerms,
                request.IntroText, request.OutroText,
                request.TotalNet, request.TotalTaxAmount, request.TotalGross,
                request.Products),
            cancellationToken);

        return CreatedAtAction(nameof(GetInvoice), new { id }, new { id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutInvoice(
        int id,
        [FromBody] UpdateInvoiceRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id != request.Id)         return BadRequest(new { error = "ID stimmt nicht überein." });
        if (request.RecipientId <= 0) return BadRequest(new { error = "RecipientId muss gesetzt sein." });
        if (request.ProviderId <= 0)  return BadRequest(new { error = "ProviderId muss gesetzt sein." });

        await commandDispatcher.DispatchAsync(
            new UpdateInvoiceCommand(
                id, GetUserId(), request.RecipientId, request.ProviderId,
                request.InvoiceNumber, request.InvoiceDate, request.DueDate,
                request.ServicePeriod, request.CustomerNumber, request.PaymentTerms,
                request.IntroText, request.OutroText, request.IsPaid,
                request.TotalNet, request.TotalTaxAmount, request.TotalGross,
                request.Products),
            cancellationToken);

        return NoContent();
    }

    [HttpPatch("{id:int}/paid")]
    public async Task<IActionResult> MarkPaid(
        int id,
        [FromBody] MarkPaidRequest request,
        CancellationToken cancellationToken = default)
    {
        await commandDispatcher.DispatchAsync(
            new MarkInvoicePaidCommand(id, GetUserId(), request.IsPaid),
            cancellationToken);
        return NoContent();
    }

    [HttpPost("convert-from-offer/{offerId:int}")]
    public async Task<IActionResult> ConvertFromOffer(
        int offerId,
        [FromQuery] bool includeOfferLines = true,
        CancellationToken cancellationToken = default)
    {
        var id = await commandDispatcher.DispatchAsync<ConvertOfferToInvoiceCommand, int>(
            new ConvertOfferToInvoiceCommand(offerId, GetUserId(), includeOfferLines),
            cancellationToken);
        return CreatedAtAction(nameof(GetInvoice), new { id }, new { id });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteInvoice(int id, CancellationToken cancellationToken = default)
    {
        await commandDispatcher.DispatchAsync(
            new DeleteInvoiceCommand(id, GetUserId()),
            cancellationToken);
        return NoContent();
    }
}
