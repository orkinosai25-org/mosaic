# Database Migration Application Script (PowerShell)
# Helps apply EF Core migrations with proper error handling and validation

param(
    [Parameter(Position=0)]
    [ValidateSet("update", "list", "script", "check", "verify", "clean")]
    [string]$Action = "update",
    
    [Parameter(Position=1)]
    [string]$MigrationName = "",
    
    [string]$ConnectionString = "",
    
    [switch]$Help
)

$ErrorActionPreference = "Stop"

function Write-ColorOutput {
    param(
        [string]$Message,
        [ConsoleColor]$ForegroundColor = [ConsoleColor]::White
    )
    
    $currentColor = $Host.UI.RawUI.ForegroundColor
    $Host.UI.RawUI.ForegroundColor = $ForegroundColor
    Write-Output $Message
    $Host.UI.RawUI.ForegroundColor = $currentColor
}

Write-ColorOutput "=== OrkinosaiCMS Database Migration Tool ===" -ForegroundColor Cyan
Write-Output ""

if ($Help) {
    Write-Output "Usage: .\apply-migrations.ps1 [action] [options]"
    Write-Output ""
    Write-Output "Actions:"
    Write-Output "  update              Apply all pending migrations (default)"
    Write-Output "  list                List all migrations"
    Write-Output "  script              Generate SQL script for all migrations"
    Write-Output "  check               Check for pending model changes"
    Write-Output "  verify              Verify database schema"
    Write-Output "  clean               Drop all tables and reapply migrations (DESTRUCTIVE!)"
    Write-Output ""
    Write-Output "Parameters:"
    Write-Output "  -ConnectionString   Override database connection string"
    Write-Output "  -MigrationName      (For 'script') Output file name"
    Write-Output ""
    Write-Output "Examples:"
    Write-Output "  .\apply-migrations.ps1 update"
    Write-Output "  .\apply-migrations.ps1 list"
    Write-Output "  .\apply-migrations.ps1 script -MigrationName migration.sql"
    Write-Output "  .\apply-migrations.ps1 -ConnectionString 'Server=...' update"
    exit 0
}

# Check prerequisites
try {
    $null = Get-Command dotnet -ErrorAction Stop
} catch {
    Write-ColorOutput "ERROR: .NET SDK not found. Please install .NET 10 SDK." -ForegroundColor Red
    Write-Output "Download from: https://dotnet.microsoft.com/download"
    exit 1
}

try {
    $null = Get-Command dotnet-ef -ErrorAction Stop
} catch {
    Write-ColorOutput "WARNING: EF Core tools not found. Installing..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
}

# Get script directory and project paths
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Split-Path -Parent $ScriptDir
$InfraProject = Join-Path $RepoRoot "src\OrkinosaiCMS.Infrastructure"
$WebProject = Join-Path $RepoRoot "src\OrkinosaiCMS.Web"

# Check if projects exist
if (!(Test-Path $InfraProject)) {
    Write-ColorOutput "ERROR: Infrastructure project not found at $InfraProject" -ForegroundColor Red
    exit 1
}

if (!(Test-Path $WebProject)) {
    Write-ColorOutput "ERROR: Web project not found at $WebProject" -ForegroundColor Red
    exit 1
}

function Invoke-EFCommand {
    param([string]$Command)
    
    Push-Location $InfraProject
    
    try {
        # Set environment to Production to avoid InMemory database
        $env:ASPNETCORE_ENVIRONMENT = "Production"
        
        # Override connection string if provided
        if ($ConnectionString) {
            $env:ConnectionStrings__DefaultConnection = $ConnectionString
            Write-ColorOutput "Using custom connection string" -ForegroundColor Cyan
        }
        
        # Run EF Core command
        Write-ColorOutput "Running: dotnet ef $Command" -ForegroundColor Cyan
        $output = dotnet ef $Command --project . --startup-project $WebProject --verbose 2>&1
        
        Write-Output $output
        
        if ($LASTEXITCODE -ne 0) {
            throw "EF Core command failed with exit code $LASTEXITCODE"
        }
        
        return $output
    }
    finally {
        Pop-Location
    }
}

switch ($Action) {
    "update" {
        Write-ColorOutput "Applying database migrations..." -ForegroundColor Yellow
        Write-Output ""
        
        # Check for pending changes first
        Write-Output "Checking for pending model changes..."
        $checkOutput = Invoke-EFCommand "migrations has-pending-model-changes"
        
        if ($checkOutput -match "pending changes") {
            Write-ColorOutput "ERROR: Model has pending changes that aren't captured in migrations!" -ForegroundColor Red
            Write-ColorOutput "Please create a new migration first:" -ForegroundColor Yellow
            Write-Output "  cd $InfraProject"
            Write-Output "  `$env:ASPNETCORE_ENVIRONMENT='Production'; dotnet ef migrations add YourMigrationName --startup-project $WebProject"
            exit 1
        }
        
        # List pending migrations
        Write-Output ""
        Write-Output "Pending migrations:"
        Invoke-EFCommand "migrations list --pending"
        
        # Apply migrations
        Write-Output ""
        Write-ColorOutput "Applying migrations to database..." -ForegroundColor Yellow
        
        try {
            Invoke-EFCommand "database update"
            Write-Output ""
            Write-ColorOutput "✓ Migrations applied successfully!" -ForegroundColor Green
            Write-Output ""
            Write-Output "Next steps:"
            Write-Output "  1. Verify database schema: .\apply-migrations.ps1 verify"
            Write-Output "  2. Start the application"
            Write-Output "  3. Login with default credentials (admin / Admin@123)"
        }
        catch {
            Write-Output ""
            Write-ColorOutput "✗ Migration failed!" -ForegroundColor Red
            Write-Output ""
            Write-Output "Troubleshooting:"
            Write-Output "  1. Check the error message above"
            Write-Output "  2. Review: docs\DATABASE_MIGRATION_TROUBLESHOOTING.md"
            Write-Output "  3. Verify database connection"
            Write-Output "  4. Check database permissions"
            exit 1
        }
    }
    
    "list" {
        Write-ColorOutput "Listing all migrations..." -ForegroundColor Yellow
        Write-Output ""
        Invoke-EFCommand "migrations list"
    }
    
    "script" {
        $OutputFile = if ($MigrationName) { $MigrationName } else { "$env:TEMP\migration-script.sql" }
        Write-ColorOutput "Generating SQL migration script..." -ForegroundColor Yellow
        Write-Output "Output: $OutputFile"
        Write-Output ""
        
        try {
            Invoke-EFCommand "migrations script --output $OutputFile"
            Write-Output ""
            Write-ColorOutput "✓ SQL script generated successfully!" -ForegroundColor Green
            Write-Output ""
            Write-Output "Review the script:"
            Write-Output "  notepad $OutputFile"
            Write-Output ""
            Write-Output "Apply manually to database:"
            Write-Output "  sqlcmd -S yourserver -d yourdatabase -i $OutputFile"
        }
        catch {
            Write-ColorOutput "Failed to generate script" -ForegroundColor Red
            exit 1
        }
    }
    
    "check" {
        Write-ColorOutput "Checking for pending model changes..." -ForegroundColor Yellow
        Write-Output ""
        
        try {
            $output = Invoke-EFCommand "migrations has-pending-model-changes"
            Write-Output ""
            Write-ColorOutput "✓ No pending changes" -ForegroundColor Green
        }
        catch {
            Write-Output ""
            Write-ColorOutput "✗ Model has pending changes!" -ForegroundColor Red
            Write-Output ""
            Write-Output "Create a new migration:"
            Write-Output "  cd $InfraProject"
            Write-Output "  `$env:ASPNETCORE_ENVIRONMENT='Production'; dotnet ef migrations add YourMigrationName --startup-project $WebProject"
        }
    }
    
    "verify" {
        Write-ColorOutput "Verifying database schema..." -ForegroundColor Yellow
        Write-Output ""
        
        Write-Output "Please verify the following tables exist in your database:"
        Write-Output ""
        Write-Output "Core Tables:"
        Write-Output "  ✓ Sites, Pages, MasterPages"
        Write-Output "  ✓ Modules, PageModules"
        Write-Output "  ✓ Themes"
        Write-Output "  ✓ Contents"
        Write-Output ""
        Write-Output "Identity Tables:"
        Write-Output "  ✓ AspNetUsers, AspNetRoles, AspNetUserRoles"
        Write-Output "  ✓ AspNetUserClaims, AspNetRoleClaims"
        Write-Output "  ✓ AspNetUserLogins, AspNetUserTokens"
        Write-Output ""
        Write-Output "Legacy Tables (backward compatibility):"
        Write-Output "  ✓ LegacyUsers, LegacyRoles, LegacyUserRoles"
        Write-Output "  ✓ Permissions, RolePermissions"
        Write-Output ""
        Write-Output "Subscription Tables:"
        Write-Output "  ✓ Customers, Subscriptions, Invoices, PaymentMethods"
        Write-Output ""
        Write-Output "Run this SQL query to verify:"
        Write-Output "  SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME;"
    }
    
    "clean" {
        Write-ColorOutput "WARNING: This will DROP ALL TABLES and reapply migrations!" -ForegroundColor Red
        Write-ColorOutput "All data will be LOST!" -ForegroundColor Red
        Write-Output ""
        
        $confirm = Read-Host "Are you sure? Type 'yes' to continue"
        
        if ($confirm -ne "yes") {
            Write-Output "Aborted."
            exit 0
        }
        
        Write-Output ""
        Write-ColorOutput "Dropping database and reapplying migrations..." -ForegroundColor Yellow
        
        try {
            Invoke-EFCommand "database drop --force"
            Write-Output ""
            Invoke-EFCommand "database update"
            Write-Output ""
            Write-ColorOutput "✓ Database recreated successfully!" -ForegroundColor Green
        }
        catch {
            Write-ColorOutput "Failed to recreate database" -ForegroundColor Red
            exit 1
        }
    }
}

Set-Location $RepoRoot
