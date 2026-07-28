using Handwerker.Domain.Entities;

namespace Handwerker.ApiService.Controllers;

// ── Response-DTOs ─────────────────────────────────────────────────────────────

public record OrderDto
{
    public int Id                     { get; init; }
    public string OrderNumber         { get; init; } = string.Empty;
    public DateTime OrderDate         { get; init; }
    public string CustomerNumber      { get; init; } = string.Empty;
    public string RecipientName       { get; init; } = string.Empty;
    public OrderStatus Status         { get; init; }
    public Priority Priority          { get; init; }
    public decimal TotalNet           { get; init; }
    public decimal TotalGross         { get; init; }
    public DateTime? PlannedStartDate { get; init; }
    public DateTime? PlannedEndDate   { get; init; }
    public DateTime? ActualStartDate  { get; init; }
    public DateTime? ActualEndDate    { get; init; }
    public decimal EstimatedHours     { get; init; }
    public decimal ActualHours        { get; init; }
    public int? InvoiceId             { get; init; }
    public bool IsPaid                { get; init; }
}

public record OrderDetailDto : OrderDto
{
    public Recipient Recipient                  { get; init; } = new();
    public Provider Provider                    { get; init; } = new();
    public decimal TotalTaxAmount               { get; init; }
    public string Description                   { get; init; } = string.Empty;
    public string InternalNotes                 { get; init; } = string.Empty;
    public List<Product> Products               { get; init; } = [];
    public List<OrderOfferDto> SourceOffers     { get; init; } = [];
    public List<OrderMaterial> Materials        { get; init; } = [];
    public List<WorkTimeEntry> WorkTimeEntries  { get; init; } = [];
    public DateTime CreatedAt                   { get; init; }
    public DateTime? UpdatedAt                  { get; init; }
}

public record OrderOfferDto
{
    public int OfferId               { get; init; }
    public string OfferNumber        { get; init; } = string.Empty;
    public decimal PortionPercentage { get; init; }
}

// ── Request-Typen ─────────────────────────────────────────────────────────────

public record CreateOrderRequest
{
    public DateTime OrderDate          { get; init; }
    public string CustomerNumber       { get; init; } = string.Empty;
    public Recipient Recipient         { get; init; } = new();
    public Provider Provider           { get; init; } = new();
    public List<Product> Products      { get; init; } = [];
    public Priority Priority           { get; init; } = Priority.Normal;
    public DateTime? PlannedStartDate  { get; init; }
    public DateTime? PlannedEndDate    { get; init; }
    public decimal EstimatedHours      { get; init; }
    public string Description          { get; init; } = string.Empty;
    public string InternalNotes        { get; init; } = string.Empty;
}

public record CreateOrderFromOffersRequest
{
    public List<int> OfferIds          { get; init; } = [];
    public DateTime OrderDate          { get; init; }
    public Priority Priority           { get; init; } = Priority.Normal;
    public DateTime? PlannedStartDate  { get; init; }
    public DateTime? PlannedEndDate    { get; init; }
    public decimal EstimatedHours      { get; init; }
    public string Description          { get; init; } = string.Empty;
    public string InternalNotes        { get; init; } = string.Empty;
}

public record UpdateOrderRequest
{
    public int Id                      { get; init; }
    public DateTime OrderDate          { get; init; }
    public string CustomerNumber       { get; init; } = string.Empty;
    public Recipient Recipient         { get; init; } = new();
    public Provider Provider           { get; init; } = new();
    public List<Product> Products      { get; init; } = [];
    public Priority Priority           { get; init; }
    public DateTime? PlannedStartDate  { get; init; }
    public DateTime? PlannedEndDate    { get; init; }
    public decimal EstimatedHours      { get; init; }
    public string Description          { get; init; } = string.Empty;
    public string InternalNotes        { get; init; } = string.Empty;
}

public record UpdateStatusRequest
{
    public OrderStatus Status { get; init; }
}

public record WorkTimeEntryRequest
{
    public DateTime Date           { get; init; }
    public TimeSpan StartTime      { get; init; }
    public TimeSpan EndTime        { get; init; }
    public TimeSpan BreakDuration  { get; init; }
    public string Description      { get; init; } = string.Empty;
    public bool IsBillable         { get; init; } = true;
    public decimal? HourlyRate     { get; init; }
}

public record AddMaterialRequest
{
    public int ArticleId            { get; init; }
    public int WarehouseId          { get; init; }
    public decimal PlannedQuantity  { get; init; }
}

public record ConfirmMaterialRequest
{
    public decimal ActualQuantity { get; init; }
}
