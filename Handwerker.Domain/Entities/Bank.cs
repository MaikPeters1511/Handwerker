using System.ComponentModel.DataAnnotations;

namespace Handwerker.Domain.Entities;

public class Bank
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(30)]
    public string Iban { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(10)]
    public string Plz { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Ort { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Bic { get; set; } = string.Empty;
}

