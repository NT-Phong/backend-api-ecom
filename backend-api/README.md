# MebiOne – Local Setup Guide (PostgreSQL + pgAdmin)

This README covers the basic steps to run **MebiOne** locally with **PostgreSQL** (Docker) and manage it via **pgAdmin**:

1. Start infrastructure with `docker-compose.yml`.
2. Create a new database for the app (recommended: `mebione`).
3. Set `ConnectionStrings.Default` in `appsettings.json`.
4. Run the .NET app.

---

## 1. Start infrastructure with Docker

### 1.1. Prerequisites

- **Docker** and **Docker Compose** installed.
- Source code for **MebiOne** cloned locally:

```bash
git clone <YOUR_REPO_URL>
cd MebiOne
```

### 1.2. Run `docker-compose.yml`

From the folder that contains `docker-compose.yml` (usually the project root), run:

```bash
docker compose up -d
# or (depending on your Docker version)
docker-compose up -d
```

### 1.3. What gets exposed on your host

Based on your current `docker-compose.yml`:

**PostgreSQL**
- **Host/Port**: `localhost:5432`
- **Container**: `postgres-18`
- **User**: `postgres` (default)
- **Password**: value of `POSTGRES_PASSWORD` (currently `1`)
- **Maintenance DB**: `postgres`

**pgAdmin (Web UI)**
- **URL**: `http://localhost:5433`
- **Container**: `pgadmin`
- **Email**: `PGADMIN_DEFAULT_EMAIL` (currently `admin@abc.xyz`)
- **Password**: `PGADMIN_DEFAULT_PASSWORD` (currently `1`)

> Tip: If you change any passwords in compose, update them here and in your `appsettings.json`.

---

## 2. Connect to PostgreSQL using pgAdmin

1. Open pgAdmin in your browser:
   - `http://localhost:5433`

2. Login using:
   - **Email**: `admin@abc.xyz`
   - **Password**: `1`

3. Add your Postgres server:
   - In pgAdmin: **Add New Server**
   - **General** tab:
     - Name: `local-postgres` (any name)
   - **Connection** tab:
     - Host name/address: `postgres-18`
       - (Because pgAdmin runs inside Docker and should connect to the Postgres container by service/container name)
     - Port: `5432`
     - Maintenance database: `postgres`
     - Username: `postgres`
     - Password: `1`
     - ✅ Save password: checked (optional)

4. Click **Save** → you should see the server and databases.

---

## 3. Create database for MebiOne

### Recommended database name (Postgres-friendly): `mebione`

Postgres folds unquoted identifiers to **lowercase**, so the most frictionless choice is:

- Database: `mebione` (lowercase)

### Option A — Create DB using pgAdmin (recommended)

1. In pgAdmin, expand your server → **Databases**
2. Right‑click **Databases** → **Create** → **Database...**
3. Database name: `mebione`
4. Save

### Option B — Create DB using `psql` inside the Docker container (no GUI)

Create `mebione`:

```bash
docker exec -it postgres-18 psql -U postgres -d postgres -c "CREATE DATABASE mebione;"
```

---

### If you insist on the exact name `MebiOne` (with capital letters)

You must create it **with double quotes**:

```sql
CREATE DATABASE "MebiOne";
```

Then your connection string must use **exactly** `Database=MebiOne` (same casing).

(Again: this works, but lowercase names are usually smoother on Postgres.)

---

## 4. Configure the connection string in `appsettings.json`

Open `appsettings.json` and update the `ConnectionStrings` section.

### App runs on your host (typical dev)
Use `localhost:5432`:

```jsonc
{
  // ...
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=mebione;Username=postgres;Password=1;SSL Mode=Disable;"
  }
  // ...
}
```

### App runs as a container in the same compose network
Use the service/container name instead of localhost:

```text
Host=postgres-18;Port=5432;Database=mebione;Username=postgres;Password=1;SSL Mode=Disable;
```

---

## 5. Run the MebiOne application

After:

- Docker is running (`docker compose up -d`)
- Database exists (`mebione` or `"MebiOne"`)
- `ConnectionStrings.Default` is set

Run:

```bash
dotnet restore
dotnet build
dotnet run
```

(Run inside the API project folder, e.g. `src/MebiOne.Api`, depending on your solution structure.)

---

## 6. Quick summary

1. **Start Docker**
   - `docker compose up -d`

2. **Open pgAdmin**
   - `http://localhost:5433` → login `admin@abc.xyz / 1`
   - Add server → host `postgres-18`, port `5432`, user `postgres`, pass `1`

3. **Create DB**
   - Recommended: `mebione`

4. **Update config**
   - `ConnectionStrings.Default` → Postgres connection string

5. **Run**
   - `dotnet restore && dotnet build && dotnet run`

---

## 7. Deploy to Production (PRD)

This section describes a simple production deployment flow for the WebApi using Docker.

### 7.1. Build & push Docker image

From the solution root folder (where the `.sln` lives, with the Dockerfile located at `WebApi/Dockerfile`):

```bash
# Build image
docker build -t <YOUR_REGISTRY>/MebiOne-webapi:<TAG> -f WebApi/Dockerfile .

# Push image to registry (Docker Hub / ACR / others)
docker push <YOUR_REGISTRY>/MebiOne-webapi:<TAG>
```

Where:

- `<YOUR_REGISTRY>`: your container registry name (for example `mebisoft` or `myacr.azurecr.io`).
- `<TAG>`: version tag (for example `v1.0.0`, `v1.0.1`, `latest`, ...).

---

### 7.2. Run PRD on self-hosted server using `docker-compose.prod.yml`

On your production server (Linux VM / bare-metal / on‑prem):

1. Copy the required files:

- `docker-compose.prod.yml`
- `WebApi/Dockerfile` (optional, depending on your CI/CD flow)
- Any `.env` file or shell script that exports environment variables (if used)

2. Start the service:

```bash
docker compose -f docker-compose.prod.yml up -d
# or
docker-compose -f docker-compose.prod.yml up -d
```

To update to a new version (when using images pulled from a registry):

```bash
docker compose -f docker-compose.prod.yml pull
docker compose -f docker-compose.prod.yml up -d
```

---

### 7.3. Deploy to Azure App Service (Linux – Web App for Containers)

1. **Push image to Azure Container Registry (ACR)**

```bash
docker tag MebiOne-webapi:<TAG> <YOUR_ACR>.azurecr.io/MebiOne-webapi:<TAG>
docker push <YOUR_ACR>.azurecr.io/MebiOne-webapi:<TAG>
```

- `<YOUR_ACR>`: your ACR name (for example `mebisoftacr`).

2. **Create Azure Web App for Containers**

- OS: **Linux**
- Publish: **Docker Container**
- Image source: **Azure Container Registry**
- Select the image `MebiOne-webapi:<TAG>` that you pushed.

3. **Configure Application Settings** (Portal → Web App → _Configuration_ → _Application settings_)

Add these environment variables:

- `ASPNETCORE_ENVIRONMENT = Production`
- `WEBSITES_PORT = 8080` *(must match the port Kestrel listens on inside the container)*

4. **Restart the Web App**

After saving the configuration, restart the Web App so the new environment variables and image/tag are applied.

> **PRD tip**
>
> Prefer logging to **Console** (for example, Serilog `WriteTo.Console`) so the platform can collect logs automatically.
