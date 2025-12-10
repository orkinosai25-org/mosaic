# Python Backend Deployment Guide

## Overview

The Zoota AI Assistant uses a Python Flask backend (`app.py`) that communicates with Azure OpenAI to provide intelligent chat responses. This guide covers deployment, troubleshooting, and maintenance of the Python backend.

## Architecture

```
┌─────────────────────────────────────────┐
│         Azure App Service               │
├─────────────────────────────────────────┤
│  startup.sh (Entry Point)               │
│      ↓                                   │
│  ┌──────────────────┐                   │
│  │ Python Backend   │                   │
│  │ (Port 8000)      │                   │
│  │ - Flask + WSGI   │                   │
│  │ - Gunicorn       │                   │
│  │ - Azure OpenAI   │                   │
│  └────────┬─────────┘                   │
│           │                              │
│  ┌────────▼─────────┐                   │
│  │ .NET Application │                   │
│  │ (Port 80/443)    │                   │
│  │ - Blazor Server  │                   │
│  │ - ChatAgent      │                   │
│  └──────────────────┘                   │
└─────────────────────────────────────────┘
```

## Files Structure

```
src/orkinosaiCMS.Server/
├── PythonBackend/
│   ├── app.py                 # Flask application
│   ├── wsgi.py               # WSGI entry point for production
│   ├── requirements.txt      # Python dependencies
│   └── README.md             # Backend documentation
├── startup.sh                # Startup script (starts Python + .NET)
└── appsettings.json         # Configuration (shared by both)
```

## Key Components

### 1. app.py - Flask Application

**Purpose:** Main Flask application that provides chat API endpoints

**Key Features:**
- Reads configuration from `appsettings.json`
- Connects to Azure OpenAI for chat completions
- Optionally connects to SQL Server for training data
- Provides health check endpoint
- Fallback to mock responses when Azure OpenAI is not configured

**Endpoints:**
- `GET /health` - Health check endpoint
- `POST /api/chat` - Chat completion endpoint
- `GET /api/config` - Public configuration endpoint

### 2. wsgi.py - WSGI Entry Point

**Purpose:** Production entry point for gunicorn WSGI server

**Content:**
```python
from app import app
application = app
```

**Why needed:** Gunicorn requires a WSGI application object. While `app:app` works, `wsgi:application` is the standard convention.

### 3. requirements.txt - Dependencies

```
flask==3.0.0
flask-cors==4.0.0
openai==1.54.3
gunicorn==21.2.0
pyodbc==5.1.0
```

### 4. startup.sh - Startup Script

**Purpose:** Orchestrates startup of Python backend and .NET application

**Process:**
1. Detects environment (Azure or local)
2. Validates `appsettings.json` exists
3. Installs Python dependencies
4. Starts gunicorn in daemon mode (background)
5. Verifies Python backend is responding
6. Starts .NET application

## Deployment

### Automatic Deployment (GitHub Actions)

The GitHub Actions workflow (`.github/workflows/main_orkinosai.yml`) automatically:

1. **Build Phase:**
   - Builds .NET application
   - Publishes to output directory
   - Copies Python backend files
   - Copies startup.sh script
   - Uploads artifact

2. **Deploy Phase:**
   - Downloads artifact
   - Deploys to Azure App Service

3. **Startup (Azure App Service):**
   - Executes `startup.sh`
   - Starts Python backend on port 8000
   - Starts .NET app on port 80/443

### Azure Configuration Required

#### 1. Startup Command

**Azure Portal → App Service → Configuration → General Settings → Startup Command:**

```bash
bash startup.sh
```

This tells Azure to run the startup script which handles both Python and .NET startup.

#### 2. Application Settings (Optional)

If you want to override `appsettings.json` values via environment variables:

```
AZURE_OPENAI_ENDPOINT=https://your-resource.openai.azure.com/
AZURE_OPENAI_API_KEY=your-api-key-here
AZURE_OPENAI_DEPLOYMENT_NAME=gpt-4o
AZURE_OPENAI_API_VERSION=2024-08-01-preview
```

However, the recommended approach is to update `appsettings.json` directly.

#### 3. Python Version

Azure App Service usually has Python 3.x pre-installed. If not:

**Azure Portal → App Service → Configuration → General Settings → Stack Settings:**
- Stack: Python
- Major version: 3
- Minor version: Latest

### Manual Deployment

If deploying manually:

```bash
# 1. Build and publish .NET application
dotnet publish src/orkinosaiCMS.Server/orkinosaiCMS.Server.csproj -c Release -o ./publish

# 2. Copy Python backend
cp -r src/orkinosaiCMS.Server/PythonBackend ./publish/

# 3. Copy startup script
cp src/orkinosaiCMS.Server/startup.sh ./publish/
chmod +x ./publish/startup.sh

# 4. Deploy to Azure (using Azure CLI)
cd publish
zip -r ../deploy.zip .
az webapp deployment source config-zip \
  --resource-group your-rg \
  --name orkinosai \
  --src ../deploy.zip
```

## Local Development

### Prerequisites

- Python 3.8+
- pip
- .NET 10.0 SDK

### Running Python Backend Locally

```bash
# Navigate to Python backend directory
cd src/orkinosaiCMS.Server/PythonBackend

# Install dependencies
pip install -r requirements.txt

# Run Flask development server
python app.py

# Backend will start on http://localhost:8000
```

### Testing Endpoints

```bash
# Health check
curl http://localhost:8000/health

# Test chat
curl -X POST http://localhost:8000/api/chat \
  -H "Content-Type: application/json" \
  -d '{"message": "Hello, Zoota!"}'

# Get configuration
curl http://localhost:8000/api/config
```

### Running Full Stack

**Terminal 1 - Python Backend:**
```bash
cd src/orkinosaiCMS.Server/PythonBackend
python app.py
```

**Terminal 2 - .NET Application:**
```bash
cd src/orkinosaiCMS.Server
dotnet run
```

Access at: `https://localhost:5001`

## Troubleshooting

### Issue 1: Python Backend Not Starting

**Symptoms:**
- Zoota chat shows error: "The Python backend is currently unavailable"
- Health check at `/health` fails
- No response from port 8000

**Diagnosis:**

1. **Check Azure Logs:**
```bash
# Via Azure Portal
App Service → Log Stream

# Or via Azure CLI
az webapp log tail --name orkinosai --resource-group your-rg

# Or via Kudu console
https://orkinosai.scm.azurewebsites.net/DebugConsole
# Navigate to /home/LogFiles/
# Check python_error.log and python_access.log
```

2. **Check if gunicorn is running:**
```bash
# In Kudu console or SSH
ps aux | grep gunicorn
```

3. **Check if port 8000 is listening:**
```bash
netstat -tulpn | grep 8000
# or
lsof -i :8000
```

**Common Causes & Solutions:**

#### A. Missing Dependencies

**Error in logs:**
```
ModuleNotFoundError: No module named 'flask'
```

**Solution:**
```bash
# In Kudu console
cd /home/site/wwwroot/PythonBackend
pip install -r requirements.txt
```

#### B. Missing wsgi.py

**Error in logs:**
```
Error: No module named 'wsgi'
```

**Solution:**
Ensure `wsgi.py` exists in PythonBackend directory. If missing, create it:
```python
from app import app
application = app
if __name__ == "__main__":
    app.run()
```

#### C. Port Already in Use

**Error in logs:**
```
[ERROR] Retrying in 1 second.
[ERROR] Can't connect to ('0.0.0.0', 8000)
```

**Solution:**
```bash
# Kill existing gunicorn process
pkill -f gunicorn
# Restart app service
```

#### D. appsettings.json Not Found

**Error in logs:**
```
Could not find appsettings.json
```

**Solution:**
Verify `appsettings.json` is in the publish output and deployed to Azure. Check GitHub Actions build logs.

#### E. Python Not Installed

**Error in logs:**
```
python3: command not found
```

**Solution:**
Configure Python stack in Azure Portal (see Azure Configuration section above).

### Issue 2: Python Backend Starts But Returns Errors

**Symptoms:**
- Backend health check passes
- Chat returns error messages
- Logs show Azure OpenAI connection failures

**Diagnosis:**

Check Python backend logs:
```bash
# In Azure Kudu console
cat /home/LogFiles/python_error.log
```

**Common Causes & Solutions:**

#### A. Azure OpenAI Not Configured

**Log message:**
```
WARNING - Azure OpenAI using placeholder values - responses will be mocked
```

**Solution:**
Update `appsettings.json` with real Azure OpenAI credentials:
```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "ApiKey": "your-api-key-here",
    "DeploymentName": "gpt-4o",
    "ApiVersion": "2024-08-01-preview"
  }
}
```

**Note:** This is expected behavior. Backend will return mock responses when Azure OpenAI is not configured.

#### B. Invalid Azure OpenAI Credentials

**Log message:**
```
ERROR - Azure OpenAI error: Authentication failed
```

**Solution:**
Verify credentials in Azure Portal:
1. Go to Azure OpenAI resource
2. Check "Keys and Endpoint"
3. Update `appsettings.json`

#### C. Deployment Name Incorrect

**Log message:**
```
ERROR - The deployment 'gpt-4o' could not be found
```

**Solution:**
1. Go to Azure OpenAI resource
2. Check "Model deployments"
3. Use exact deployment name in `appsettings.json`

#### D. Network/Firewall Issues

**Log message:**
```
ERROR - Connection timeout to Azure OpenAI
```

**Solution:**
1. Check Azure OpenAI resource firewall settings
2. Ensure App Service IP is whitelisted
3. Check if Azure OpenAI is in same region

### Issue 3: .NET App Can't Connect to Python Backend

**Symptoms:**
- Python backend is running (health check passes)
- Chat still shows "backend unavailable" error
- .NET logs show connection errors

**Diagnosis:**

Check .NET application logs in Azure Portal.

**Common Causes & Solutions:**

#### A. Port Mismatch

**Solution:**
Verify port in `appsettings.json` matches gunicorn configuration:
```json
{
  "PythonBackend": {
    "Port": 8000
  }
}
```

And in `startup.sh`:
```bash
gunicorn --bind 0.0.0.0:8000 ...
```

#### B. Python Backend Started After .NET

**Solution:**
Ensure startup script starts Python backend BEFORE .NET:
```bash
# startup.sh should start Python first
gunicorn --daemon ...  # Daemon mode (background)
sleep 2                # Wait for startup
dotnet orkinosaiCMS.Server.dll  # Then start .NET
```

#### C. HttpClient Timeout

**Solution:**
Increase timeout in `appsettings.json`:
```json
{
  "PythonBackend": {
    "Timeout": 120
  }
}
```

### Issue 4: Startup Script Not Executing

**Symptoms:**
- .NET app starts but Python backend doesn't
- No Python logs in `/home/LogFiles/`

**Diagnosis:**

Check Azure startup command configuration.

**Solution:**

1. **Set Startup Command in Azure Portal:**
   - Configuration → General Settings → Startup Command: `bash startup.sh`
   
2. **Verify startup.sh is executable:**
```bash
# In Kudu console
cd /home/site/wwwroot
ls -la startup.sh
# Should show -rwxr-xr-x (executable)

# If not:
chmod +x startup.sh
```

3. **Check startup.sh is in wwwroot:**
```bash
# In Kudu console
ls -la /home/site/wwwroot/startup.sh
# Should exist
```

### Issue 5: Works Locally But Not in Azure

**Symptoms:**
- Everything works in local development
- Fails in Azure deployment

**Common Differences:**

| Aspect | Local | Azure |
|--------|-------|-------|
| Working directory | `src/orkinosaiCMS.Server/` | `/home/site/wwwroot/` |
| appsettings.json location | Same directory | `/home/site/wwwroot/` |
| Log directory | `./logs/` | `/home/LogFiles/` |
| Python command | `python` or `python3` | Usually `python3` |
| Gunicorn | May not be installed | Must be in requirements.txt |

**Solution:**

Use environment detection in code (already implemented in `app.py`):
```python
config_paths = [
    Path(__file__).parent.parent / 'appsettings.json',  # Local
    Path('/home/site/wwwroot/appsettings.json'),        # Azure
]
```

## Monitoring

### Health Checks

**Python Backend Health:**
```bash
curl https://your-site.azurewebsites.net:8000/health
```

**Response (healthy):**
```json
{
  "status": "healthy",
  "service": "Zoota AI Backend",
  "azure_configured": true,
  "config_loaded": true
}
```

**Response (unhealthy):**
- Connection refused: Backend not running
- Timeout: Backend slow or hung
- 500 error: Backend crashed

**.NET Application Health:**
```bash
curl https://your-site.azurewebsites.net/health
```

### Log Monitoring

**Azure Portal Log Stream:**
1. Go to App Service
2. Click "Log stream"
3. Select "Application logs" or "Web server logs"

**Kudu Console:**
1. Go to `https://orkinosai.scm.azurewebsites.net/`
2. Navigate to `/home/LogFiles/`
3. Check:
   - `python_error.log` - Python backend errors
   - `python_access.log` - Python backend access logs
   - Application logs - .NET application logs

**Viewing Logs:**
```bash
# In Kudu console or SSH
tail -f /home/LogFiles/python_error.log
tail -f /home/LogFiles/python_access.log
```

### Performance Monitoring

**Application Insights (if configured):**
- Track request rates to `/api/chat`
- Monitor response times
- Track errors and exceptions
- Monitor Python backend health

## Configuration

### appsettings.json Structure

```json
{
  "PythonBackend": {
    "Enabled": true,
    "Port": 8000,
    "Host": "0.0.0.0",
    "Workers": 2,
    "Timeout": 120,
    "LogDirectory": "/home/LogFiles"
  },
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "ApiKey": "your-api-key-here",
    "DeploymentName": "gpt-4o",
    "ApiVersion": "2024-08-01-preview",
    "MaxTokens": 800,
    "Temperature": 0.7
  },
  "Zoota": {
    "SystemPrompt": "You are Zoota, a friendly AI assistant...",
    "WelcomeMessage": "Hi! I'm Zoota...",
    "KnowledgeBase": {
      "Services": [...],
      "Expertise": [...]
    }
  }
}
```

### Environment Variables (Alternative)

Instead of editing `appsettings.json`, you can set Azure Application Settings:

```
AZUREOPENAI__ENDPOINT=https://your-resource.openai.azure.com/
AZUREOPENAI__APIKEY=your-api-key-here
AZUREOPENAI__DEPLOYMENTNAME=gpt-4o
PYTHONBACKEND__PORT=8000
PYTHONBACKEND__TIMEOUT=120
```

Format: `SectionName__SubSection__Key` (double underscore)

## Security Best Practices

1. **Never commit API keys to Git**
   - Use placeholder values in repository
   - Set real values in Azure Portal or via CI/CD

2. **Use Azure Key Vault** (recommended for production)
   ```json
   {
     "AzureOpenAI": {
       "ApiKey": "@Microsoft.KeyVault(SecretUri=https://...)"
     }
   }
   ```

3. **Enable HTTPS only**
   - Azure Portal → TLS/SSL settings → HTTPS Only: On

4. **Restrict CORS** (if needed)
   - Update Flask CORS configuration in `app.py`

5. **Rate Limiting**
   - Consider adding rate limiting to `/api/chat` endpoint

6. **Log Sanitization**
   - Don't log sensitive data (API keys, personal info)

## Scaling

### Horizontal Scaling (Multiple Instances)

If running multiple app instances:

1. **Shared Session State:** Not required (stateless chat)
2. **Health Checks:** Each instance runs own Python backend
3. **Load Balancer:** Azure handles automatically

### Vertical Scaling (More Resources)

If chat is slow:

1. **Increase Workers:**
   ```bash
   gunicorn --workers 4 ...  # More worker processes
   ```

2. **Increase Timeout:**
   ```json
   {
     "PythonBackend": {
       "Timeout": 180
     }
   }
   ```

3. **Scale App Service Plan:**
   - Azure Portal → Scale up (larger VM)
   - Scale out (more instances)

## Testing Checklist

After deployment, verify:

- [ ] Website loads: `https://orkinosai.azurewebsites.net`
- [ ] .NET health check: `https://orkinosai.azurewebsites.net/health` → 200 OK
- [ ] Python health check: `https://orkinosai.azurewebsites.net:8000/health` → 200 OK (may need firewall rule)
- [ ] Chat button appears on website
- [ ] Chat opens and shows welcome message
- [ ] Can send message and get response
- [ ] No errors in browser console (F12)
- [ ] Python logs exist: `/home/LogFiles/python_*.log`
- [ ] No errors in Python logs
- [ ] Gunicorn process is running: `ps aux | grep gunicorn`

## Quick Command Reference

### Local Development
```bash
# Start Python backend
cd src/orkinosaiCMS.Server/PythonBackend
pip install -r requirements.txt
python app.py

# Start .NET app
cd src/orkinosaiCMS.Server
dotnet run

# Test Python backend
curl http://localhost:8000/health
curl -X POST http://localhost:8000/api/chat -H "Content-Type: application/json" -d '{"message": "test"}'
```

### Azure Debugging
```bash
# View logs
az webapp log tail --name orkinosai --resource-group your-rg

# Restart app
az webapp restart --name orkinosai --resource-group your-rg

# SSH into container (if Linux)
az webapp ssh --name orkinosai --resource-group your-rg

# Download logs
az webapp log download --name orkinosai --resource-group your-rg

# Check status
az webapp show --name orkinosai --resource-group your-rg --query state
```

### Kudu Console Commands
```bash
# Check gunicorn
ps aux | grep gunicorn
netstat -tulpn | grep 8000

# View logs
tail -f /home/LogFiles/python_error.log
tail -f /home/LogFiles/python_access.log

# Test backend
curl http://localhost:8000/health

# Kill and restart
pkill -f gunicorn
cd /home/site/wwwroot/PythonBackend
gunicorn --bind 0.0.0.0:8000 --workers 2 --daemon wsgi:application
```

## Support Resources

- **Quick Deployment Checklist:** `QUICK-DEPLOYMENT-CHECKLIST.md`
- **General Deployment Guide:** `DEPLOYMENT.md`
- **Configuration Guide:** `CONFIGURATION-GUIDE.md`
- **Python Backend README:** `src/orkinosaiCMS.Server/PythonBackend/README.md`
- **Architecture:** `ARCHITECTURE.md`

## Summary

✅ **Python backend provides AI chat functionality**  
✅ **Gunicorn runs backend in production**  
✅ **startup.sh orchestrates startup**  
✅ **All configuration in appsettings.json**  
✅ **Health checks for monitoring**  
✅ **Fallback responses when Azure OpenAI unavailable**  
✅ **Comprehensive logging for troubleshooting**

---

**Last Updated:** December 3, 2025  
**Version:** 1.0.0
