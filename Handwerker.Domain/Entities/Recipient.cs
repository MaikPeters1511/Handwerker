using System.ComponentModel.DataAnnotations;

namespace Handwerker.Domain.Entities;

public class Recipient
{
    public int Id { get; set; }
    
    // Identifikation
    [MaxLength(150)]
    public string CustomerNumber { get; set; } = string.Empty; // Kundennummer zur Referenz

    // Persönliche Daten
    [MaxLength(30)]
    public string Salutation { get; set; } = string.Empty; // Anrede (z.B. "Herr", "Frau", "Firma")
    [MaxLength(250)]
    public string Name { get; set; } = string.Empty; // Vollständiger Name oder Firmenname
    [MaxLength(250)]
    public string ContactPerson { get; set; } = string.Empty; // Ansprechpartner (z.B. "z.Hd. Herrn Müller")

    // Erweiterte Adresse
    [MaxLength(250)]
    public string Street { get; set; } = string.Empty;
    [MaxLength(250)]
    public string AddressLine2 { get; set; } = string.Empty; // Adresszusatz (z.B. "Hinterhaus", "c/o")
    [MaxLength(10)]
    public string ZipCode { get; set; } = string.Empty;
    [MaxLength(250)]
    public string City { get; set; } = string.Empty;
    [MaxLength(250)]
    public string Country { get; set; } = "Deutschland"; // Land (Wichtig für steuerl. Regeln wie EU-Ausland)

    // Kontakt für Rechnungsversand
    [EmailAddress]
    public string Email { get; set; } = string.Empty; // Für digitalen Rechnungsversand
    [Phone]
    public string Phone { get; set; } = string.Empty; // Für Rückfragen
}