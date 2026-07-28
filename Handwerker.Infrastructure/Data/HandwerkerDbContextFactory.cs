using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Handwerker.Infrastructure.Data;

/// <summary>
/// Factory für die Erstellung des DbContext zur Design-Zeit (z.B. für Migrationen).
/// </summary>
public class HandwerkerDbContextFactory : IDesignTimeDbContextFactory<HandwerkerDbContext>
{
    public HandwerkerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<HandwerkerDbContext>();

        // Verwende eine temporäre Connection-String für Design-Zeit
        // Die eigentliche Connection-String wird zur Laufzeit aus der Konfiguration geladen
        optionsBuilder.UseNpgsql("Host=localhost;Database=handwerker;Username=postgres;Password=postgres");

        return new HandwerkerDbContext(optionsBuilder.Options);
    }
}
