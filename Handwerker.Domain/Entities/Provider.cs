using System.ComponentModel.DataAnnotations;

namespace Handwerker.Domain.Entities;

public record Provider
{
    public int Id { get; set; }
    
    // Firmen-Infos
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty; // Inhaber / Ansprechpartner
    
    [MaxLength(255)]
    public string Company { get; set; } = string.Empty; // Firmenname
    
    // Adresse (Essenziell für Rechnungen)
    [MaxLength(255)]
    public string Street { get; set; } = string.Empty;
    [MaxLength(10)]
    public string ZipCode { get; set; } = string.Empty; 
    [MaxLength(255)]
    public string City { get; set; } = string.Empty;

    // Kontakt
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Phone]
    public string Phone { get; set; } = string.Empty;
    [Url]
    public string? Website { get; set; } // Optional

    // Steuern & Rechtliches
    [MaxLength(255)]
    public string TaxId { get; set; } = string.Empty; // USt-IdNr. (für EU)
    [MaxLength(255)]
    public string TaxNumber { get; set; } = string.Empty; // Steuernummer (für Finanzamt)
    [MaxLength(255)]
    public string CommercialRegister { get; set; } = string.Empty; // HRB/HRA (falls vorhanden)
    [MaxLength(255)]
    public string RegisterCourt { get; set; } = string.Empty; // Amtsgericht (falls vorhanden)
    [Required]
    public Bank Bank { get; set; } = new Bank();
}