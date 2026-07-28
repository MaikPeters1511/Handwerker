using System.ComponentModel.DataAnnotations;

namespace Handwerker.Domain.Entities;

/// <summary>
/// Auftrag (Order) - Zentrale Entity für Auftragsbearbeitung
/// </summary>
public record Order
{
    public int Id { get; set; }

    [MaxLength(50)]
    public string OrderNumber { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime OrderDate { get; set; }

    [MaxLength(30)]
    public string CustomerNumber { get; set; } = string.Empty;

    // Kunde (Snapshot)
    public Recipient Recipient { get; set; } = new();

    // Handwerker/Anbieter (Snapshot)
    public Provider Provider { get; set; } = new();

    // Status und Priorität
    public OrderStatus Status { get; set; } = OrderStatus.Draft;

    public Priority Priority { get; set; } = Priority.Normal;

    // Finanzen
    [DataType(DataType.Currency)]
    public decimal TotalNet { get; set; }

    [DataType(DataType.Currency)]
    public decimal TotalTaxAmount { get; set; }

    [DataType(DataType.Currency)]
    public decimal TotalGross { get; set; }

    // Zeiten
    [DataType(DataType.Date)]
    public DateTime? PlannedStartDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? PlannedEndDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? ActualStartDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? ActualEndDate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal EstimatedHours { get; set; }

    // Berechnet aus WorkTimeEntries
    public decimal ActualHours => WorkTimeEntries?.Sum(w => (decimal)w.TotalHours.TotalHours) ?? 0;

    // Beschreibung
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string InternalNotes { get; set; } = string.Empty;

    // Verknüpfungen
    /// <summary>
    /// Verknüpfte Angebote (mehrere Angebote können zu einem Auftrag führen)
    /// </summary>
    public List<OrderOffer> SourceOffers { get; set; } = new();

    /// <summary>
    /// Positionen/Produkte im Auftrag
    /// </summary>
    public List<Product> Products { get; set; } = new();

    /// <summary>
    /// Material-Entnahmen
    /// </summary>
    public List<OrderMaterial> Materials { get; set; } = new();

    /// <summary>
    /// Arbeitszeiten
    /// </summary>
    public List<WorkTimeEntry> WorkTimeEntries { get; set; } = new();

    // Rechnungsverknüpfung
    public int? InvoiceId { get; set; }

    public bool IsPaid { get; set; }

    public DateTime? PaymentReceivedAt { get; set; }

    // Metadaten
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;
}
