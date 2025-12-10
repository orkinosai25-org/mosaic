# Zoota AI Backend - Python Service

## Overview

This is the Python backend service for Zoota AI Assistant. It provides Azure OpenAI integration for the chat interface.

**Key Features:**
- ✅ Reads configuration from `appsettings.json` (no environment variables)
- ✅ Azure OpenAI integration
- ✅ Fallback to mock responses when Azure is not configured
- ✅ RESTful API with Flask
- ✅ CORS support for frontend integration
- ✅ Comprehensive logging
- ✅ Health check endpoint

---

## Configuration

All configuration is read from `../appsettings.json` (parent directory).

The Python backend automatically looks for `appsettings.json` in:
1. Parent directory (`../appsettings.json`)
2. Current directory (`./appsettings.json`)
3. Azure App Service path (`/home/site/wwwroot/appsettings.json`)

### Required Configuration Sections

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "ApiKey": "your-api-key",
    "DeploymentName": "gpt-4o",
    "ApiVersion": "2024-08-01-preview",
    "MaxTokens": 800,
    "Temperature": 0.7
  },
  "Zoota": {
    "SystemPrompt": "You are Zoota, a friendly AI assistant...",
    "KnowledgeBase": {
      "CompanyName": "OrkinosAI",
      "Services": ["..."],
      "Expertise": ["..."]
    }
  }
}
```

---

## Installation

### Prerequisites

- Python 3.8 or higher
- pip (Python package manager)

### Install Dependencies

```bash
pip install -r requirements.txt
```

### Dependencies

- `flask` - Web framework
- `flask-cors` - CORS support
- `openai` - Azure OpenAI SDK
- `gunicorn` - WSGI HTTP server (production)

---

## Running Locally

### Development Mode

```bash
python app.py
```

The server will start on `http://localhost:8000`

### Production Mode (with Gunicorn)

```bash
# Using app:app
gunicorn --bind 0.0.0.0:8000 --workers 2 --timeout 120 app:app

# OR using wsgi:application (recommended)
gunicorn --bind 0.0.0.0:8000 --workers 2 --timeout 120 wsgi:application
```

**Note:** `wsgi.py` is the recommended entry point for production deployments.

---

## API Endpoints

### 1. Health Check

**Endpoint:** `GET /health`

**Response:**
```json
{
  "status": "healthy",
  "service": "Zoota AI Backend",
  "azure_configured": true,
  "config_loaded": true
}
```

### 2. Chat

**Endpoint:** `POST /api/chat`

**Request:**
```json
{
  "message": "Tell me about OrkinosAI",
  "history": [
    {
      "role": "user",
      "content": "Hello"
    },
    {
      "role": "assistant",
      "content": "Hi! How can I help?"
    }
  ]
}
```

**Response:**
```json
{
  "message": "OrkinosAI is a technology consultancy...",
  "source": "azure_openai"
}
```

**Sources:**
- `azure_openai` - Response from Azure OpenAI
- `mock` - Mock response (Azure not configured)
- `mock_fallback` - Fallback after Azure error

### 3. Get Configuration

**Endpoint:** `GET /api/config`

**Response:**
```json
{
  "name": "Zoota AI Assistant",
  "version": "1.0.0",
  "welcomeMessage": "Hi! I'm Zoota...",
  "azureConfigured": true
}
```

---

## Azure OpenAI Integration

### When Azure OpenAI is Configured

The backend uses the Azure OpenAI Python SDK to generate responses.

**Features:**
- Maintains conversation history (last 10 messages)
- Uses system prompt from configuration
- Configurable parameters (temperature, max_tokens, etc.)
- Automatic error handling with fallback

### When Azure OpenAI is NOT Configured

The backend provides intelligent mock responses based on:
- Keywords in the user's message
- Knowledge base from configuration
- Pre-programmed conversation patterns

**Mock responses include:**
- Company information
- Service descriptions
- Contact information
- General assistance

---

## Logging

Logs are written to:
- **Console:** All log levels (INFO, WARNING, ERROR)
- **Access Log:** `/home/LogFiles/python_access.log` (Azure)
- **Error Log:** `/home/LogFiles/python_error.log` (Azure)

Log format:
```
2025-12-03 10:30:45 - app - INFO - Configuration loaded successfully
2025-12-03 10:30:46 - app - INFO - Azure OpenAI client initialized successfully
2025-12-03 10:30:50 - app - INFO - Received chat message: Tell me about...
```

---

## Deployment

### Azure App Service

The backend is automatically deployed with the .NET application using `startup.sh`.

**Startup Process:**
1. `startup.sh` installs Python dependencies
2. Starts Gunicorn server on port 8000 in daemon mode
3. Starts .NET application on default port

**Port Configuration:**
- Python backend: 8000
- .NET application: 80/443

### Testing Deployment

```bash
# Test health endpoint
curl http://localhost:8000/health

# Test chat endpoint
curl -X POST http://localhost:8000/api/chat \
  -H "Content-Type: application/json" \
  -d '{"message": "Hello"}'
```

---

## Error Handling

### Configuration Errors

If `appsettings.json` is not found:
```
ERROR: Could not find appsettings.json
```

**Solution:** Ensure appsettings.json is in the correct location.

### Azure OpenAI Errors

If Azure OpenAI fails:
- Error is logged
- Automatically falls back to mock responses
- Response includes `"source": "mock_fallback"`

### Network Errors

If the .NET application cannot reach the Python backend:
- .NET service has built-in fallback responses
- Check Python logs for startup errors
- Verify port 8000 is not blocked

---

## Development Tips

### Adding New Mock Responses

Edit the `get_mock_response()` function in `app.py`:

```python
def get_mock_response(user_message):
    message_lower = user_message.lower()
    
    if 'your_keyword' in message_lower:
        return "Your custom response"
    
    # ... rest of the function
```

### Customizing Azure OpenAI Parameters

Update `appsettings.json`:

```json
{
  "AzureOpenAI": {
    "Temperature": 0.5,      // Lower = more focused
    "MaxTokens": 1000,       // Longer responses
    "TopP": 0.9,             // Alternative to temperature
    "FrequencyPenalty": 0.2, // Reduce repetition
    "PresencePenalty": 0.2   // Encourage variety
  }
}
```

### Testing Without Azure OpenAI

Use placeholder values in `appsettings.json`:
```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource-name.openai.azure.com/",
    "ApiKey": "your-api-key-here"
  }
}
```

The backend will detect placeholders and use mock responses.

---

## Troubleshooting

### Issue: Backend not starting

**Check:**
```bash
# Check if Python is installed
python --version

# Check if dependencies are installed
pip list | grep -E "flask|openai|gunicorn"

# Try running manually
cd PythonBackend
python app.py
```

### Issue: Cannot find appsettings.json

**Check:**
```bash
# Verify file exists
ls -la ../appsettings.json

# Check working directory
pwd

# Try running from correct directory
cd src/orkinosaiCMS.Server/PythonBackend
python app.py
```

### Issue: Azure OpenAI errors

**Check:**
```bash
# Verify configuration in appsettings.json
cat ../appsettings.json | grep -A 10 "AzureOpenAI"

# Test endpoint connectivity
curl https://your-resource.openai.azure.com/

# Check Python logs
tail -f /home/LogFiles/python_error.log
```

---

## File Structure

```
PythonBackend/
├── app.py              # Main Flask application
├── wsgi.py            # WSGI entry point for production
├── requirements.txt    # Python dependencies
└── README.md          # This file
```

---

## Support

For issues or questions:
1. Check logs in `/home/LogFiles/python_*.log`
2. Verify `appsettings.json` is correct
3. Test endpoints with curl
4. Review this README

---

**Last Updated:** December 3, 2025  
**Version:** 1.0.0
