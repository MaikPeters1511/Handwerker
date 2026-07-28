namespace Handwerker.ApiService.Models;

public class InstallationFinalDto
{
    public string Industry { get; set; } = string.Empty; // Branche
    public string ReferralSource { get; set; } = string.Empty; // Wie aufmerksam geworden
    public bool AvAgreementAccepted { get; set; } // AV-Vertrag Zustimmung
}
