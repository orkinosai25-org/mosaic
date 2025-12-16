#!/usr/bin/env bash
# Database Migration Application Script
# Helps apply EF Core migrations with proper error handling and validation

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}=== OrkinosaiCMS Database Migration Tool ===${NC}"
echo ""

# Check prerequisites
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}ERROR: .NET SDK not found. Please install .NET 10 SDK.${NC}"
    exit 1
fi

if ! command -v dotnet-ef &> /dev/null; then
    echo -e "${YELLOW}WARNING: EF Core tools not found. Installing...${NC}"
    dotnet tool install --global dotnet-ef
fi

# Get script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
REPO_ROOT="$(dirname "$SCRIPT_DIR")"
INFRA_PROJECT="$REPO_ROOT/src/OrkinosaiCMS.Infrastructure"
WEB_PROJECT="$REPO_ROOT/src/OrkinosaiCMS.Web"

# Check if projects exist
if [ ! -d "$INFRA_PROJECT" ]; then
    echo -e "${RED}ERROR: Infrastructure project not found at $INFRA_PROJECT${NC}"
    exit 1
fi

if [ ! -d "$WEB_PROJECT" ]; then
    echo -e "${RED}ERROR: Web project not found at $WEB_PROJECT${NC}"
    exit 1
fi

# Parse command line arguments
ACTION="${1:-update}"
MIGRATION_NAME="${2:-}"

print_usage() {
    echo "Usage: $0 [action] [options]"
    echo ""
    echo "Actions:"
    echo "  update              Apply all pending migrations (default)"
    echo "  list                List all migrations"
    echo "  script              Generate SQL script for all migrations"
    echo "  check               Check for pending model changes"
    echo "  verify              Verify database schema"
    echo "  clean               Drop all tables and reapply migrations (DESTRUCTIVE!)"
    echo ""
    echo "Environment Variables:"
    echo "  CONNECTION_STRING   Override database connection string"
    echo "  DATABASE_PROVIDER   Database provider (SqlServer, SQLite, InMemory)"
    echo ""
    echo "Examples:"
    echo "  $0 update                    # Apply all pending migrations"
    echo "  $0 list                      # List all migrations"
    echo "  $0 script                    # Generate SQL script"
    echo "  $0 check                     # Check for pending changes"
    echo "  CONNECTION_STRING='...' $0 update  # Use custom connection string"
}

# Function to run EF Core commands with proper environment
run_ef_command() {
    local ef_command="$1"
    
    cd "$INFRA_PROJECT"
    
    # Set environment to Production to avoid InMemory database
    export ASPNETCORE_ENVIRONMENT=Production
    
    # Override connection string if provided
    if [ -n "${CONNECTION_STRING:-}" ]; then
        export ConnectionStrings__DefaultConnection="$CONNECTION_STRING"
        echo -e "${BLUE}Using custom connection string${NC}"
    fi
    
    # Run EF Core command
    echo -e "${BLUE}Running: dotnet ef $ef_command${NC}"
    dotnet ef $ef_command --project . --startup-project "$WEB_PROJECT" --verbose
}

case "$ACTION" in
    update)
        echo -e "${YELLOW}Applying database migrations...${NC}"
        echo ""
        
        # Check for pending changes first
        echo "Checking for pending model changes..."
        if run_ef_command "migrations has-pending-model-changes" 2>&1 | grep -q "pending changes"; then
            echo -e "${RED}ERROR: Model has pending changes that aren't captured in migrations!${NC}"
            echo -e "${YELLOW}Please create a new migration first:${NC}"
            echo -e "  cd $INFRA_PROJECT"
            echo -e "  ASPNETCORE_ENVIRONMENT=Production dotnet ef migrations add YourMigrationName --startup-project $WEB_PROJECT"
            exit 1
        fi
        
        # List pending migrations
        echo ""
        echo "Pending migrations:"
        run_ef_command "migrations list --pending"
        
        # Apply migrations
        echo ""
        echo -e "${YELLOW}Applying migrations to database...${NC}"
        if run_ef_command "database update"; then
            echo ""
            echo -e "${GREEN}✓ Migrations applied successfully!${NC}"
            echo ""
            echo "Next steps:"
            echo "  1. Verify database schema: $0 verify"
            echo "  2. Start the application"
            echo "  3. Login with default credentials (admin / Admin@123)"
        else
            echo ""
            echo -e "${RED}✗ Migration failed!${NC}"
            echo ""
            echo "Troubleshooting:"
            echo "  1. Check the error message above"
            echo "  2. Review: docs/DATABASE_MIGRATION_TROUBLESHOOTING.md"
            echo "  3. Verify database connection"
            echo "  4. Check database permissions"
            exit 1
        fi
        ;;
        
    list)
        echo -e "${YELLOW}Listing all migrations...${NC}"
        echo ""
        run_ef_command "migrations list"
        ;;
        
    script)
        OUTPUT_FILE="${MIGRATION_NAME:-/tmp/migration-script.sql}"
        echo -e "${YELLOW}Generating SQL migration script...${NC}"
        echo "Output: $OUTPUT_FILE"
        echo ""
        
        if run_ef_command "migrations script --output $OUTPUT_FILE"; then
            echo ""
            echo -e "${GREEN}✓ SQL script generated successfully!${NC}"
            echo ""
            echo "Review the script:"
            echo "  less $OUTPUT_FILE"
            echo ""
            echo "Apply manually to database:"
            echo "  sqlcmd -S yourserver -d yourdatabase -i $OUTPUT_FILE"
        fi
        ;;
        
    check)
        echo -e "${YELLOW}Checking for pending model changes...${NC}"
        echo ""
        
        if run_ef_command "migrations has-pending-model-changes"; then
            echo ""
            echo -e "${GREEN}✓ No pending changes${NC}"
        else
            echo ""
            echo -e "${RED}✗ Model has pending changes!${NC}"
            echo ""
            echo "Create a new migration:"
            echo "  cd $INFRA_PROJECT"
            echo "  ASPNETCORE_ENVIRONMENT=Production dotnet ef migrations add YourMigrationName --startup-project $WEB_PROJECT"
        fi
        ;;
        
    verify)
        echo -e "${YELLOW}Verifying database schema...${NC}"
        echo ""
        
        # This would require a SQL query - for now just show what to check
        echo "Please verify the following tables exist in your database:"
        echo ""
        echo "Core Tables:"
        echo "  ✓ Sites, Pages, MasterPages"
        echo "  ✓ Modules, PageModules"
        echo "  ✓ Themes"
        echo "  ✓ Contents"
        echo ""
        echo "Identity Tables:"
        echo "  ✓ AspNetUsers, AspNetRoles, AspNetUserRoles"
        echo "  ✓ AspNetUserClaims, AspNetRoleClaims"
        echo "  ✓ AspNetUserLogins, AspNetUserTokens"
        echo ""
        echo "Legacy Tables (backward compatibility):"
        echo "  ✓ LegacyUsers, LegacyRoles, LegacyUserRoles"
        echo "  ✓ Permissions, RolePermissions"
        echo ""
        echo "Subscription Tables:"
        echo "  ✓ Customers, Subscriptions, Invoices, PaymentMethods"
        echo ""
        echo "Run this SQL query to verify:"
        echo "  SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME;"
        ;;
        
    clean)
        echo -e "${RED}WARNING: This will DROP ALL TABLES and reapply migrations!${NC}"
        echo -e "${RED}All data will be LOST!${NC}"
        echo ""
        read -p "Are you sure? Type 'yes' to continue: " confirm
        
        if [ "$confirm" != "yes" ]; then
            echo "Aborted."
            exit 0
        fi
        
        echo ""
        echo -e "${YELLOW}Dropping database and reapplying migrations...${NC}"
        
        if run_ef_command "database drop --force"; then
            echo ""
            run_ef_command "database update"
            echo ""
            echo -e "${GREEN}✓ Database recreated successfully!${NC}"
        fi
        ;;
        
    *)
        echo -e "${RED}Unknown action: $ACTION${NC}"
        echo ""
        print_usage
        exit 1
        ;;
esac

cd "$REPO_ROOT"
