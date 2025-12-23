#!/bin/bash
# Complete Application Startup Script
# This script handles the complete startup sequence with error handling and recovery
# Suitable for both local development and Azure App Service deployment

set -eo pipefail

# Color codes for output (disabled in Azure)
if [ -t 1 ]; then
    RED='\033[0;31m'
    GREEN='\033[0;32m'
    YELLOW='\033[1;33m'
    NC='\033[0m' # No Color
else
    RED=''
    GREEN=''
    YELLOW=''
    NC=''
fi

echo "========================================"
echo "MOSAIC Application Startup"
echo "========================================"
echo "Starting at: $(date)"
echo ""

# Detect environment
if [ -d "/home/site/wwwroot" ]; then
    ENVIRONMENT="Azure App Service"
    APP_DIR="/home/site/wwwroot"
    LOG_DIR="/home/LogFiles"
    IS_AZURE=true
else
    ENVIRONMENT="Local/Development"
    APP_DIR="${1:-.}"
    LOG_DIR="${APP_DIR}/logs"
    IS_AZURE=false
fi

echo "Environment: $ENVIRONMENT"
echo "Application Directory: $APP_DIR"
echo "Log Directory: $LOG_DIR"
echo ""

# Change to application directory
cd "$APP_DIR"

# Create necessary directories
echo "Setting up required directories..."
mkdir -p "$LOG_DIR"
mkdir -p "logs"
mkdir -p "App_Data/Logs"
mkdir -p "App_Data/DataProtection-Keys"
echo "✓ Directories created"
echo ""

# Step 1: Run pre-startup diagnostics
echo "========================================"
echo "Step 1: Pre-Startup Diagnostics"
echo "========================================"
echo ""

DIAGNOSTICS_SCRIPT="scripts/pre-startup-check.sh"
if [ -f "$DIAGNOSTICS_SCRIPT" ]; then
    bash "$DIAGNOSTICS_SCRIPT" "$APP_DIR" || {
        echo ""
        echo -e "${YELLOW}⚠️  WARNING: Pre-startup diagnostics found issues${NC}"
        echo "The application may fail to start. Review warnings above."
        echo ""
        
        # Give user a chance to cancel (not applicable in Azure)
        if [ "$IS_AZURE" = false ]; then
            read -p "Continue anyway? (y/n) " -n 1 -r
            echo ""
            if [[ ! $REPLY =~ ^[Yy]$ ]]; then
                echo "Startup cancelled by user"
                exit 1
            fi
        fi
    }
else
    echo "Pre-startup diagnostics script not found (optional)"
    echo "Continuing with startup..."
fi

echo ""

# Step 2: Start Python Backend (if present)
echo "========================================"
echo "Step 2: Python Backend (Zoota AI)"
echo "========================================"
echo ""

if [ -d "PythonBackend" ]; then
    echo "Found PythonBackend directory"
    cd PythonBackend
    
    # Check for required files
    if [ ! -f "requirements.txt" ] || [ ! -f "app.py" ]; then
        echo -e "${RED}❌ ERROR: Missing required Python files${NC}"
        echo "Expected: requirements.txt, app.py"
        cd ..
    else
        # Install dependencies
        echo "Installing Python dependencies..."
        pip3 install -r requirements.txt --quiet --no-cache-dir || {
            echo -e "${YELLOW}⚠️  WARNING: Failed to install some Python dependencies${NC}"
        }
        
        # Ensure wsgi.py exists
        if [ ! -f "wsgi.py" ]; then
            echo "Creating wsgi.py..."
            cat > wsgi.py << 'EOF'
from app import app
application = app
if __name__ == "__main__":
    app.run()
EOF
        fi
        
        # Kill existing gunicorn processes
        PORT_PID=$(lsof -ti:8000 2>/dev/null || true)
        if [ -n "$PORT_PID" ]; then
            echo "Stopping existing Python backend (PID: $PORT_PID)..."
            kill -15 $PORT_PID 2>/dev/null || true
            sleep 2
        fi
        
        # Start gunicorn
        echo "Starting Python backend on port 8000..."
        gunicorn --bind 0.0.0.0:8000 \
                 --workers 2 \
                 --timeout 120 \
                 --daemon \
                 --access-logfile "$LOG_DIR/python_access.log" \
                 --error-logfile "$LOG_DIR/python_error.log" \
                 --log-level info \
                 --pid "$LOG_DIR/gunicorn.pid" \
                 wsgi:application && {
            echo -e "${GREEN}✓ Python backend started${NC}"
            
            # Wait and verify
            sleep 2
            if curl -s http://localhost:8000/health > /dev/null 2>&1; then
                echo -e "${GREEN}✓ Python backend is responding${NC}"
            else
                echo -e "${YELLOW}⚠️  WARNING: Python backend health check failed${NC}"
            fi
        } || {
            echo -e "${RED}❌ Failed to start Python backend${NC}"
            echo "Check logs: $LOG_DIR/python_error.log"
        }
        
        cd ..
    fi
else
    echo "No PythonBackend directory found (optional)"
    echo "Zoota AI features will not be available"
fi

echo ""

# Step 3: Start .NET Application
echo "========================================"
echo "Step 3: .NET Application"
echo "========================================"
echo ""

if [ ! -f "OrkinosaiCMS.Web.dll" ]; then
    echo -e "${RED}❌ ERROR: OrkinosaiCMS.Web.dll not found!${NC}"
    echo ""
    echo "Expected deployment structure:"
    echo "  OrkinosaiCMS.Web.dll"
    echo "  web.config"
    echo "  appsettings.json"
    echo "  wwwroot/"
    echo ""
    echo "Current directory contents:"
    ls -la
    exit 1
fi

if [ ! -f "web.config" ]; then
    echo -e "${YELLOW}⚠️  WARNING: web.config not found${NC}"
    echo "This file is required for Azure App Service deployments"
fi

echo "✓ Application files verified"
echo ""
echo "Starting ASP.NET Core application..."
echo ""
echo "Logs will be written to:"
echo "  - stdout: $LOG_DIR/stdout/"
echo "  - Application: App_Data/Logs/"
if [ "$IS_AZURE" = true ]; then
    echo "  - Azure Log Stream: Azure Portal → Log Stream"
fi
echo ""
echo "Health check endpoints:"
echo "  - /api/health (overall health)"
echo "  - /api/health/ready (readiness probe)"
echo ""

# Use exec to replace shell with dotnet process
# This ensures signals (like SIGTERM) are properly forwarded
exec dotnet OrkinosaiCMS.Web.dll
