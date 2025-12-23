# HTTP Error 500.30 Fix - Quick Reference

This guide provides quick commands for preventing and fixing HTTP Error 500.30 using the enhanced startup scripts.

## For Local Development

### Before Deployment - Test Locally

```bash
# 1. Build and publish
dotnet publish src/OrkinosaiCMS.Web/OrkinosaiCMS.Web.csproj \
  --configuration Release \
  --output ./publish

# 2. Run pre-startup diagnostics
./scripts/pre-startup-check.sh ./publish

# 3. Start the application
cd ./publish
dotnet OrkinosaiCMS.Web.dll

# 4. In another terminal, verify startup
./scripts/verify-startup.sh http://localhost:5000
```

### Use Complete Startup Script

```bash
# Build and publish first
dotnet publish src/OrkinosaiCMS.Web/OrkinosaiCMS.Web.csproj \
  --configuration Release \
  --output ./publish

# Copy scripts to publish directory
mkdir -p ./publish/scripts
cp scripts/*.sh ./publish/scripts/
chmod +x ./publish/scripts/*.sh

# Start with full diagnostics
./scripts/start-application.sh ./publish
```

## For Azure App Service Deployment

### Manual Deployment with Verification

```bash
# 1. Build and publish
dotnet publish src/OrkinosaiCMS.Web/OrkinosaiCMS.Web.csproj \
  --configuration Release \
  --output ./publish

# 2. Verify artifacts before deployment
./scripts/pre-startup-check.sh ./publish

# 3. Deploy to Azure (using Azure CLI)
az webapp deployment source config-zip \
  --resource-group <resource-group> \
  --name mosaic-saas \
  --src ./publish.zip

# 4. Verify deployment
./scripts/verify-startup.sh https://mosaic-saas.azurewebsites.net
```

### Automated Deployment (GitHub Actions)

The deployment workflow `.github/workflows/deploy.yml` automatically:
1. ✅ Runs pre-startup diagnostics on published artifacts
2. ✅ Copies scripts to publish directory
3. ✅ Deploys to Azure App Service
4. ✅ Runs health checks after deployment

Simply push to main:
```bash
git push origin main
```

## Troubleshooting Commands

### Check Configuration Before Startup

```bash
# Quick validation
./scripts/pre-startup-check.sh ./publish

# What it checks:
# - web.config exists with stdout logging enabled
# - logs directory exists
# - Application DLL exists
# - Required directories (wwwroot, App_Data)
# - Azure-specific: Connection strings, .NET runtime, permissions
```

### Verify Application Health

```bash
# Default settings (localhost:5000, 10 retries, 5s delay)
./scripts/verify-startup.sh

# Custom settings for Azure
./scripts/verify-startup.sh https://mosaic-saas.azurewebsites.net 15 10

# Returns:
# - Exit code 0: Application is healthy
# - Exit code 1: Application failed to start or health check failed
```

### View Startup Logs

**Azure App Service:**
```bash
# Real-time logs
az webapp log tail --name mosaic-saas --resource-group <resource-group>

# Or use Azure Portal
# Portal → App Service → Log Stream
```

**Local Development:**
```bash
# Application logs
tail -f App_Data/Logs/*.log

# stdout logs (if running through IIS Express or web.config)
tail -f logs/stdout*.log
```

## Common Issues and Quick Fixes

### Issue: "web.config not found"

**Fix:**
```bash
# Ensure web.config is in source
ls -la src/OrkinosaiCMS.Web/web.config

# If missing, check it exists and is tracked by git
git ls-files | grep web.config
```

### Issue: "logs directory does not exist"

**Fix:**
```bash
# Create logs directory with placeholder
mkdir -p src/OrkinosaiCMS.Web/logs
echo "# Logs directory" > src/OrkinosaiCMS.Web/logs/.gitkeep
git add -f src/OrkinosaiCMS.Web/logs/.gitkeep
```

### Issue: "OrkinosaiCMS.Web.dll not found"

**Fix:**
```bash
# Rebuild and publish
dotnet clean
dotnet publish src/OrkinosaiCMS.Web/OrkinosaiCMS.Web.csproj \
  --configuration Release \
  --output ./publish
```

### Issue: "Connection string not found" in Azure

**Fix:**
```bash
# Set connection string in Azure Portal
# Portal → App Service → Configuration → Connection strings
# Name: DefaultConnection
# Type: SQLServer
# Value: <your-connection-string>

# Or use Azure CLI
az webapp config connection-string set \
  --resource-group <resource-group> \
  --name mosaic-saas \
  --settings DefaultConnection="<connection-string>" \
  --connection-string-type SQLServer
```

### Issue: Application starts but returns HTTP 503

**Troubleshooting:**
```bash
# 1. Check health endpoint
curl https://mosaic-saas.azurewebsites.net/api/health

# 2. Check readiness endpoint
curl https://mosaic-saas.azurewebsites.net/api/health/ready

# 3. View logs
az webapp log tail --name mosaic-saas --resource-group <resource-group>

# 4. Check database connectivity
# Verify connection string is correct
# Verify database exists and is accessible
# Verify migrations have been applied
```

## Script Reference

### pre-startup-check.sh
**Purpose:** Validate configuration before startup  
**Usage:** `./scripts/pre-startup-check.sh <directory>`  
**Exit Codes:** 0 = success, 1 = critical errors found  

### verify-startup.sh
**Purpose:** Verify application health with retries  
**Usage:** `./scripts/verify-startup.sh [url] [max-retries] [retry-delay]`  
**Exit Codes:** 0 = healthy, 1 = failed  

### start-application.sh
**Purpose:** Complete startup with diagnostics  
**Usage:** `./scripts/start-application.sh [directory]`  
**Exit Codes:** 0 = started successfully, 1 = startup failed  

## Best Practices

1. **Always run pre-startup diagnostics** before deploying
2. **Use verify-startup.sh** after deployment to confirm health
3. **Enable Azure logging** for production troubleshooting
4. **Monitor health endpoints** in your monitoring solution
5. **Keep scripts up to date** with application changes

## Related Documentation

- [TROUBLESHOOTING_HTTP_500_30.md](./TROUBLESHOOTING_HTTP_500_30.md) - Complete troubleshooting guide
- [HTTP_500_30_FIX_SUMMARY.md](./HTTP_500_30_FIX_SUMMARY.md) - Implementation summary
- [scripts/README.md](./scripts/README.md) - Complete scripts documentation
- [DEPLOYMENT_VERIFICATION_GUIDE.md](./DEPLOYMENT_VERIFICATION_GUIDE.md) - Post-deployment verification

## Support

If issues persist:
1. Check Azure Log Stream for detailed errors
2. Review stdout logs in Kudu Console
3. Run emergency diagnostics: `pwsh scripts/emergency-diagnostics.ps1`
4. See [TROUBLESHOOTING_HTTP_500_30.md](./TROUBLESHOOTING_HTTP_500_30.md) for detailed guidance
