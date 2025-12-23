# Deploying the Independent Diagnostic Harness

## Overview

The OrkinosaiCMS.Diagnostics service is designed to run independently of the main application, providing critical diagnostic information even when the main app fails to start (HTTP 500.30 errors).

## Deployment Strategies

### Strategy 1: Separate Azure App Service (Recommended for Production)

This is the most robust approach - deploy diagnostics as a completely separate App Service.

#### Benefits:
- ✅ Guaranteed availability even if main app completely fails
- ✅ Independent scaling and resources
- ✅ Separate URL for security
- ✅ No port conflicts

#### Steps:

1. **Create a new Azure App Service**
   ```bash
   az webapp create \
     --resource-group orkinosai_group \
     --plan your-app-service-plan \
     --name mosaic-diagnostics \
     --runtime "DOTNET|9.0"
   ```

2. **Configure App Settings**
   ```bash
   az webapp config appsettings set \
     --resource-group orkinosai_group \
     --name mosaic-diagnostics \
     --settings \
       ASPNETCORE_ENVIRONMENT=Production \
       MainAppPath=/home/site/wwwroot/main-app \
       DiagnosticsSettings__EnableSecureAccess=true \
       DiagnosticsSettings__AccessToken=your-secure-token
   ```

3. **Deploy the Diagnostics Service**
   ```bash
   cd src/OrkinosaiCMS.Diagnostics
   dotnet publish -c Release -o ./publish
   az webapp deployment source config-zip \
     --resource-group orkinosai_group \
     --name mosaic-diagnostics \
     --src ./publish.zip
   ```

4. **Access Diagnostics**
   - URL: `https://mosaic-diagnostics.azurewebsites.net/diagnostics`
   - Health: `https://mosaic-diagnostics.azurewebsites.net/api/diagnostics/health`

### Strategy 2: Azure App Service Deployment Slot

Deploy diagnostics to a deployment slot of the main App Service.

#### Benefits:
- ✅ Same resource group and billing
- ✅ Shared App Service Plan
- ✅ Easy to manage

#### Steps:

1. **Create Deployment Slot**
   ```bash
   az webapp deployment slot create \
     --resource-group orkinosai_group \
     --name mosaic-saas \
     --slot diagnostics
   ```

2. **Configure Slot**
   ```bash
   az webapp config appsettings set \
     --resource-group orkinosai_group \
     --name mosaic-saas \
     --slot diagnostics \
     --settings \
       MainAppPath=/home/site/wwwroot
   ```

3. **Deploy to Slot**
   ```bash
   az webapp deployment source config-zip \
     --resource-group orkinosai_group \
     --name mosaic-saas \
     --slot diagnostics \
     --src ./publish.zip
   ```

4. **Access Diagnostics**
   - URL: `https://mosaic-saas-diagnostics.azurewebsites.net/diagnostics`

### Strategy 3: Same App Service, Different Port (Not Recommended)

Run diagnostics on same server but different port using reverse proxy.

⚠️ **Warning**: If the server crashes or resource exhaustion occurs, this approach won't work.

#### Configuration (web.config):
```xml
<configuration>
  <system.webServer>
    <rewrite>
      <rules>
        <rule name="Diagnostics Route" stopProcessing="true">
          <match url="^diagnostics(.*)" />
          <action type="Rewrite" url="http://localhost:5001/{R:1}" />
        </rule>
      </rules>
    </rewrite>
  </system.webServer>
</configuration>
```

### Strategy 4: Docker Container (Advanced)

Deploy diagnostics as a separate container in the same container group.

#### Docker Compose Example:
```yaml
version: '3.8'
services:
  main-app:
    image: mosaic-cms:latest
    ports:
      - "80:8080"
    
  diagnostics:
    image: mosaic-diagnostics:latest
    ports:
      - "5001:5001"
    volumes:
      - ./logs:/app/logs:ro
      - ./config:/app/config:ro
    environment:
      - MainAppPath=/app
```

## Configuration

### Environment Variables

Set these in Azure App Service Configuration:

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| `ASPNETCORE_ENVIRONMENT` | Environment name | Production | No |
| `Urls` | Listening URL/port | http://*:5001 | No |
| `MainAppPath` | Path to main app directory | /home/site/wwwroot | Yes |
| `DiagnosticsSettings__EnableSecureAccess` | Enable token auth | false | No |
| `DiagnosticsSettings__AccessToken` | Access token | (empty) | If secure access enabled |

### Access Token Setup

For production, always enable secure access:

1. Generate a secure token:
   ```bash
   openssl rand -base64 32
   ```

2. Set in Azure:
   ```bash
   az webapp config appsettings set \
     --resource-group orkinosai_group \
     --name mosaic-diagnostics \
     --settings \
       DiagnosticsSettings__EnableSecureAccess=true \
       DiagnosticsSettings__AccessToken=your-generated-token
   ```

3. Access with token:
   ```bash
   curl -H "Authorization: Bearer your-generated-token" \
     https://mosaic-diagnostics.azurewebsites.net/api/diagnostics/report
   ```

## CI/CD Integration

### Azure DevOps Pipeline

```yaml
# azure-pipelines-diagnostics.yml
trigger:
  branches:
    include:
      - main
  paths:
    include:
      - src/OrkinosaiCMS.Diagnostics/**

pool:
  vmImage: 'ubuntu-latest'

variables:
  buildConfiguration: 'Release'

steps:
- task: DotNetCoreCLI@2
  displayName: 'Restore Diagnostics'
  inputs:
    command: 'restore'
    projects: 'src/OrkinosaiCMS.Diagnostics/OrkinosaiCMS.Diagnostics.csproj'

- task: DotNetCoreCLI@2
  displayName: 'Build Diagnostics'
  inputs:
    command: 'build'
    projects: 'src/OrkinosaiCMS.Diagnostics/OrkinosaiCMS.Diagnostics.csproj'
    arguments: '--configuration $(buildConfiguration)'

- task: DotNetCoreCLI@2
  displayName: 'Publish Diagnostics'
  inputs:
    command: 'publish'
    projects: 'src/OrkinosaiCMS.Diagnostics/OrkinosaiCMS.Diagnostics.csproj'
    arguments: '--configuration $(buildConfiguration) --output $(Build.ArtifactStagingDirectory)/diagnostics'
    publishWebProjects: false

- task: ArchiveFiles@2
  displayName: 'Archive Diagnostics'
  inputs:
    rootFolderOrFile: '$(Build.ArtifactStagingDirectory)/diagnostics'
    includeRootFolder: false
    archiveType: 'zip'
    archiveFile: '$(Build.ArtifactStagingDirectory)/diagnostics.zip'

- task: AzureWebApp@1
  displayName: 'Deploy Diagnostics to Azure'
  inputs:
    azureSubscription: 'Azure-Subscription'
    appType: 'webAppLinux'
    appName: 'mosaic-diagnostics'
    package: '$(Build.ArtifactStagingDirectory)/diagnostics.zip'
```

### GitHub Actions

```yaml
# .github/workflows/deploy-diagnostics.yml
name: Deploy Diagnostics Service

on:
  push:
    branches: [ main ]
    paths:
      - 'src/OrkinosaiCMS.Diagnostics/**'
  workflow_dispatch:

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'
    
    - name: Restore dependencies
      run: dotnet restore src/OrkinosaiCMS.Diagnostics/OrkinosaiCMS.Diagnostics.csproj
    
    - name: Build
      run: dotnet build src/OrkinosaiCMS.Diagnostics/OrkinosaiCMS.Diagnostics.csproj --configuration Release --no-restore
    
    - name: Publish
      run: dotnet publish src/OrkinosaiCMS.Diagnostics/OrkinosaiCMS.Diagnostics.csproj -c Release -o ./diagnostics
    
    - name: Deploy to Azure Web App
      uses: azure/webapps-deploy@v2
      with:
        app-name: 'mosaic-diagnostics'
        publish-profile: ${{ secrets.AZURE_DIAGNOSTICS_PUBLISH_PROFILE }}
        package: './diagnostics'
```

## Verification

After deployment, verify the diagnostics service:

### 1. Health Check
```bash
curl https://mosaic-diagnostics.azurewebsites.net/api/diagnostics/health
```

Expected response:
```json
{
  "status": "Healthy",
  "service": "OrkinosaiCMS.Diagnostics",
  "timestamp": "2025-12-23T17:00:00Z",
  "message": "Diagnostic service is running"
}
```

### 2. Diagnostic Report
```bash
curl https://mosaic-diagnostics.azurewebsites.net/api/diagnostics/report
```

### 3. Web Dashboard
Open in browser: `https://mosaic-diagnostics.azurewebsites.net/diagnostics`

## Monitoring

### Azure Application Insights

Add Application Insights to diagnostics service:

```bash
az monitor app-insights component create \
  --resource-group orkinosai_group \
  --app mosaic-diagnostics-insights \
  --location eastus \
  --application-type web

# Get instrumentation key
az monitor app-insights component show \
  --resource-group orkinosai_group \
  --app mosaic-diagnostics-insights \
  --query instrumentationKey
```

Add to appsettings:
```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "your-key-here"
  }
}
```

### Health Check Alerts

Create alert rule for diagnostics service health:

```bash
az monitor metrics alert create \
  --resource-group orkinosai_group \
  --name diagnostics-service-down \
  --description "Alert when diagnostics service is unavailable" \
  --scopes /subscriptions/{subscription-id}/resourceGroups/orkinosai_group/providers/Microsoft.Web/sites/mosaic-diagnostics \
  --condition "avg availability < 90" \
  --evaluation-frequency 1m \
  --window-size 5m
```

## Troubleshooting

### Diagnostics Service Won't Start

1. Check Azure App Service logs:
   ```bash
   az webapp log tail --resource-group orkinosai_group --name mosaic-diagnostics
   ```

2. Verify configuration:
   ```bash
   az webapp config appsettings list --resource-group orkinosai_group --name mosaic-diagnostics
   ```

3. Check Application Insights for errors

### Can't Access Main App Logs

1. Verify `MainAppPath` is correct
2. Check file system permissions in Azure
3. Ensure main app is using Serilog file sink
4. Verify App Service file system is persistent (not using ephemeral storage)

### Authentication Issues

1. Verify access token is set correctly
2. Check Authorization header format: `Bearer <token>`
3. Review diagnostics service logs for auth errors

## Security Best Practices

1. **Always use separate App Service** for production
2. **Enable secure access** with strong token
3. **Restrict access** using Azure networking (VNet, firewall rules)
4. **Rotate tokens** regularly (every 90 days)
5. **Monitor access logs** for unauthorized attempts
6. **Use HTTPS only** - disable HTTP
7. **Limit IP access** using App Service IP restrictions

## Cost Optimization

- Use **B1 tier** (Basic) for diagnostics service (~$13/month)
- Share App Service Plan with other services
- Use consumption-based pricing for Container Apps
- Consider cold start acceptable - this is a troubleshooting tool

---

**Last Updated**: December 23, 2025
**Version**: 1.0.0
