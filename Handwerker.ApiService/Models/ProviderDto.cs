namespace Handwerker.ApiService.Models;

public class ProviderDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Website { get; set; }
    public string TaxId { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public string CommercialRegister { get; set; } = string.Empty;
    public string RegisterCourt { get; set; } = string.Empty;
    public BankDto Bank { get; set; } = new BankDto();
}

public class BankDto
{
    public string Name { get; set; } = string.Empty;
    public string Iban { get; set; } = string.Empty;
    public string Bic { get; set; } = string.Empty;
}
