using System.Collections.Generic;

namespace Handwerker.ApiService.Models;

public class InstallationSuppliersDto
{
    public List<int> SelectedSupplierIds { get; set; } = new List<int>(); // Multiselect IDs der Lieferanten
}
