#!/bin/bash
# Pre-Startup Diagnostics Script for ASP.NET Core Application
# This script runs before application startup to validate configuration and environment
# Helps prevent HTTP Error 500.30 by catching configuration issues early

set -e

echo "========================================"
echo "ASP.NET Core Pre-Startup Diagnostics"
echo "========================================"
echo "Starting diagnostics at: $(date)"
echo ""

# Detect environment
if [ -d "/home/site/wwwroot" ]; then
    echo "Environment: Azure App Service"
    APP_DIR="/home/site/wwwroot"
    LOG_DIR="/home/LogFiles"
    IS_AZURE=true
else
    echo "Environment: Local/Development"
    APP_DIR="${1:-.}"
    LOG_DIR="./logs"
    IS_AZURE=false
fi

echo "Application Directory: $APP_DIR"
echo "Log Directory: $LOG_DIR"
echo ""

# Change to application directory
cd "$APP_DIR"

# Check 1: Verify web.config exists
echo "✓ Checking web.config..."
if [ ! -f "web.config" ]; then
    echo "❌ ERROR: web.config not found!"
    echo "   This file is REQUIRED for Azure App Service deployments"
    echo "   Without it, the application will fail with HTTP 500.30"
    exit 1
fi
echo "  ✓ web.config exists"

# Check 2: Verify stdout logging is enabled
if grep -q 'stdoutLogEnabled="true"' web.config; then
    echo "  ✓ stdout logging is enabled"
else
    echo "  ⚠️  WARNING: stdoutLogEnabled not set to true in web.config"
    echo "     This will make troubleshooting difficult"
fi

# Check 3: Verify logs directory exists
echo ""
echo "✓ Checking logs directory..."
if [ ! -d "logs" ]; then
    echo "  ⚠️  WARNING: logs directory does not exist, creating it..."
    mkdir -p logs
    echo "  ✓ logs directory created"
else
    echo "  ✓ logs directory exists"
fi

# Check 4: Verify application DLL exists
echo ""
echo "✓ Checking application files..."
if [ ! -f "OrkinosaiCMS.Web.dll" ]; then
    echo "❌ ERROR: OrkinosaiCMS.Web.dll not found!"
    echo "   This indicates the application was not published correctly"
    exit 1
fi
echo "  ✓ OrkinosaiCMS.Web.dll exists"

# Check 5: Verify critical directories
echo ""
echo "✓ Checking critical directories..."
for dir in "wwwroot" "App_Data"; do
    if [ ! -d "$dir" ]; then
        echo "  ⚠️  WARNING: $dir directory does not exist, creating it..."
        mkdir -p "$dir"
    else
        echo "  ✓ $dir exists"
    fi
done

# Check 6: Azure-specific checks
if [ "$IS_AZURE" = true ]; then
    echo ""
    echo "✓ Azure App Service specific checks..."
    
    # Check environment variables
    if [ -z "$ConnectionStrings__DefaultConnection" ] && [ -z "$SQLAZURECONNSTR_DefaultConnection" ]; then
        echo "  ⚠️  WARNING: Connection string not found in environment variables"
        echo "     Set ConnectionStrings__DefaultConnection in Azure Portal Configuration"
        echo "     Or use Connection strings section to set DefaultConnection"
    else
        echo "  ✓ Connection string environment variable is set"
    fi
    
    # Check if .NET runtime is available
    if command -v dotnet &> /dev/null; then
        DOTNET_VERSION=$(dotnet --version 2>&1 || echo "unknown")
        echo "  ✓ .NET runtime available: $DOTNET_VERSION"
    else
        echo "  ⚠️  WARNING: dotnet command not found"
        echo "     Ensure .NET 10 runtime is installed or use self-contained deployment"
    fi
fi

# Check 7: File permissions (Azure App Service specific)
if [ "$IS_AZURE" = true ]; then
    echo ""
    echo "✓ Checking file permissions..."
    
    # Test if we can write to logs directory
    if touch logs/.test 2>/dev/null; then
        rm -f logs/.test
        echo "  ✓ Can write to logs directory"
    else
        echo "  ⚠️  WARNING: Cannot write to logs directory"
        echo "     This may prevent stdout logging from working"
    fi
    
    # Test if we can write to App_Data
    if [ -d "App_Data" ]; then
        if touch App_Data/.test 2>/dev/null; then
            rm -f App_Data/.test
            echo "  ✓ Can write to App_Data directory"
        else
            echo "  ⚠️  WARNING: Cannot write to App_Data directory"
            echo "     This may affect Data Protection keys"
        fi
    fi
fi

# Summary
echo ""
echo "========================================"
echo "Pre-Startup Diagnostics Complete"
echo "========================================"
echo ""

# Check if there were any errors
if [ $? -eq 0 ]; then
    echo "✅ All critical checks passed"
    echo "✅ Application is ready to start"
    echo ""
    exit 0
else
    echo "❌ Some checks failed"
    echo "❌ Review the warnings above before starting the application"
    echo ""
    exit 1
fi
