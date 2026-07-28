var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache")
    .WithHostPort(6379)
    .WithContainerName("Redis-Cache")
    .WithImageTag("8.4.1")
    .WithRedisInsight(configureContainer: container => container
        .WithContainerName("Redis-Insight")
        .WithHostPort(8001)
        .WithImageTag("3.0")
        .WithEnvironment("REDIS_HOSTS", "local:Redis-Cache:6379"));

var mailpit = builder.AddMailPit("mailpit",8025,1025)
    .WaitFor(cache)
    .WithDataBindMount("./mailpit-data", isReadOnly: false)
    .WithImageTag("v1.28.2");

// var seq = builder.AddSeq("seq", port: 5341)
//     .WithDataBindMount(source: "./seq-data", isReadOnly: false)
//     .ExcludeFromManifest()
//     .WithLifetime(ContainerLifetime.Persistent)
//     .WithEnvironment("ACCEPT_EULA", "Y");


// Postgres-Datenbank Server mit pgAdmin
var postgresUsername = builder.AddParameter("postgres-username", secret: false);
var postgresPassword = builder.AddParameter("postgres-password", secret: true);

var postgres = builder.AddPostgres("postgres", postgresUsername, postgresPassword)
    .WithImage("postgres", "18.2-bookworm")
    // In Postgres 18+: ein einzelner Mount auf "/var/lib/postgresql" statt "/var/lib/postgresql/data"
    // Damit legt Postgres die Daten versionsspezifisch (z.B. /var/lib/postgresql/18/main) an
    .WithBindMount(source: "./postgresql", target: "/var/lib/postgresql", isReadOnly: false)
    .WithHostPort(5432)
    .WithContainerName("Postgres-Server")
    .WithPgAdmin(pgadmin => pgadmin
    .WithImage("dpage/pgadmin4", "9.11.0")
    .WithHostPort(5050)
    .WithContainerName("Postgres-Admin")
);

// Datenbankresource erzeugen, Rückgabewert nicht benötigt
var postgresdb = postgres.AddDatabase("postgresdb");
var keycloakDb = postgres.AddDatabase("keycloak-db");

// Keycloak hinzufügen (Identity Provider) mit Admin-Credentials
var keycloakAdminUsername = builder.AddParameter(
    "keycloak-admin-username", secret: true);

var keycloakAdminPassword = builder.AddParameter(
    "keycloak-admin-password", secret: true);

var keycloakClientId = builder.AddParameter(
    "keycloak-client-id", secret: true);

var keycloakClientSecret = builder.AddParameter(
    "keycloak-client-secret", secret: true);


var keycloakBaseUrl = builder.AddParameter("keycloak-baseURL", secret: false);
var keycloakAuthority = builder.AddParameter("keycloak-authority", secret: false);
var keycloakRelam = builder.AddParameter("keycloak-realm", secret: false);
var keycloakMetadataAddress = builder.AddParameter("keycloak-metadataAddress", secret: true);


var keycloak = builder
    // Host-Port 8443, passend zu keycloak-baseURL/authority in appsettings.Development.json.
    // Aspire stellt Keycloak per gemountetem Zertifikat auf HTTPS um, der Endpoint spricht
    // also https://localhost:8443 - auch wenn Aspire ihn intern "http" nennt.
    .AddKeycloak("keycloak", port: 8443, adminUsername: keycloakAdminUsername, adminPassword: keycloakAdminPassword)
    .WithReference(keycloakDb)
    .WaitFor(keycloakDb)
    .WithImage("keycloak/keycloak", "26.5.3-0")
    .WithContainerName("Keycloak-Server")
    // Postgres explizit als Datenbanktyp setzen, damit Einstellungen dauerhaft gespeichert werden
    .WithEnvironment("KC_DB", "postgres")
    .WithEnvironment("KC_DB_URL_HOST", "Postgres-Server")
    .WithEnvironment("KC_DB_URL_PORT", "5432")
    .WithEnvironment("KC_DB_URL_DATABASE", "keycloak-db")
    .WithEnvironment("KC_DB_USERNAME", postgresUsername)
    .WithEnvironment("KC_DB_PASSWORD", postgresPassword)
    .WithRealmImport("./keycloak")
    .WithBindMount(source: "./keycloak", target: "/opt/keycloak/data/import", isReadOnly: false)
    .WithEnvironment("KC_HOSTNAME_STRICT", "false")
    .WithEnvironment("KC_HOSTNAME_STRICT_HTTPS", "false")
    .WithEnvironment("KC_HTTP_ENABLED", "true")
    .WithEndpoint("management", endpoint => endpoint.Port = 9000)
    .WithOtlpExporter()
    .WithLifetime(ContainerLifetime.Persistent);


var apiService = builder.AddProject<Projects.Handwerker_ApiService>("apiservice")
    .WithEnvironment("keycloak-authority", keycloakAuthority)
    .WithEnvironment("keycloak-baseURL", keycloakBaseUrl)
    .WithEnvironment("keycloak-realm", keycloakRelam)
    .WithEnvironment("keycloak-client-id", keycloakClientId)
    .WithEnvironment("keycloak-client-secret", keycloakClientSecret)
    .WithEnvironment("keycloak-metadataAddress", keycloakMetadataAddress)
    .WithHttpHealthCheck("/health")
    //KeyCloak Referenz für die API hinzufügen
    .WithReference(keycloak)
    .WaitFor(keycloak)
    // Redis-Cache Referenz für die API hinzufügen
    .WithReference(cache)
    .WaitFor(cache)
    // Mailpit-Referenz für E-Mail-Versand
    .WithReference(mailpit)
    .WaitFor(mailpit)
    // PostgreSQL Referenz für die API hinzufügen
    .WithReference(postgresdb)
    .WaitFor(postgresdb);

// builder.AddProject<Projects.Handwerker_Web>("webfrontend")
//     .WithExternalHttpEndpoints()
//     .WithReference(cache)
//     .WaitFor(cache)
//     .WithReference(apiService)
//     .WaitFor(apiService;

// Angular 21 client - läuft mit HTTPS via Aspire (eigene mkcert-Zertifikate in angular.json)
var angularClient = builder.AddJavaScriptApp("Angular",
        "../Handwerker-Client",
        "start")
    .WithPnpm()
    // isProxied: false -> der Dev-Server lauscht selbst auf 4200, statt dass Aspire einen
    // Proxy auf 4200 stellt und dem Server per PORT einen zufälligen Port zuweist.
    // Notwendig für OAuth: angular.json hat "open": true, der Browser landet also auf dem
    // Port des Dev-Servers. Bei zufälligem Port passt die Origin nicht zu den
    // redirectUris des angular-client im Realm -> invalid_redirect_uri.
    .WithHttpsEndpoint(port: 4200, targetPort: 4200, env: "PORT", isProxied: false)
    .WithExternalHttpEndpoints()
    .WithReference(keycloak)
    .WaitFor(keycloak)
    .WithReference(apiService)
    .WaitFor(apiService)
    .WithReference(postgres)
    .WaitFor(postgres)
    .WithReference(cache)
    .WaitFor(cache);

// Hinweis: MAUI App (Handwerker.Maui) wird manuell über Visual Studio/Rider gestartet
// Sie greift auf die API über die Endpunkte zu, die im Aspire Dashboard angezeigt werden
// Für Android Emulator: verwende http://10.0.2.2:{PORT}
// Für iOS Simulator: verwende http://localhost:{PORT}
// Für physische Geräte: verwende die IP-Adresse des Entwicklungsrechners

// Create a public dev tunnel for iOS and Android
// var publicDevTunnel = builder.AddDevTunnel("devtunnel-public")
//     .WithAnonymousAccess()
//     .WithReference(apiService.GetEndpoint("http"));
//
// var mauiapp = builder.AddMauiProject("HandwerkerMaui",
//         "../Handwerker.Maui/Handwerker.Maui/Handwerker.Maui.csproj");
//
// // Add Windows device (uses localhost directly)
// mauiapp.AddWindowsDevice()
//     .WithReference(apiService);
//
// // // Add Mac Catalyst device (uses localhost directly)
// // mauiapp.AddMacCatalystDevice()
// //     .WithReference(apiService);
// //
// // // Add iOS simulator with Dev Tunnel
// // mauiapp.AddiOSSimulator()
// //     .WithOtlpDevTunnel() // Required for OpenTelemetry data collection
// //     .WithReference(apiService, publicDevTunnel);
//
// // Add Android emulator with Dev Tunnel
// mauiapp.AddAndroidEmulator()
//     .WithOtlpDevTunnel() // Required for OpenTelemetry data collection
//     .WithReference(apiService, publicDevTunnel);

builder.Build().Run();