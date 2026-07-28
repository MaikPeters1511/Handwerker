using AutoMapper;
using FluentValidation;
using Handwerker.ApiService;
using Handwerker.ApiService.Hubs;
using Handwerker.ApiService.Services;
using Handwerker.Application;
using Handwerker.Infrastructure.Data;
using Handwerker.Application.Services;
using Handwerker.Application.Services.Keycloak;
using Handwerker.Application.Services.Keycloak.Interfaces;
using Handwerker.Application.Services.Keycloak.Models;
using Handwerker.Application.Services.Keycloak.Validators;
using Handwerker.Domain.Interfaces;
using Handwerker.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<HandwerkerDbContext>("postgresdb");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddProblemDetails();

// Email Service für Mailpit
builder.Services.AddScoped<IEmailService, MailpitEmailService>();

builder.Services.AddHttpContextAccessor();

// Infrastructure Repositories
builder.Services.AddScoped<IKcUserService, KcUserService>();
builder.Services.AddScoped<IBankRepository, BankRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProviderRepository, ProviderRepository>();
builder.Services.AddScoped<IRecipientRepository, RecipientRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IOfferRepository, OfferRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<ISettingsRepository, SettingsRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IServiceItemRepository, ServiceItemRepository>();
builder.Services.AddScoped<IWageTypeRepository, WageTypeRepository>();
builder.Services.AddScoped<IKeycloakAdminApiFactory, KeycloakAdminApiFactory>();

// Register file storage service with WebRootPath
builder.Services.AddScoped<IFileStorageService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<Handwerker.Infrastructure.Services.LocalFileStorageService>>();
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    return new Handwerker.Infrastructure.Services.LocalFileStorageService(logger, env.WebRootPath);
});

// Application Services / Use Cases
builder.Services.AddApplicationHandlers(); // CQRS Handler + Dispatcher
builder.Services.AddScoped<BankService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<ProviderService>();
builder.Services.AddScoped<RecipientService>();
builder.Services.AddScoped<InvoiceService>();
builder.Services.AddScoped<OfferService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<SettingsService>();
builder.Services.AddScoped<CompanyService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IArticleService, ArticleService>();
builder.Services.AddScoped<IServiceItemService, ServiceItemService>();
builder.Services.AddScoped<IWageTypeService, WageTypeService>();

// Fluent Validation
builder.Services.AddScoped<FluentValidationFilter>();
builder.Services.AddScoped<IValidator<KcUserDto>, KcUserRequestValidator>();

var keycloakAuthority = builder.Configuration["keycloak-authority"]
    ?? throw new InvalidOperationException("keycloak-authority is not configured");
var keycloakBaseUrl = builder.Configuration["keycloak-baseURL"]
    ?? throw new InvalidOperationException("keycloak-baseURL is not configured");
var keycloakRealm = builder.Configuration["keycloak-realm"]
    ?? throw new InvalidOperationException("keycloak-realm is not configured");
var keycloakClientId = builder.Configuration["keycloak-client-id"]
    ?? throw new InvalidOperationException("keycloak-clientId is not configured");
var keycloakClientSecret = builder.Configuration["keycloak-client-secret"]
    ?? throw new InvalidOperationException("keycloak-clientSecret is not configured");
var keycloakMetadataAddress = builder.Configuration["keycloak-metadataAddress"]
    ?? throw new InvalidOperationException("keycloak-metadataAddress is not configured");

builder.Services.Configure<KeycloakSettings>(options =>
{
    options.Authority = keycloakAuthority;
    options.BaseURL = keycloakBaseUrl;
    options.Realm = keycloakRealm;
    options.ClientId = keycloakClientId;
    options.ClientSecret = keycloakClientSecret;
});

builder.Services.AddSingleton<IMapper>(sp =>
{
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

    var config = new MapperConfiguration(cfg =>
    {
        cfg.AddProfile<MappingProfile>();
    }, loggerFactory);

    config.AssertConfigurationIsValid();
    return config.CreateMapper();
});

builder.Services.AddAuthentication()
    .AddKeycloakJwtBearer(serviceName: "keycloak", realm: keycloakRealm,
        options =>
        {
            options.Authority = keycloakAuthority;
            options.MetadataAddress = keycloakMetadataAddress;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = true,
                ValidIssuer = keycloakAuthority
            };
        });

// Keycloak realm_access.roles → ASP.NET Core Role-Claims transformieren
// Notwendig, damit [Authorize(Roles = "admin")] korrekt funktioniert
builder.Services.AddScoped<IClaimsTransformation, KeycloakRolesClaimsTransformation>();
// CORS erlauben für lokale Frontend-Entwicklung
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalFrontend", policy =>
            policy.WithOrigins(
                    "https://localhost:5000",
                    "http://localhost:5000",
                    "https://localhost:4200",
                    "http://localhost:4200"
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
        // .AllowCredentials() // nur wenn wirklich Cookies/Authorization-Credentials gesendet werden
    );
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Redis Cache registrieren; ConnectionString wird über Aspire Service Discovery/Binding gesetzt
var redisConnection = builder.Configuration.GetConnectionString("cache")
                      ?? builder.Configuration["ConnectionStrings:cache"]
                      ?? builder.Configuration["cache:connectionString"]; // Fallbacks
if (!string.IsNullOrWhiteSpace(redisConnection))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
        options.InstanceName = "handwerker:";
    });
}
else
{
    // Fallback auf InMemory, falls Redis nicht gebunden ist (z.B. in Tests)
    builder.Services.AddDistributedMemoryCache();
}

builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(10),
        LocalCacheExpiration = TimeSpan.FromMinutes(10)
    };
});
builder.Services.AddMemoryCache();

// SignalR für Echtzeit-Benachrichtigungen
builder.Services.AddSignalR();

// Background Service für Low-Stock Überwachung
builder.Services.AddHostedService<LowStockMonitorService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HandwerkerDbContext>();
    await DbInitializer.SeedAsync(db);
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

// CORS früh in die Pipeline setzen
app.UseCors("AllowLocalFrontend");

// Static Files für Uploads (z.B. Company-Logos)
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("My API");
    });
    
}

// SignalR Hub
app.MapHub<NotificationHub>("/hubs/notifications");

app.MapControllers();
app.MapDefaultEndpoints();

app.Run();