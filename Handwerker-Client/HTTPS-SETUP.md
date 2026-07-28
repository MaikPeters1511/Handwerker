# HTTPS Setup für Angular mit .NET Aspire

## ✅ Aktuelle Lösung (mit mkcert)

Diese Anwendung nutzt **mkcert** generierte Zertifikate für lokales HTTPS.

### Einmalige Installation (pro Entwickler):

1. **CA-Zertifikat vertrauen:**
   ```bash
   cd Handwerker-Client
   certutil -addstore -user "Root" ca.crt
   ```

2. **Angular-Konfiguration:**
   Die `angular.json` ist bereits konfiguriert mit:
   - `sslCert: "localhost.pem"`
   - `sslKey: "localhost-key.pem"`

3. **Starten der Anwendung:**
   ```bash
   dotnet run --project Handwerker.AppHost
   ```

### Wie es funktioniert:
1. **Aspire** steuert den Angular Dev-Server über `AddNpmApp()`
2. **HTTPS wird konfiguriert** mit `.WithHttpsEndpoint()` und `ssl: true`
3. **mkcert Zertifikate** (localhost.pem) werden verwendet
4. Browser vertraut dem Zertifikat nach Installation von `ca.crt`

## Fehlerbehebung

### Falls "Nicht sichere Verbindung" (ERR_CERT_AUTHORITY_INVALID):

**CA-Zertifikat manuell installieren:**
```powershell
cd Handwerker-Client
certutil -addstore -user "Root" ca.crt
# Ausgabe sollte sein: "Root "Vertrauenswürdige Stammzertifizierungsstellen""
# Dann Browser neu starten!
```

### Zertifikat-Status prüfen:
```powershell
certutil -user -store "Root" | Select-String -Pattern "mkcert"
```

### Zertifikate neu erstellen (falls fehlend):

**Mit mkcert:**
```bash
# mkcert installieren (einmalig):
choco install mkcert
# oder: scoop install mkcert

# Zertifikate generieren:
cd Handwerker-Client
mkcert -install
mkcert -key-file localhost-key.pem -cert-file localhost.pem localhost 127.0.0.1 ::1

# CA-Zertifikat kopieren:
copy "$env:LOCALAPPDATA\mkcert\rootCA.pem" ca.crt
```

## Alternative: .NET Dev-Certs verwenden

Falls Sie .NET Entwicklungszertifikate bevorzugen:

1. Zertifikate generieren und exportieren:
   ```bash
   dotnet dev-certs https --trust
   dotnet dev-certs https --export-path Handwerker-Client/localhost.pfx -p YourPassword
   
   # PFX zu PEM konvertieren (mit OpenSSL):
   openssl pkcs12 -in localhost.pfx -out localhost.pem -clcerts -nokeys -passin pass:YourPassword
   openssl pkcs12 -in localhost.pfx -out localhost-key.pem -nocerts -nodes -passin pass:YourPassword
   ```

2. Browser neu starten

## Vorteile der mkcert-Lösung:
- ✅ **Funktioniert in allen Browsern** (nutzt System-Zertifikatsspeicher)
- ✅ **Einfache Team-Verteilung** (ca.crt in Git committen)
- ✅ **Keine Passwort-Konvertierung** nötig
- ✅ **Standardlösung** für lokale Entwicklung

## Technische Details:
- Angular Dev-Server läuft auf: `https://localhost:5000`
- Zertifikate liegen in: `Handwerker-Client/`
- Gültig für: `localhost`, `127.0.0.1`, `::1`
