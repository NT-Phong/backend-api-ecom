# Configuration inventory

## Files intentionally excluded from the initial Git baseline

- `Presentation/Ecom.API/appsettings.json`
- `Presentation/Ecom.API/appsettings.Development.json`
- `Presentation/Ecom.API/Properties/launchSettings.json`
- `docker-compose.yml`
- `docker-compose.dev.yml`
- `docker-compose.stag.yml`
- `docker-compose.prod.yml`
- `Dockerfile`
- `Dockerfile.migrator`

These files can contain connection strings, credentials, hostnames, ports, deployment identities, or environment-specific endpoints. Later phases will replace them with sanitized templates; this baseline never copies their values into inventory documents.

