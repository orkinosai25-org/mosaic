#!/bin/bash
# Startup script for Azure App Service
# Starts Python backend and .NET application
# All configuration is read from appsettings.json
# Enhanced with pre-startup diagnostics to prevent HTTP 500.30 errors

set -e

echo "================================"
echo "Zoota AI Backend Startup Script"
echo "================================"
echo "Starting deployment at: $(date)"
echo ""

# Enable error logging to a file in Azure
if [ -d "/home/LogFiles" ]; then
    exec 1> >(tee -a /home/LogFiles/startup.log)
    exec 2> >(tee -a /home/LogFiles/startup_errors.log >&2)
    echo "Startup logs will be written to /home/LogFiles/startup.log"
fi

# Detect environment
if [ -d "/home/site/wwwroot" ]; then
    echo "Environment: Azure App Service"
    APP_DIR="/home/site/wwwroot"
    LOG_DIR="/home/LogFiles"
else
    echo "Environment: Local/Development"
    APP_DIR="."
    LOG_DIR="./logs"
fi

echo "Application Directory: $APP_DIR"
echo "Log Directory: $LOG_DIR"
echo ""

# Create log directory if it doesn't exist
mkdir -p "$LOG_DIR"

# Change to application directory
cd "$APP_DIR"

# Run pre-startup diagnostics if available
# This helps catch configuration issues before application startup
if [ -f "pre-startup-check.sh" ]; then
    echo "Running pre-startup diagnostics..."
    echo ""
    bash pre-startup-check.sh "$APP_DIR" || {
        echo ""
        echo "⚠️  WARNING: Pre-startup diagnostics found issues"
        echo "⚠️  Attempting to continue, but startup may fail"
        echo ""
    }
elif [ -f "scripts/pre-startup-check.sh" ]; then
    echo "Running pre-startup diagnostics..."
    echo ""
    bash scripts/pre-startup-check.sh "$APP_DIR" || {
        echo ""
        echo "⚠️  WARNING: Pre-startup diagnostics found issues"
        echo "⚠️  Attempting to continue, but startup may fail"
        echo ""
    }
fi

# Check if appsettings.json exists
if [ ! -f "appsettings.json" ]; then
    echo "ERROR: appsettings.json not found!"
    echo "All configuration must be in appsettings.json"
    exit 1
fi

echo "✓ Found appsettings.json"
echo ""

# Start Python Backend
echo "================================"
echo "Starting Python Backend (Zoota AI)"
echo "================================"

if [ -d "PythonBackend" ]; then
    cd PythonBackend
    
    # Check Python version
    PYTHON_VERSION=$(python3 --version 2>&1 || echo "Python not found")
    echo "Python version: $PYTHON_VERSION"
    
    # Check if requirements.txt exists
    if [ ! -f "requirements.txt" ]; then
        echo "ERROR: requirements.txt not found!"
        exit 1
    fi
    
    echo "Installing Python dependencies..."
    pip3 install -r requirements.txt --quiet --no-cache-dir || {
        echo "ERROR: Failed to install Python dependencies"
        echo "Attempting to continue..."
    }
    echo "✓ Python dependencies installed"
    echo ""
    
    # Verify app.py exists
    if [ ! -f "app.py" ]; then
        echo "ERROR: app.py not found!"
        exit 1
    fi
    
    # Verify wsgi.py exists
    if [ ! -f "wsgi.py" ]; then
        echo "ERROR: wsgi.py not found! This indicates a deployment problem."
        echo "Creating wsgi.py as emergency fallback..."
        cat > wsgi.py << 'EOF'
from app import app
application = app
if __name__ == "__main__":
    app.run()
EOF
        echo "⚠️ wsgi.py created, but this should be fixed in the deployment pipeline"
    fi
    
    # Test if gunicorn is available
    if ! command -v gunicorn &> /dev/null; then
        echo "WARNING: gunicorn not found, attempting installation..."
        pip3 install gunicorn --quiet || {
            echo "ERROR: Failed to install gunicorn"
            exit 1
        }
    fi
    
    # Kill any existing gunicorn processes on port 8000
    echo "Checking for existing Python backend processes..."
    
    # Try to find process using multiple methods (fallback for different systems)
    PORT_PID=""
    if command -v lsof &> /dev/null; then
        PORT_PID=$(lsof -ti:8000 2>/dev/null || true)
    elif command -v ss &> /dev/null; then
        PORT_PID=$(ss -tlnp 2>/dev/null | grep :8000 | grep -oP 'pid=\K\d+' | head -1 || true)
    elif command -v netstat &> /dev/null; then
        PORT_PID=$(netstat -tlnp 2>/dev/null | grep :8000 | awk '{print $7}' | cut -d'/' -f1 | head -1 || true)
    fi
    
    if [ -n "$PORT_PID" ]; then
        echo "Found existing process on port 8000 (PID: $PORT_PID), attempting graceful shutdown..."
        # Try graceful shutdown first (SIGTERM)
        kill -15 $PORT_PID 2>/dev/null || true
        sleep 3
        
        # Check if still running and force kill if needed (SIGKILL)
        if kill -0 $PORT_PID 2>/dev/null; then
            echo "Process still running, forcing termination..."
            kill -9 $PORT_PID 2>/dev/null || true
            sleep 2
        else
            echo "Process terminated gracefully"
        fi
    fi
    
    # Start gunicorn in daemon mode using wsgi:application
    echo "Starting gunicorn server on port 8000..."
    gunicorn --bind 0.0.0.0:8000 \
             --workers 2 \
             --timeout 120 \
             --daemon \
             --access-logfile "$LOG_DIR/python_access.log" \
             --error-logfile "$LOG_DIR/python_error.log" \
             --log-level info \
             --pid "$LOG_DIR/gunicorn.pid" \
             wsgi:application || {
        echo "ERROR: Failed to start gunicorn"
        echo "Checking logs..."
        [ -f "$LOG_DIR/python_error.log" ] && tail -20 "$LOG_DIR/python_error.log"
        exit 1
    }
    
    # Wait a moment for gunicorn to start
    sleep 2
    
    # Get PID with error handling
    if [ -f "$LOG_DIR/gunicorn.pid" ]; then
        GUNICORN_PID=$(cat "$LOG_DIR/gunicorn.pid")
        echo "✓ Python backend started (PID: $GUNICORN_PID)"
    else
        GUNICORN_PID=$(pgrep -f gunicorn | head -1 || echo "unknown")
        echo "✓ Python backend started (PID: $GUNICORN_PID)"
    fi
    
    echo "  - Logs: $LOG_DIR/python_*.log"
    echo "  - Endpoint: http://localhost:8000"
    echo ""
    
    # Test if Python backend is responding with retries
    echo "Testing Python backend health..."
    HEALTH_CHECK_RETRIES=5
    HEALTH_CHECK_DELAY=2
    HEALTH_CHECK_SUCCESS=0
    
    for i in $(seq 1 $HEALTH_CHECK_RETRIES); do
        if curl -s http://localhost:8000/health > /dev/null 2>&1; then
            echo "✓ Python backend is responding (attempt $i/$HEALTH_CHECK_RETRIES)"
            HEALTH_CHECK_SUCCESS=1
            break
        else
            echo "⏳ Waiting for Python backend (attempt $i/$HEALTH_CHECK_RETRIES)..."
            sleep $HEALTH_CHECK_DELAY
        fi
    done
    
    if [ $HEALTH_CHECK_SUCCESS -eq 0 ]; then
        echo "WARNING: Python backend health check failed after $HEALTH_CHECK_RETRIES attempts"
        echo "Zoota AI may not work correctly. Check logs at: $LOG_DIR/python_*.log"
    fi
    echo ""
    
    cd ..
else
    echo "WARNING: PythonBackend directory not found"
    echo "Zoota AI features will not be available"
    echo ""
fi

# Start .NET Application
echo "================================"
echo "Starting .NET Application"
echo "================================"

if [ -f "OrkinosaiCMS.Web.dll" ]; then
    echo "✓ Found OrkinosaiCMS.Web.dll"
    echo ""
    
    # Ensure logs directory exists for stdout logging
    mkdir -p logs
    
    # Ensure App_Data directory exists for Data Protection keys
    mkdir -p App_Data/DataProtection-Keys
    
    echo "Starting ASP.NET Core application..."
    echo "If the application fails to start, check:"
    echo "  - stdout logs in: $LOG_DIR/stdout/"
    echo "  - application logs in: App_Data/Logs/"
    echo "  - Azure log stream (if on Azure)"
    echo ""
    
    # Execute the .NET application
    # Using exec to replace the shell process with dotnet
    exec dotnet OrkinosaiCMS.Web.dll
else
    echo "ERROR: OrkinosaiCMS.Web.dll not found!"
    echo ""
    echo "This indicates a deployment problem. Expected files:"
    echo "  - OrkinosaiCMS.Web.dll (main application)"
    echo "  - web.config (for Azure/IIS hosting)"
    echo "  - appsettings.json (configuration)"
    echo ""
    echo "Current directory contents:"
    ls -la
    echo ""
    exit 1
fi
