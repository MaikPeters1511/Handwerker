using Handwerker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Handwerker.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(HandwerkerDbContext context)
    {
        await context.Database.MigrateAsync();

        if (!await context.Banks.AnyAsync())
        {
            await SeedBanksAsync(context);
        }

        if (!await context.Products.AnyAsync())
        {
            await SeedProductsAsync(context);
        }

        if (!await context.Recipients.AnyAsync())
        {
            await SeedRecipientsAsync(context);
        }

        if (!await context.Providers.AnyAsync())
        {
            await SeedProviderAsync(context);
        }

        if (!await context.AppSettings.AnyAsync())
        {
            await SeedAppSettingsAsync(context);
        }
    }

    private static async Task SeedRecipientsAsync(HandwerkerDbContext context)
    {
        var faker = new Bogus.Faker<Recipient>("de")
            .RuleFor(r => r.CustomerNumber, f => f.Random.Replace("KD-#####"))
            .RuleFor(r => r.Salutation, f => f.PickRandom("Herr", "Frau", "Firma"))
            .RuleFor(r => r.Name, (f, r) => r.Salutation == "Firma" ? f.Company.CompanyName() : f.Name.FullName())
            .RuleFor(r => r.ContactPerson, (f, r) => r.Salutation == "Firma" ? f.Name.FullName() : string.Empty)
            .RuleFor(r => r.Street, f => f.Address.StreetAddress())
            .RuleFor(r => r.AddressLine2, f => f.Random.Bool(0.2f) ? f.Address.SecondaryAddress() : string.Empty)
            .RuleFor(r => r.ZipCode, f => f.Address.ZipCode())
            .RuleFor(r => r.City, f => f.Address.City())
            .RuleFor(r => r.Country, f => "Deutschland")
            .RuleFor(r => r.Email, f => f.Internet.Email())
            .RuleFor(r => r.Phone, f => f.Phone.PhoneNumber());

        var recipients = faker.Generate(80);
        await context.Recipients.AddRangeAsync(recipients);
        await context.SaveChangesAsync();
    }

    private static async Task SeedProviderAsync(HandwerkerDbContext context)
    {
        // Get some banks for the providers
        var banks = await context.Banks.Take(100).ToListAsync();
        var random = new Random();

        var faker = new Bogus.Faker<Provider>("de")
            .RuleFor(p => p.Name, f => f.Name.FullName())
            .RuleFor(p => p.Company, f => f.Company.CompanyName())
            .RuleFor(p => p.Street, f => f.Address.StreetAddress())
            .RuleFor(p => p.ZipCode, f => f.Address.ZipCode())
            .RuleFor(p => p.City, f => f.Address.City())
            .RuleFor(p => p.Email, f => f.Internet.Email())
            .RuleFor(p => p.Phone, f => f.Phone.PhoneNumber())
            .RuleFor(p => p.Website, f => f.Internet.Url())
            .RuleFor(p => p.TaxId, f => "DE" + f.Random.Replace("#########"))
            .RuleFor(p => p.TaxNumber, f => f.Random.Replace("##/###/#####"))
            .RuleFor(p => p.CommercialRegister, f => "HRB " + f.Random.Number(1000, 99999))
            .RuleFor(p => p.RegisterCourt, f => "Amtsgericht " + f.Address.City());

        var providers = faker.Generate(80);
        
        if (banks.Count != 0)
        {
            foreach (var provider in providers)
            {
                provider.Bank = banks[random.Next(banks.Count)];
            }
        }

        await context.Providers.AddRangeAsync(providers);
        await context.SaveChangesAsync();
    }

    private static async Task SeedProductsAsync(HandwerkerDbContext context)
    {
        var faker = new Bogus.Faker<Product>("de")
            .RuleFor(p => p.ArticleNumber, f => f.Commerce.Ean13())
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Position, f => 0) // Default for catalog
            .RuleFor(p => p.Quantity, f => 1)
            .RuleFor(p => p.Unit, f => f.PickRandom("Stk", "m", "qm", "l", "kg"))
            .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
            .RuleFor(p => p.TaxRate, f => 19m)
            .RuleFor(p => p.UnitPrice, f => Math.Round(f.Random.Decimal(5, 500), 2))
            .RuleFor(p => p.DiscountPercent, f => 0)
            .RuleFor(p => p.DiscountAmount, f => 0)
            .RuleFor(p => p.TaxAmount, (f, p) => Math.Round(p.UnitPrice * 0.19m, 2))
            .RuleFor(p => p.TotalNet, (f, p) => p.UnitPrice)
            .RuleFor(p => p.TotalGross, (f, p) => p.TotalNet + p.TaxAmount);

        // Generate in batches to be safe with memory
        const int batchSize = 1000;
        const int totalCount = 10000;

        for (int i = 0; i < totalCount; i += batchSize)
        {
            var products = faker.Generate(batchSize);
            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedBanksAsync(HandwerkerDbContext context)
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "Seed", "blz-aktuell-csv-data.csv");
        
        // If running in development, the file might be in the project folder, not bin.
        // But usually we should copy it to output directory.
        // Let's check source path if not found in bin
        if (!File.Exists(filePath))
        {
            // Fallback for development time: assume standard project structure
            filePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../Seed/blz-aktuell-csv-data.csv"));
        }

        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Bank seed file not found: {filePath}");
            return;
        }

        // Register code pages provider for Windows-1252
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        var encoding = System.Text.Encoding.GetEncoding(1252);

        var banks = new List<Bank>();
        using var reader = new StreamReader(filePath, encoding);
        
        string? line;
        bool isHeader = true;
        
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            
            // Basic CSV parsing for semicolon separated
            // Note: This simple split works if fields don't contain the separator.
            // Bank names usually don't contain semicolons.
            
            var parts = line.Split(';');
            
            if (isHeader)
            {
                // Skip header
                isHeader = false;
                continue;
            }

            if (parts.Length < 8) continue; // Ensure enough columns

            // Removing potential quotes
            var blz = parts[0].Trim('"');
            var name = parts[2].Trim('"');
            var plz = parts[3].Trim('"');
            var city = parts[4].Trim('"');
            var bic = parts[7].Trim('"');

            if (!string.IsNullOrWhiteSpace(blz) && !string.IsNullOrWhiteSpace(name))
            {
                banks.Add(new Bank
                {
                    Iban = blz,
                    Name = name,
                    Plz = plz,
                    Ort = city,
                    Bic = bic
                });
            }
            
            // Insert in chunks to avoid memory issues if list is huge
            if (banks.Count >= 1000)
            {
                await context.Banks.AddRangeAsync(banks);
                await context.SaveChangesAsync();
                banks.Clear();
            }
        }

        if (banks.Count > 0)
        {
            await context.Banks.AddRangeAsync(banks);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedAppSettingsAsync(HandwerkerDbContext context)
    {
        var settings = new AppSettings();
        await context.AppSettings.AddAsync(settings);
        await context.SaveChangesAsync();
    }
}
