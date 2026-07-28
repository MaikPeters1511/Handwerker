namespace Handwerker.ApiService.Models;

public class CompanyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    
    // Bank details
    public string BankName { get; set; } = string.Empty;
    public string Iban { get; set; } = string.Empty;
    public string Bic { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Public URL to access the logo (if present)
    public string? LogoUrl { get; set; }

    public string TaxNumber { get; set; } = string.Empty;
    public bool VatExemption { get; set; }
    public string CommercialRegister { get; set; } = string.Empty;
    public string RegisterCourt { get; set; } = string.Empty;
}
