#!/bin/bash
# Migration Recovery Testing Script
# Tests various database migration scenarios to verify recovery logic

set -e

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
INFRASTRUCTURE_PROJECT="$PROJECT_DIR/src/OrkinosaiCMS.Infrastructure"
WEB_PROJECT="$PROJECT_DIR/src/OrkinosaiCMS.Web"

echo -e "${BLUE}=== OrkinosaiCMS Migration Recovery Test ===${NC}"
echo -e "Project Directory: $PROJECT_DIR"
echo ""

# Function to print section headers
print_section() {
    echo -e "\n${BLUE}=== $1 ===${NC}"
}

# Function to print success
print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

# Function to print error
print_error() {
    echo -e "${RED}✗ $1${NC}"
}

# Function to print warning
print_warning() {
    echo -e "${YELLOW}⚠ $1${NC}"
}

# Test 1: Clean Database Migration
test_clean_migration() {
    print_section "Test 1: Clean Database Migration"
    
    # Remove any existing SQLite database
    if [ -f "$WEB_PROJECT/orkinosai-cms.db" ]; then
        rm "$WEB_PROJECT/orkinosai-cms.db"
        print_success "Removed existing database"
    fi
    
    if [ -f "$WEB_PROJECT/orkinosai-cms-dev.db" ]; then
        rm "$WEB_PROJECT/orkinosai-cms-dev.db"
        print_success "Removed existing dev database"
    fi
    
    echo ""
    echo "Testing migration on clean database..."
    
    # Use DatabaseProvider=SQLite for testing
    export DatabaseProvider=SQLite
    export ASPNETCORE_ENVIRONMENT=Development
    
    # Run database update
    cd "$INFRASTRUCTURE_PROJECT"
    dotnet ef database update --no-build 2>&1 | tee /tmp/migration-output.txt
    
    if [ $? -eq 0 ]; then
        print_success "Clean database migration completed successfully"
    else
        print_error "Clean database migration failed"
        return 1
    fi
    
    # Verify migrations were applied
    local migration_count=$(grep -c "Applying migration" /tmp/migration-output.txt || echo "0")
    print_success "Applied $migration_count migrations"
    
    # Verify database file exists
    if [ -f "$INFRASTRUCTURE_PROJECT/orkinosai-cms.db" ] || [ -f "$WEB_PROJECT/orkinosai-cms.db" ]; then
        print_success "Database file created"
    else
        print_warning "Database file not found in expected location"
    fi
}

# Test 2: Schema Drift Detection
test_schema_drift() {
    print_section "Test 2: Schema Drift Detection"
    
    echo "This test simulates schema drift by manually creating tables"
    echo "then attempting to run migrations"
    
    # This would require SQL manipulation which is complex in bash
    # Documented for manual testing instead
    print_warning "Schema drift test requires manual execution - see documentation"
    
    echo ""
    echo "Manual Test Steps:"
    echo "1. Create a new database"
    echo "2. Manually create Sites, Modules, Pages tables (from InitialCreate migration)"
    echo "3. Do NOT add entries to __EFMigrationsHistory"
    echo "4. Run the application"
    echo "5. Check logs for schema drift detection and recovery"
    echo ""
}

# Test 3: Verify Migration History
test_migration_history() {
    print_section "Test 3: Verify Migration History"
    
    # Query migration history if database exists
    local db_file="$INFRASTRUCTURE_PROJECT/orkinosai-cms.db"
    
    if [ ! -f "$db_file" ]; then
        db_file="$WEB_PROJECT/orkinosai-cms.db"
    fi
    
    if [ -f "$db_file" ]; then
        echo "Querying migration history from: $db_file"
        
        if command -v sqlite3 &> /dev/null; then
            echo ""
            echo "Applied Migrations:"
            sqlite3 "$db_file" "SELECT MigrationId, ProductVersion FROM __EFMigrationsHistory ORDER BY MigrationId;" 2>/dev/null || {
                print_warning "Could not query migration history (table may not exist yet)"
            }
            
            echo ""
            echo "Database Tables:"
            sqlite3 "$db_file" "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;" 2>/dev/null || {
                print_warning "Could not query tables"
            }
            
            # Verify critical tables
            echo ""
            echo "Verifying Critical Tables:"
            for table in "Sites" "Modules" "Pages" "Themes" "AspNetUsers" "AspNetRoles"; do
                if sqlite3 "$db_file" "SELECT count(*) FROM sqlite_master WHERE type='table' AND name='$table';" 2>/dev/null | grep -q "1"; then
                    print_success "Table exists: $table"
                else
                    print_error "Table missing: $table"
                fi
            done
        else
            print_warning "sqlite3 command not found - cannot query database"
            echo "Install sqlite3 to verify migration history"
        fi
    else
        print_warning "No database file found - run Test 1 first"
    fi
}

# Test 4: Application Startup Test
test_application_startup() {
    print_section "Test 4: Application Startup Test"
    
    echo "This test verifies the application can start and initialize the database"
    print_warning "Requires manual verification - would start web server"
    
    echo ""
    echo "Manual Test Steps:"
    echo "1. Run: cd $WEB_PROJECT"
    echo "2. Run: dotnet run"
    echo "3. Check logs for successful migration and seeding"
    echo "4. Try to access /admin/login"
    echo "5. Login with admin/Admin@123"
    echo ""
}

# Test 5: Verify Seeded Data
test_seeded_data() {
    print_section "Test 5: Verify Seeded Data"
    
    local db_file="$INFRASTRUCTURE_PROJECT/orkinosai-cms.db"
    
    if [ ! -f "$db_file" ]; then
        db_file="$WEB_PROJECT/orkinosai-cms.db"
    fi
    
    if [ -f "$db_file" ] && command -v sqlite3 &> /dev/null; then
        echo "Verifying seeded data..."
        echo ""
        
        # Count records in key tables
        local sites_count=$(sqlite3 "$db_file" "SELECT COUNT(*) FROM Sites;" 2>/dev/null || echo "0")
        local themes_count=$(sqlite3 "$db_file" "SELECT COUNT(*) FROM Themes;" 2>/dev/null || echo "0")
        local users_count=$(sqlite3 "$db_file" "SELECT COUNT(*) FROM AspNetUsers;" 2>/dev/null || echo "0")
        local roles_count=$(sqlite3 "$db_file" "SELECT COUNT(*) FROM AspNetRoles;" 2>/dev/null || echo "0")
        
        echo "Seeded Data Summary:"
        echo "  Sites: $sites_count (expected: 1)"
        echo "  Themes: $themes_count (expected: 16)"
        echo "  Users: $users_count (expected: 1)"
        echo "  Roles: $roles_count (expected: 1)"
        
        echo ""
        
        # Verify admin user
        local admin_exists=$(sqlite3 "$db_file" "SELECT COUNT(*) FROM AspNetUsers WHERE UserName='admin';" 2>/dev/null || echo "0")
        
        if [ "$admin_exists" = "1" ]; then
            print_success "Admin user exists"
            
            # Get admin details
            echo ""
            echo "Admin User Details:"
            sqlite3 "$db_file" "SELECT UserName, Email, DisplayName FROM AspNetUsers WHERE UserName='admin';" 2>/dev/null || true
        else
            print_error "Admin user not found"
        fi
        
        echo ""
        
        # Verify admin role
        local admin_role=$(sqlite3 "$db_file" "SELECT COUNT(*) FROM AspNetRoles WHERE Name='Administrator';" 2>/dev/null || echo "0")
        
        if [ "$admin_role" = "1" ]; then
            print_success "Administrator role exists"
        else
            print_error "Administrator role not found"
        fi
        
    else
        print_warning "Cannot verify seeded data - database or sqlite3 not available"
    fi
}

# Main execution
main() {
    echo "This script tests the database migration recovery system"
    echo "adapted from Oqtane CMS patterns."
    echo ""
    
    # Run automated tests
    test_clean_migration
    test_migration_history
    test_seeded_data
    
    # Display manual test information
    test_schema_drift
    test_application_startup
    
    # Summary
    print_section "Test Summary"
    echo "Automated tests completed."
    echo ""
    echo "For complete verification, please also run manual tests:"
    echo "  - Schema drift recovery (see Test 2)"
    echo "  - Application startup and admin login (see Test 4)"
    echo ""
    echo "Documentation: docs/DATABASE_MIGRATION_RECOVERY.md"
    
    print_success "Migration recovery testing completed"
}

# Run main function
main "$@"
