namespace Handwerker.ApiService.Models;

public class InstallationCompanyDataDto
{
    public string Name { get; set; } = string.Empty; // Firmenname
    public string Street { get; set; } = string.Empty; // Straße und Hausnummer
    public string ZipCode { get; set; } = string.Empty; // Postleitzahl
    public string City { get; set; } = string.Empty; // Ort
    public string Phone { get; set; } = string.Empty; // Telefon
    public string Email { get; set; } = string.Empty; // E-Mail
    public string CommercialRegister { get; set; } = string.Empty; // Handelsregisternummer
    public string RegisterCourt { get; set; } = string.Empty; // Amtsgericht
    public string TaxId { get; set; } = string.Empty; // Umsatzsteuer-ID
    public string TaxNumber { get; set; } = string.Empty; // Steuernummer
    public IFormFile? Logo { get; set; } // Firmalogo
    public bool VatExemption { get; set; } // Umsatzsteuerbefreiung
}
