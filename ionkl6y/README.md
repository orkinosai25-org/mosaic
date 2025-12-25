# Isolated Mosaic CMS Deployment (ionkl6y)

## Overview

This is an **isolated deployment environment** for the Mosaic CMS, created to test the CMS functionality in a clean, standalone setup without any legacy code or configuration issues from the main repository.

**Deployment ID:** `ionkl6y`  
**Purpose:** Fresh deployment testing of Mosaic CMS API services  
**Isolation Level:** Complete - separate from main codebase

## Directory Structure

```
ionkl6y/
├── src/
│   └── MosaicCMS/           # Mosaic CMS API project
│       ├── Controllers/      # API controllers
│       ├── Models/          # Data models
│       ├── Services/        # Business logic services
│       ├── Program.cs       # Application entry point
│       └── appsettings.json # Configuration
├── scripts/
│   └── deploy.sh            # Deployment automation script
├── docs/
│   └── DEPLOYMENT_REPORT.md # Deployment outcome documentation
├── Dockerfile               # Container build configuration
├── docker-compose.yml       # Service orchestration
├── MosaicCMS.sln           # Solution file
└── .env.example            # Environment configuration template
```

## What's Included

This isolated deployment includes:

- ✅ **MosaicCMS API** - Minimal ASP.NET Core Web API
- ✅ **Azure Blob Storage Integration** - Media storage services
- ✅ **Docker Configuration** - Containerized deployment
- ✅ **Health Checks** - Service monitoring
- ✅ **Deployment Scripts** - Automated deployment tools

## What's NOT Included

To maintain isolation and minimize complexity:

- ❌ No OrkinosaiCMS.Web (Blazor components)
- ❌ No OrkinosaiCMS.Infrastructure (Entity Framework)
- ❌ No Database dependencies
- ❌ No Frontend portal
- ❌ No authentication/authorization
- ❌ No existing migrations or schema
- ❌ No legacy configuration

## Prerequisites

- Docker 20.10 or higher
- Docker Compose 2.0 or higher
- (Optional) Azure Blob Storage account for media storage

## Quick Start

### 1. Configuration

Copy the environment template:

```bash
cp .env.example .env
```

Edit `.env` and configure your settings (optional):

```bash
# Azure Blob Storage (Optional)
AZURE_BLOB_CONNECTION_STRING=your_connection_string_here
AZURE_BLOB_CONTAINER=mosaic-cms
```

### 2. Deploy

Use the deployment script for easy management:

```bash
# Full deployment (build + start)
./scripts/deploy.sh deploy

# Or step by step:
./scripts/deploy.sh build   # Build Docker images
./scripts/deploy.sh start   # Start services
```

### 3. Verify

Once deployed, the CMS will be accessible at:

- **HTTP:** http://localhost:8080
- **Health Check:** http://localhost:8080/health
- **OpenAPI/Swagger:** http://localhost:8080/openapi/v1.json (if enabled)

### 4. Monitor

View real-time logs:

```bash
./scripts/deploy.sh logs
```

Check service status:

```bash
./scripts/deploy.sh status
```

## Deployment Script Commands

The `deploy.sh` script provides the following commands:

| Command | Description |
|---------|-------------|
| `build` | Build Docker images |
| `start` | Start services |
| `stop` | Stop services |
| `restart` | Restart services |
| `logs` | Show real-time logs |
| `status` | Show service status |
| `clean` | Remove all containers, images, and volumes |
| `deploy` | Full deployment (build + start) |

## Testing the Deployment

### 1. Health Check

```bash
curl http://localhost:8080/health
```

Expected response: `Healthy` (200 OK)

### 2. API Endpoints

Test API functionality:

```bash
# Example: List containers (if configured with Azure Blob)
curl http://localhost:8080/api/storage/containers
```

### 3. Container Logs

Check for errors or warnings:

```bash
docker logs mosaic-cms-isolated
```

### 4. Container Health

Verify the health status:

```bash
docker inspect mosaic-cms-isolated --format='{{.State.Health.Status}}'
```

## Architecture

### Single Service Design

This isolated deployment uses a minimal architecture:

```
┌─────────────────────────────────────┐
│     Mosaic CMS Container            │
│  ┌───────────────────────────────┐  │
│  │   ASP.NET Core Web API        │  │
│  │   - Controllers               │  │
│  │   - Services                  │  │
│  │   - Health Checks             │  │
│  └───────────────────────────────┘  │
│              │                       │
│              ▼                       │
│  ┌───────────────────────────────┐  │
│  │  Azure Blob Storage (Optional)│  │
│  │  - Media storage              │  │
│  └───────────────────────────────┘  │
└─────────────────────────────────────┘
```

### No Database Required

This deployment is **database-free** to avoid:
- Migration conflicts
- Schema issues
- Connection string problems
- Entity Framework complexity

### Network Isolation

Services run on an isolated Docker network (`mosaic-isolated-network`) to prevent interference with other deployments.

## Configuration

### Environment Variables

Configure via `.env` file or Docker environment:

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Production` |
| `ASPNETCORE_URLS` | Listening URLs | `http://+:8080` |
| `AZURE_BLOB_CONNECTION_STRING` | Azure Storage connection | (empty) |
| `AZURE_BLOB_CONTAINER` | Default container name | `mosaic-cms` |

### Ports

| Port | Purpose |
|------|---------|
| 8080 | HTTP endpoint |
| 8081 | HTTPS endpoint (if configured) |

## Troubleshooting

### Container Won't Start

Check logs:
```bash
docker logs mosaic-cms-isolated
```

Common issues:
- Port 8080 already in use
- Invalid environment configuration
- Build errors

### Health Check Failing

Verify the service is responding:
```bash
curl -v http://localhost:8080/health
```

### Build Errors

Clean and rebuild:
```bash
./scripts/deploy.sh clean
./scripts/deploy.sh build
```

### Permission Issues

Ensure Docker daemon is running:
```bash
sudo systemctl status docker
```

## Stopping the Deployment

### Graceful Shutdown

```bash
./scripts/deploy.sh stop
```

### Complete Cleanup

Remove all containers, images, and volumes:

```bash
./scripts/deploy.sh clean
```

## Deployment Outcomes

Results of the deployment test are documented in:
- [DEPLOYMENT_REPORT.md](docs/DEPLOYMENT_REPORT.md)

This report includes:
- Deployment steps executed
- Success/failure status
- Error logs (if any)
- Performance observations
- Recommendations

## Comparison with Main Repository

| Aspect | Main Repository | Isolated Deployment (ionkl6y) |
|--------|----------------|-------------------------------|
| **Complexity** | High (multiple projects) | Low (single API) |
| **Dependencies** | Many (.NET libraries, EF, Identity) | Minimal (ASP.NET Core only) |
| **Database** | Required (SQL Server) | Not required |
| **Frontend** | React + Blazor | None |
| **Configuration** | Complex (multi-tenant, auth) | Simple (API only) |
| **Purpose** | Full SaaS platform | API testing |

## Success Criteria

The isolated deployment is considered successful if:

- ✅ Docker container builds without errors
- ✅ Container starts and runs successfully
- ✅ Health check endpoint responds
- ✅ No runtime exceptions in logs
- ✅ API endpoints are accessible
- ✅ Container remains stable over time

## Isolation Benefits

This isolated deployment helps identify:

1. **Core CMS Functionality** - Does the base API work?
2. **Dependency Issues** - Are there hidden dependencies?
3. **Configuration Problems** - Is the configuration valid?
4. **Build Process** - Does the build pipeline work?
5. **Runtime Stability** - Does it run without crashes?

## Next Steps

After successful isolated deployment:

1. Document findings in DEPLOYMENT_REPORT.md
2. Compare behavior with main repository
3. Identify specific issues in main codebase
4. Apply fixes to main repository
5. Gradually reintroduce removed components

## Support

For issues specific to this isolated deployment, refer to:
- Deployment logs: `docker logs mosaic-cms-isolated`
- Build logs: `docker-compose logs`
- Main repository issues: https://github.com/orkinosai25-org/mosaic/issues

## License

This isolated deployment inherits the license from the main Mosaic repository.

---

**Deployment Environment ID:** `ionkl6y`  
**Created:** 2025-12-25  
**Purpose:** Isolated testing of Mosaic CMS core functionality  
**Status:** Ready for deployment
