namespace Handwerker.ApiService.Models;

public class InstallationUserDataDto
{
    public string Salutation { get; set; } = string.Empty; // Anrede
    public string Title { get; set; } = string.Empty; // Titel
    public string FirstName { get; set; } = string.Empty; // Vorname
    public string LastName { get; set; } = string.Empty; // Nachname
    public IFormFile? ProfileImage { get; set; } // Profilbild upload
}
