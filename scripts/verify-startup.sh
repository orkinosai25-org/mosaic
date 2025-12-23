#!/bin/bash
# Startup Verification Script - Validates application health after startup
# This script checks if the application started successfully and is responding
# Helps diagnose HTTP Error 500.30 and other startup failures

set -e

echo "========================================"
echo "ASP.NET Core Startup Verification"
echo "========================================"
echo "Starting verification at: $(date)"
echo ""

# Configuration
APP_URL="${1:-http://localhost:5000}"
HEALTH_ENDPOINT="${APP_URL}/api/health"
READY_ENDPOINT="${APP_URL}/api/health/ready"
MAX_RETRIES="${2:-10}"
RETRY_DELAY="${3:-5}"

echo "Configuration:"
echo "  Application URL: $APP_URL"
echo "  Health Endpoint: $HEALTH_ENDPOINT"
echo "  Max Retries: $MAX_RETRIES"
echo "  Retry Delay: ${RETRY_DELAY}s"
echo ""

# Function to check if application is responding
check_health() {
    local endpoint=$1
    local attempt=$2
    
    echo "Attempt $attempt/$MAX_RETRIES: Checking $endpoint..."
    
    # Try to get HTTP status code
    HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" "$endpoint" 2>/dev/null || echo "000")
    
    echo "  HTTP Status: $HTTP_CODE"
    
    if [ "$HTTP_CODE" = "200" ]; then
        echo "  ✅ Endpoint is healthy!"
        return 0
    elif [ "$HTTP_CODE" = "503" ]; then
        echo "  ⚠️  Service Unavailable (503) - Application started but not ready"
        return 1
    elif [ "$HTTP_CODE" = "000" ]; then
        echo "  ❌ Cannot connect to application"
        return 1
    else
        echo "  ⚠️  Unexpected status code: $HTTP_CODE"
        return 1
    fi
}

# Function to get detailed health status
get_health_details() {
    local endpoint=$1
    
    echo ""
    echo "Fetching detailed health status..."
    
    HEALTH_RESPONSE=$(curl -s "$endpoint" 2>/dev/null || echo "{}")
    
    if [ "$HEALTH_RESPONSE" != "{}" ]; then
        echo "Health Check Response:"
        
        # Try multiple JSON formatters in order of preference, with fallback to raw output
        format_json() {
            local json="$1"
            if command -v jq &> /dev/null; then
                echo "$json" | jq . 2>/dev/null && return 0
            elif command -v python3 &> /dev/null; then
                echo "$json" | python3 -m json.tool 2>/dev/null && return 0
            elif command -v python &> /dev/null; then
                echo "$json" | python -m json.tool 2>/dev/null && return 0
            fi
            # Fallback: just print the raw JSON
            echo "$json"
            return 0
        }
        
        format_json "$HEALTH_RESPONSE"
    else
        echo "  (No response received)"
    fi
    echo ""
}

# Main verification loop
echo "Starting health check loop..."
echo ""

SUCCESS=false

for i in $(seq 1 $MAX_RETRIES); do
    if check_health "$HEALTH_ENDPOINT" "$i"; then
        SUCCESS=true
        break
    else
        if [ $i -lt $MAX_RETRIES ]; then
            echo "  ⏳ Waiting ${RETRY_DELAY}s before retry..."
            sleep $RETRY_DELAY
        fi
    fi
    echo ""
done

if [ "$SUCCESS" = true ]; then
    echo "========================================"
    echo "✅ Startup Verification PASSED"
    echo "========================================"
    echo ""
    
    # Get detailed health information
    get_health_details "$HEALTH_ENDPOINT"
    
    # Check readiness endpoint
    echo "Checking readiness endpoint..."
    check_health "$READY_ENDPOINT" "1"
    get_health_details "$READY_ENDPOINT"
    
    echo "✅ Application is running and healthy!"
    echo "✅ Ready to accept traffic"
    echo ""
    exit 0
else
    echo "========================================"
    echo "❌ Startup Verification FAILED"
    echo "========================================"
    echo ""
    echo "The application did not respond after $MAX_RETRIES attempts."
    echo ""
    echo "Possible causes:"
    echo "  1. Application failed to start (HTTP Error 500.30)"
    echo "  2. Database connection issues"
    echo "  3. Missing configuration (connection strings, secrets)"
    echo "  4. Port already in use"
    echo "  5. Application is still starting (increase retry count)"
    echo ""
    echo "Troubleshooting steps:"
    echo "  1. Check stdout logs: tail -f logs/stdout*.log"
    echo "  2. Check application logs: tail -f App_Data/Logs/*.log"
    echo "  3. For Azure: Check Log Stream in Azure Portal"
    echo "  4. For Azure: Check Kudu logs at https://<app-name>.scm.azurewebsites.net"
    echo "  5. Review TROUBLESHOOTING_HTTP_500_30.md"
    echo ""
    exit 1
fi
