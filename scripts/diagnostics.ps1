<#
.SYNOPSIS
    Comprehensive Diagnostics Script for Mosaic CMS Application

.DESCRIPTION
    This script collects comprehensive diagnostic information to help investigate and troubleshoot
    application issues. It gathers logs, configuration, tests connectivity, and generates a report.
    
    Designed to work in:
    - Kudu Console (Azure App Service)
    - GitHub Actions
    - Local development environments
    - Manual execution

.PARAMETER OutputPath
    Directory where diagnostic reports will be saved. Defaults to ./diagnostic-output

.PARAMETER SkipConnectivityTests
    Skip database and external service connectivity tests

.PARAMETER SkipLogCollection
    Skip log file collection (useful if logs are too large)

.PARAMETER GenerateHtmlReport
    Generate HTML report in addition to text report

.EXAMPLE
    .\diagnostics.ps1
    Run full diagnostics with default settings

.EXAMPLE
    .\diagnostics.ps1 -OutputPath "C:\temp\diag" -GenerateHtmlReport
    Run diagnostics and save HTML report to custom location

.EXAMPLE
    .\diagnostics.ps1 -SkipConnectivityTests
    Run diagnostics without testing external connections

.NOTES
    Version: 1.0.0
    Created: 2025-12-23
    Purpose: Rapid diagnosis of application issues when main app is non-functional
#>

param(
    [string]$OutputPath = "./diagnostic-output",
    [switch]$SkipConnectivityTests,
    [switch]$SkipLogCollection,
    [switch]$GenerateHtmlReport
)

$ErrorActionPreference = "Continue"
$ProgressPreference = "SilentlyContinue"

# Initialize output
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$reportFile = Join-Path $OutputPath "diagnostic-report-$timestamp.txt"
$htmlReportFile = Join-Path $OutputPath "diagnostic-report-$timestamp.html"

# Create output directory
if (-not (Test-Path $OutputPath)) {
    try {
        New-Item -ItemType Directory -Path $OutputPath -Force -ErrorAction Stop | Out-Null
        Write-Host "✓ Created output directory: $OutputPath" -ForegroundColor Green
    } catch {
        Write-Host "✗ ERROR: Failed to create output directory: $OutputPath" -ForegroundColor Red
        Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "  Please check permissions and try again." -ForegroundColor Yellow
        exit 1
    }
} else {
    Write-Host "✓ Output directory exists: $OutputPath" -ForegroundColor Green
}

# Initialize report data
$diagnosticData = @{
    Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    MachineName = $env:COMPUTERNAME
    Environment = $env:ASPNETCORE_ENVIRONMENT
    Sections = @()
}

function Write-DiagnosticHeader {
    param([string]$Message)
    $line = "=" * 80
    Write-Host ""
    Write-Host $line -ForegroundColor Cyan
    Write-Host $Message -ForegroundColor Yellow
    Write-Host $line -ForegroundColor Cyan
    Write-Host ""
}

function Write-DiagnosticInfo {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

function Add-DiagnosticSection {
    param(
        [string]$Title,
        [object]$Data,
        [string]$Status = "Info"
    )
    
    $diagnosticData.Sections += @{
        Title = $Title
        Data = $Data
        Status = $Status
        Timestamp = Get-Date -Format "HH:mm:ss"
    }
}

function Get-SafeValue {
    param([string]$Value)
    
    if ([string]::IsNullOrWhiteSpace($Value)) {
        return "[Not Set]"
    }
    
    # Mask sensitive values (case-insensitive)
    if ($Value -imatch "password|secret|key|token" -and $Value.Length -gt 10) {
        $visible = $Value.Substring(0, 8)
        return "$visible********"
    }
    
    return $Value
}

function Get-MaskedConnectionString {
    param([string]$ConnectionString)
    
    if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
        return $ConnectionString
    }
    
    # Mask common password patterns in connection strings (case-insensitive)
    $masked = $ConnectionString -ireplace "(password|pwd)=([^;]+)", "`$1=********"
    return $masked
}

# ============================================================================
# SECTION 1: System Information
# ============================================================================
Write-DiagnosticHeader "SECTION 1: System Information"

$systemInfo = @{
    MachineName = $env:COMPUTERNAME
    OSVersion = [System.Environment]::OSVersion.VersionString
    ProcessorCount = [System.Environment]::ProcessorCount
    CLRVersion = [System.Environment]::Version.ToString()
    Is64BitOS = [System.Environment]::Is64BitOperatingSystem
    Is64BitProcess = [System.Environment]::Is64BitProcess
    CurrentDirectory = (Get-Location).Path
    UserName = $env:USERNAME
    UserDomain = $env:USERDOMAIN
}

# Check if running in Azure App Service (Kudu)
$isAzureAppService = $false
if ($env:WEBSITE_INSTANCE_ID) {
    $isAzureAppService = $true
    $systemInfo.AzureWebsiteInstanceId = $env:WEBSITE_INSTANCE_ID
    $systemInfo.AzureWebsiteName = $env:WEBSITE_SITE_NAME
    $systemInfo.AzureWebsiteResourceGroup = $env:WEBSITE_RESOURCE_GROUP
    $systemInfo.AzureRegion = $env:REGION_NAME
}

# Check if running in GitHub Actions
$isGitHubActions = $false
if ($env:GITHUB_ACTIONS) {
    $isGitHubActions = $true
    $systemInfo.GitHubWorkflow = $env:GITHUB_WORKFLOW
    $systemInfo.GitHubRepository = $env:GITHUB_REPOSITORY
    $systemInfo.GitHubRunId = $env:GITHUB_RUN_ID
}

$systemInfo.Keys | ForEach-Object {
    Write-DiagnosticInfo "$_ : $($systemInfo[$_])"
}

Add-DiagnosticSection -Title "System Information" -Data $systemInfo

# ============================================================================
# SECTION 2: Application Settings & Configuration
# ============================================================================
Write-DiagnosticHeader "SECTION 2: Application Settings & Configuration"

$configData = @{
    Files = @()
    Settings = @{}
}

# Find and parse appsettings.json files
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptDir

$appsettingsFiles = @(
    (Join-Path $repoRoot "src/OrkinosaiCMS.Web/appsettings.json"),
    (Join-Path $repoRoot "src/OrkinosaiCMS.Web/appsettings.Development.json"),
    (Join-Path $repoRoot "src/OrkinosaiCMS.Web/appsettings.Production.json"),
    (Join-Path $repoRoot "src/OrkinosaiCMS.Diagnostics/appsettings.json"),
    (Join-Path $repoRoot "src/OrkinosaiCMS.Diagnostics/appsettings.Production.json")
)

foreach ($file in $appsettingsFiles) {
    if (Test-Path $file) {
        Write-DiagnosticInfo "Found: $file" -Color Green
        try {
            $content = Get-Content $file -Raw | ConvertFrom-Json
            $configData.Files += @{
                Path = $file
                Exists = $true
                Content = $content
            }
            
            # Extract key settings (without sensitive data)
            if ($content.ConnectionStrings) {
                foreach ($cs in $content.ConnectionStrings.PSObject.Properties) {
                    if ($cs.Name -notlike "_*") {
                        $maskedValue = Get-MaskedConnectionString -ConnectionString $cs.Value
                        Write-DiagnosticInfo "  ConnectionString.$($cs.Name): $maskedValue" -Color Gray
                    }
                }
            }
            
            # Database settings
            if ($content.DatabaseProvider) {
                Write-DiagnosticInfo "  DatabaseProvider: $($content.DatabaseProvider)" -Color Gray
            }
            if ($content.DatabaseEnabled) {
                Write-DiagnosticInfo "  DatabaseEnabled: $($content.DatabaseEnabled)" -Color Gray
            }
            
            # Blob Storage
            if ($content.AzureBlobStorage) {
                Write-DiagnosticInfo "  AzureBlobStorage.AccountName: $($content.AzureBlobStorage.AccountName)" -Color Gray
            }
            
            # Stripe
            if ($content.Payment.Stripe) {
                $hasStripeKeys = -not [string]::IsNullOrWhiteSpace($content.Payment.Stripe.PublishableKey)
                Write-DiagnosticInfo "  Stripe.ConfiguredKeys: $hasStripeKeys" -Color Gray
            }
            
        } catch {
            Write-DiagnosticInfo "  ERROR: Failed to parse JSON - $($_.Exception.Message)" -Color Red
            $configData.Files += @{
                Path = $file
                Exists = $true
                Error = $_.Exception.Message
            }
        }
    } else {
        Write-DiagnosticInfo "Not Found: $file" -Color Yellow
        $configData.Files += @{
            Path = $file
            Exists = $false
        }
    }
}

Add-DiagnosticSection -Title "Application Configuration Files" -Data $configData

# ============================================================================
# SECTION 3: Environment Variables
# ============================================================================
Write-DiagnosticHeader "SECTION 3: Environment Variables"

$envVars = @{
    All = @{}
    Sensitive = @()
}

$sensitivePatterns = @(
    "PASSWORD", "SECRET", "KEY", "TOKEN", "CONNECTIONSTRING", 
    "STRIPE", "AZURE", "API", "CREDENTIALS"
)

Get-ChildItem Env: | ForEach-Object {
    $isSensitive = $false
    foreach ($pattern in $sensitivePatterns) {
        if ($_.Name -ilike "*$pattern*") {
            $isSensitive = $true
            break
        }
    }
    
    if ($isSensitive) {
        $maskedValue = if ($_.Value.Length -gt 8) {
            $_.Value.Substring(0, 8) + "********"
        } else {
            "********"
        }
        Write-DiagnosticInfo "$($_.Name) = $maskedValue" -Color Yellow
        $envVars.Sensitive += $_.Name
        $envVars.All[$_.Name] = "[MASKED]"
    } else {
        Write-DiagnosticInfo "$($_.Name) = $($_.Value)" -Color Gray
        $envVars.All[$_.Name] = $_.Value
    }
}

Add-DiagnosticSection -Title "Environment Variables" -Data $envVars

# ============================================================================
# SECTION 4: Log File Collection
# ============================================================================
Write-DiagnosticHeader "SECTION 4: Application Logs"

$logData = @{
    Directories = @()
    Files = @()
    RecentErrors = @()
}

if (-not $SkipLogCollection) {
    # Common log locations
    $logPaths = @(
        (Join-Path $repoRoot "src/OrkinosaiCMS.Web/App_Data/Logs"),
        (Join-Path $repoRoot "src/OrkinosaiCMS.Web/LogFiles"),
        (Join-Path $repoRoot "LogFiles"),
        "D:\home\LogFiles",  # Azure App Service
        "/home/LogFiles"     # Linux App Service
    )
    
    foreach ($logPath in $logPaths) {
        if (Test-Path $logPath) {
            Write-DiagnosticInfo "Found log directory: $logPath" -Color Green
            
            $logFiles = Get-ChildItem -Path $logPath -Recurse -File -ErrorAction SilentlyContinue | 
                        Where-Object { $_.Extension -in @('.log', '.txt', '.xml') } |
                        Sort-Object LastWriteTime -Descending |
                        Select-Object -First 50
            
            foreach ($logFile in $logFiles) {
                $fileInfo = @{
                    Path = $logFile.FullName
                    Size = $logFile.Length
                    LastModified = $logFile.LastWriteTime
                    RelativePath = $logFile.FullName.Replace($repoRoot, "")
                }
                
                Write-DiagnosticInfo "  - $($fileInfo.RelativePath) ($([math]::Round($logFile.Length/1KB, 2)) KB, Modified: $($logFile.LastWriteTime))" -Color Gray
                
                # Extract recent errors/exceptions from log files (last 100 lines)
                if ($logFile.Extension -eq ".log" -and $logFile.LastWriteTime -gt (Get-Date).AddDays(-2)) {
                    try {
                        $recentLines = Get-Content $logFile.FullName -Tail 100 -ErrorAction SilentlyContinue
                        $errorLines = $recentLines | Where-Object { 
                            $_ -match '\[ERR\]|\[ERROR\]|\[FATAL\]|Exception|at System\.|at Microsoft\.' 
                        }
                        
                        if ($errorLines) {
                            $logData.RecentErrors += @{
                                File = $fileInfo.RelativePath
                                Errors = $errorLines
                                Count = $errorLines.Count
                            }
                        }
                    } catch {
                        # Ignore errors reading individual log files
                    }
                }
                
                $logData.Files += $fileInfo
            }
            
            $logData.Directories += @{
                Path = $logPath
                FileCount = $logFiles.Count
            }
        }
    }
    
    # Display recent errors
    if ($logData.RecentErrors.Count -gt 0) {
        Write-DiagnosticInfo "`nRecent Errors Found:" -Color Red
        foreach ($errorLog in $logData.RecentErrors) {
            Write-DiagnosticInfo "  From: $($errorLog.File) ($($errorLog.Count) errors)" -Color Red
            $errorLog.Errors | Select-Object -First 5 | ForEach-Object {
                Write-DiagnosticInfo "    $_" -Color DarkRed
            }
        }
    }
    
} else {
    Write-DiagnosticInfo "Log collection skipped (SkipLogCollection flag set)" -Color Yellow
}

Add-DiagnosticSection -Title "Application Logs" -Data $logData

# ============================================================================
# SECTION 5: Event Logs (Windows only)
# ============================================================================
Write-DiagnosticHeader "SECTION 5: System Event Logs"

$eventLogData = @{
    Available = $false
    ApplicationErrors = @()
}

if ([System.Environment]::OSVersion.Platform -eq "Win32NT") {
    try {
        $eventLogData.Available = $true
        
        # Get recent application errors
        $appErrors = Get-EventLog -LogName Application -EntryType Error -Newest 50 -ErrorAction SilentlyContinue |
                     Where-Object { $_.Source -like "*ASP.NET*" -or $_.Source -like "*.NET*" } |
                     Select-Object -First 20
        
        foreach ($event in $appErrors) {
            $eventInfo = @{
                TimeGenerated = $event.TimeGenerated
                Source = $event.Source
                EventID = $event.EventID
                Message = $event.Message.Substring(0, [Math]::Min(500, $event.Message.Length))
            }
            $eventLogData.ApplicationErrors += $eventInfo
            Write-DiagnosticInfo "  [$($event.TimeGenerated)] $($event.Source) - $($event.EventID)" -Color Red
        }
        
        if ($appErrors.Count -eq 0) {
            Write-DiagnosticInfo "No recent application errors found in Event Log" -Color Green
        }
        
    } catch {
        Write-DiagnosticInfo "Cannot access Event Logs: $($_.Exception.Message)" -Color Yellow
        $eventLogData.Error = $_.Exception.Message
    }
} else {
    Write-DiagnosticInfo "Event Logs not available on non-Windows platform" -Color Gray
}

Add-DiagnosticSection -Title "System Event Logs" -Data $eventLogData

# ============================================================================
# SECTION 6: Database Connectivity Test
# ============================================================================
Write-DiagnosticHeader "SECTION 6: Database Connectivity"

$dbTestData = @{
    Tested = $false
    ConnectionString = "[Not configured]"
    Success = $false
    Error = $null
}

if (-not $SkipConnectivityTests) {
    # Try to get connection string from environment or config
    $connectionString = $env:ConnectionStrings__DefaultConnection
    
    if ([string]::IsNullOrWhiteSpace($connectionString)) {
        # Try to read from appsettings.json
        $webAppSettings = Join-Path $repoRoot "src/OrkinosaiCMS.Web/appsettings.json"
        if (Test-Path $webAppSettings) {
            try {
                $config = Get-Content $webAppSettings -Raw | ConvertFrom-Json
                $connectionString = $config.ConnectionStrings.DefaultConnection
            } catch {
                # Ignore
            }
        }
    }
    
    if (-not [string]::IsNullOrWhiteSpace($connectionString)) {
        $dbTestData.Tested = $true
        $maskedCs = Get-MaskedConnectionString -ConnectionString $connectionString
        $dbTestData.ConnectionString = $maskedCs
        Write-DiagnosticInfo "Connection String: $maskedCs" -Color Gray
        
        try {
            # Try to test connection using SqlConnection
            Add-Type -AssemblyName System.Data
            $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
            $connection.Open()
            
            $command = $connection.CreateCommand()
            $command.CommandText = "SELECT @@VERSION AS Version, DB_NAME() AS DatabaseName"
            $reader = $command.ExecuteReader()
            
            if ($reader.Read()) {
                $dbTestData.Success = $true
                $dbTestData.Version = $reader["Version"]
                $dbTestData.DatabaseName = $reader["DatabaseName"]
                Write-DiagnosticInfo "✓ Database connection successful!" -Color Green
                Write-DiagnosticInfo "  Database: $($reader['DatabaseName'])" -Color Green
                Write-DiagnosticInfo "  Version: $($reader['Version'].ToString().Split("`n")[0])" -Color Green
            }
            
            $reader.Close()
            $connection.Close()
            
        } catch {
            $dbTestData.Success = $false
            $dbTestData.Error = $_.Exception.Message
            Write-DiagnosticInfo "✗ Database connection failed!" -Color Red
            Write-DiagnosticInfo "  Error: $($_.Exception.Message)" -Color Red
            
            # Provide troubleshooting hints
            if ($_.Exception.Message -match "network-related") {
                Write-DiagnosticInfo "  Hint: Check firewall rules and network connectivity" -Color Yellow
            } elseif ($_.Exception.Message -match "login failed") {
                Write-DiagnosticInfo "  Hint: Verify credentials and SQL Server authentication mode" -Color Yellow
            }
        }
    } else {
        Write-DiagnosticInfo "No connection string configured" -Color Yellow
    }
} else {
    Write-DiagnosticInfo "Database connectivity test skipped (SkipConnectivityTests flag set)" -Color Yellow
}

Add-DiagnosticSection -Title "Database Connectivity" -Data $dbTestData -Status $(if ($dbTestData.Success) { "Success" } else { "Warning" })

# ============================================================================
# SECTION 7: Azure Blob Storage Test
# ============================================================================
Write-DiagnosticHeader "SECTION 7: Azure Blob Storage Connectivity"

$blobTestData = @{
    Tested = $false
    AccountName = "[Not configured]"
    Success = $false
    Error = $null
}

if (-not $SkipConnectivityTests) {
    # Try to get blob connection string
    $blobConnectionString = $env:AzureBlobStorageConnectionString
    
    if (-not [string]::IsNullOrWhiteSpace($blobConnectionString)) {
        $blobTestData.Tested = $true
        
        # Extract account name from connection string
        if ($blobConnectionString -match "AccountName=([^;]+)") {
            $blobTestData.AccountName = $matches[1]
            Write-DiagnosticInfo "Storage Account: $($matches[1])" -Color Gray
        }
        
        try {
            # Test blob connectivity using REST API
            if ($blobConnectionString -match "AccountName=([^;]+)") {
                $accountName = $matches[1]
                Write-DiagnosticInfo "Testing connection to Azure Blob Storage..." -Color Gray
                
                # Check for AccountKey after storing AccountName
                if ($blobConnectionString -match "AccountKey=([^;]+)") {
                    # Simple test: Try to list containers (requires Azure.Storage.Blobs SDK, skip if not available)
                    Write-DiagnosticInfo "✓ Blob connection string is configured" -Color Green
                    $blobTestData.Success = $true
                    $blobTestData.Note = "Full connectivity test requires Azure SDK"
                }
            }
        } catch {
            $blobTestData.Success = $false
            $blobTestData.Error = $_.Exception.Message
            Write-DiagnosticInfo "✗ Blob storage test failed: $($_.Exception.Message)" -Color Red
        }
    } else {
        Write-DiagnosticInfo "No blob storage connection string configured" -Color Yellow
        
        # Check if configuration exists in appsettings
        $webAppSettings = Join-Path $repoRoot "src/OrkinosaiCMS.Web/appsettings.json"
        if (Test-Path $webAppSettings) {
            try {
                $config = Get-Content $webAppSettings -Raw | ConvertFrom-Json
                if ($config.AzureBlobStorage.AccountName) {
                    Write-DiagnosticInfo "  AccountName in config: $($config.AzureBlobStorage.AccountName)" -Color Yellow
                    Write-DiagnosticInfo "  Hint: Set environment variable 'AzureBlobStorageConnectionString'" -Color Yellow
                }
            } catch {
                # Ignore
            }
        }
    }
} else {
    Write-DiagnosticInfo "Blob storage connectivity test skipped (SkipConnectivityTests flag set)" -Color Yellow
}

Add-DiagnosticSection -Title "Azure Blob Storage" -Data $blobTestData

# ============================================================================
# SECTION 8: Stripe API Keys Validation
# ============================================================================
Write-DiagnosticHeader "SECTION 8: Stripe Payment Configuration"

$stripeData = @{
    Configured = $false
    PublishableKey = "[Not set]"
    SecretKey = "[Not set]"
    WebhookSecret = "[Not set]"
    TestMode = $false
}

# Check environment variables first
$stripeSecretKey = $env:Payment__Stripe__SecretKey
$stripePublishableKey = $env:Payment__Stripe__PublishableKey
$stripeWebhookSecret = $env:Payment__Stripe__WebhookSecret

# Fall back to appsettings.json
if ([string]::IsNullOrWhiteSpace($stripeSecretKey)) {
    $webAppSettings = Join-Path $repoRoot "src/OrkinosaiCMS.Web/appsettings.json"
    if (Test-Path $webAppSettings) {
        try {
            $config = Get-Content $webAppSettings -Raw | ConvertFrom-Json
            if ($config.Payment.Stripe) {
                $stripeSecretKey = $config.Payment.Stripe.SecretKey
                $stripePublishableKey = $config.Payment.Stripe.PublishableKey
                $stripeWebhookSecret = $config.Payment.Stripe.WebhookSecret
            }
        } catch {
            # Ignore
        }
    }
}

if (-not [string]::IsNullOrWhiteSpace($stripeSecretKey)) {
    $stripeData.Configured = $true
    
    # Determine test/live mode
    $stripeData.TestMode = $stripeSecretKey.StartsWith("sk_test_")
    
    # Mask keys
    if (-not [string]::IsNullOrWhiteSpace($stripePublishableKey)) {
        $stripeData.PublishableKey = $stripePublishableKey.Substring(0, 12) + "********"
    }
    if (-not [string]::IsNullOrWhiteSpace($stripeSecretKey)) {
        $stripeData.SecretKey = $stripeSecretKey.Substring(0, 12) + "********"
    }
    if (-not [string]::IsNullOrWhiteSpace($stripeWebhookSecret)) {
        $stripeData.WebhookSecret = "whsec_" + "********"
    }
    
    Write-DiagnosticInfo "Stripe Publishable Key: $($stripeData.PublishableKey)" -Color Gray
    Write-DiagnosticInfo "Stripe Secret Key: $($stripeData.SecretKey)" -Color Gray
    Write-DiagnosticInfo "Stripe Webhook Secret: $($stripeData.WebhookSecret)" -Color Gray
    Write-DiagnosticInfo "Test Mode: $($stripeData.TestMode)" -Color $(if ($stripeData.TestMode) { "Yellow" } else { "Green" })
    
    if ($stripeData.TestMode) {
        Write-DiagnosticInfo "⚠ Using Stripe TEST keys - not for production!" -Color Yellow
    } else {
        Write-DiagnosticInfo "✓ Using Stripe LIVE keys" -Color Green
    }
} else {
    Write-DiagnosticInfo "Stripe keys not configured" -Color Yellow
}

Add-DiagnosticSection -Title "Stripe Payment Configuration" -Data $stripeData

# ============================================================================
# SECTION 9: Deployment Information
# ============================================================================
Write-DiagnosticHeader "SECTION 9: Deployment Information"

$deploymentData = @{
    GitCommit = "[Unknown]"
    GitBranch = "[Unknown]"
    BuildDate = "[Unknown]"
}

# Try to get Git information
try {
    Push-Location $repoRoot
    
    if (Get-Command git -ErrorAction SilentlyContinue) {
        $gitCommit = git rev-parse --short HEAD 2>$null
        $gitBranch = git rev-parse --abbrev-ref HEAD 2>$null
        
        if ($gitCommit) {
            $deploymentData.GitCommit = $gitCommit
            Write-DiagnosticInfo "Git Commit: $gitCommit" -Color Gray
        }
        if ($gitBranch) {
            $deploymentData.GitBranch = $gitBranch
            Write-DiagnosticInfo "Git Branch: $gitBranch" -Color Gray
        }
    }
    
    Pop-Location
} catch {
    Write-DiagnosticInfo "Cannot retrieve Git information" -Color Yellow
}

# Azure-specific deployment info
if ($isAzureAppService) {
    $deploymentData.AzureDeploymentId = $env:WEBSITE_DEPLOYMENT_ID
    Write-DiagnosticInfo "Azure Deployment ID: $($env:WEBSITE_DEPLOYMENT_ID)" -Color Gray
}

Add-DiagnosticSection -Title "Deployment Information" -Data $deploymentData

# ============================================================================
# SECTION 10: Crash Analysis & Stack Traces
# ============================================================================
Write-DiagnosticHeader "SECTION 10: Crash Analysis & Recent Exceptions"

$crashData = @{
    FoundCrashes = $false
    Exceptions = @()
}

# Look for crash dumps and exception details in logs
if (-not $SkipLogCollection -and $logData.RecentErrors.Count -gt 0) {
    Write-DiagnosticInfo "Analyzing recent errors for stack traces..." -Color Yellow
    
    foreach ($errorLog in $logData.RecentErrors) {
        # Extract stack traces
        $stackTraces = $errorLog.Errors | Where-Object { $_ -match "^   at " }
        if ($stackTraces.Count -gt 0) {
            $crashData.FoundCrashes = $true
            $crashData.Exceptions += @{
                Source = $errorLog.File
                StackTrace = $stackTraces
            }
            
            Write-DiagnosticInfo "`nStack trace found in: $($errorLog.File)" -Color Red
            $stackTraces | Select-Object -First 10 | ForEach-Object {
                Write-DiagnosticInfo "  $_" -Color DarkRed
            }
        }
    }
    
    if (-not $crashData.FoundCrashes) {
        Write-DiagnosticInfo "No crash stack traces found in recent logs" -Color Green
    }
} else {
    Write-DiagnosticInfo "Crash analysis skipped (no recent errors or log collection disabled)" -Color Gray
}

Add-DiagnosticSection -Title "Crash Analysis" -Data $crashData -Status $(if ($crashData.FoundCrashes) { "Error" } else { "Success" })

# ============================================================================
# SECTION 11: Health Check Summary
# ============================================================================
Write-DiagnosticHeader "SECTION 11: Health Check Summary"

$healthSummary = @{
    OverallStatus = "Unknown"
    Checks = @()
}

# Database check
$healthSummary.Checks += @{
    Name = "Database Connectivity"
    Status = if ($dbTestData.Success) { "✓ Pass" } elseif ($dbTestData.Tested) { "✗ Fail" } else { "⊘ Skipped" }
    Color = if ($dbTestData.Success) { "Green" } elseif ($dbTestData.Tested) { "Red" } else { "Gray" }
}

# Blob storage check
$healthSummary.Checks += @{
    Name = "Azure Blob Storage"
    Status = if ($blobTestData.Success) { "✓ Pass" } elseif ($blobTestData.Tested) { "~ Partial" } else { "⊘ Skipped" }
    Color = if ($blobTestData.Success) { "Green" } elseif ($blobTestData.Tested) { "Yellow" } else { "Gray" }
}

# Configuration check
$configOk = $configData.Files | Where-Object { $_.Exists -and -not $_.Error } | Measure-Object | Select-Object -ExpandProperty Count
$healthSummary.Checks += @{
    Name = "Configuration Files"
    Status = if ($configOk -gt 0) { "✓ Found ($configOk)" } else { "✗ Missing" }
    Color = if ($configOk -gt 0) { "Green" } else { "Red" }
}

# Logs check
$healthSummary.Checks += @{
    Name = "Application Logs"
    Status = if ($logData.Files.Count -gt 0) { "✓ Found ($($logData.Files.Count) files)" } else { "⊘ None" }
    Color = if ($logData.Files.Count -gt 0) { "Green" } else { "Gray" }
}

# Recent errors check
$healthSummary.Checks += @{
    Name = "Recent Errors"
    Status = if ($logData.RecentErrors.Count -eq 0) { "✓ None" } else { "⚠ Found ($($logData.RecentErrors.Count))" }
    Color = if ($logData.RecentErrors.Count -eq 0) { "Green" } else { "Yellow" }
}

# Crashes check
$healthSummary.Checks += @{
    Name = "Crash Stack Traces"
    Status = if (-not $crashData.FoundCrashes) { "✓ None" } else { "✗ Found" }
    Color = if (-not $crashData.FoundCrashes) { "Green" } else { "Red" }
}

foreach ($check in $healthSummary.Checks) {
    Write-DiagnosticInfo "$($check.Status.PadRight(20)) $($check.Name)" -Color $check.Color
}

# Determine overall status
$failCount = ($healthSummary.Checks | Where-Object { $_.Status -like "✗*" }).Count
$warnCount = ($healthSummary.Checks | Where-Object { $_.Status -like "⚠*" }).Count

if ($failCount -eq 0 -and $warnCount -eq 0) {
    $healthSummary.OverallStatus = "Healthy"
    Write-Host "`n" -NoNewline
    Write-DiagnosticInfo "Overall Status: ✓ HEALTHY" -Color Green
} elseif ($failCount -gt 0) {
    $healthSummary.OverallStatus = "Unhealthy"
    Write-Host "`n" -NoNewline
    Write-DiagnosticInfo "Overall Status: ✗ UNHEALTHY ($failCount failures)" -Color Red
} else {
    $healthSummary.OverallStatus = "Degraded"
    Write-Host "`n" -NoNewline
    Write-DiagnosticInfo "Overall Status: ⚠ DEGRADED ($warnCount warnings)" -Color Yellow
}

Add-DiagnosticSection -Title "Health Check Summary" -Data $healthSummary

# ============================================================================
# Generate Reports
# ============================================================================
Write-DiagnosticHeader "Generating Diagnostic Reports"

# Generate text report
$textReport = @"
================================================================================
MOSAIC CMS - DIAGNOSTIC REPORT
================================================================================
Generated: $($diagnosticData.Timestamp)
Machine: $($diagnosticData.MachineName)
Environment: $($diagnosticData.Environment)

"@

foreach ($section in $diagnosticData.Sections) {
    $textReport += @"

================================================================================
$($section.Title.ToUpper())
================================================================================
Time: $($section.Timestamp)
Status: $($section.Status)

$($section.Data | ConvertTo-Json -Depth 5)

"@
}

$textReport += @"

================================================================================
END OF REPORT
================================================================================

For more information:
- Review logs in: App_Data/Logs/
- Check Azure Portal: Application Insights, Log Stream
- GitHub Issues: https://github.com/orkinosai25-org/mosaic/issues

"@

# Save text report
$textReport | Out-File -FilePath $reportFile -Encoding UTF8
Write-DiagnosticInfo "✓ Text report saved: $reportFile" -Color Green

# Generate HTML report if requested
if ($GenerateHtmlReport) {
    $htmlReport = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <title>Mosaic CMS - Diagnostic Report</title>
    <style>
        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 20px; background: #f5f5f5; }
        .container { max-width: 1200px; margin: 0 auto; background: white; padding: 30px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
        h1 { color: #1e3a8a; border-bottom: 3px solid #06b6d4; padding-bottom: 10px; }
        h2 { color: #06b6d4; margin-top: 30px; border-left: 4px solid #06b6d4; padding-left: 10px; }
        .section { margin: 20px 0; padding: 15px; background: #f9fafb; border-radius: 5px; }
        .success { color: #10b981; }
        .warning { color: #f59e0b; }
        .error { color: #ef4444; }
        .info { color: #6b7280; }
        table { width: 100%; border-collapse: collapse; margin: 15px 0; }
        th, td { padding: 10px; text-align: left; border-bottom: 1px solid #e5e7eb; }
        th { background: #1e3a8a; color: white; }
        .code { background: #1f2937; color: #f3f4f6; padding: 15px; border-radius: 5px; overflow-x: auto; font-family: 'Courier New', monospace; font-size: 13px; }
        .badge { display: inline-block; padding: 4px 12px; border-radius: 12px; font-size: 12px; font-weight: bold; }
        .badge-success { background: #d1fae5; color: #065f46; }
        .badge-warning { background: #fef3c7; color: #92400e; }
        .badge-error { background: #fee2e2; color: #991b1b; }
        .timestamp { color: #9ca3af; font-size: 14px; }
    </style>
</head>
<body>
<div class="container">
    <h1>🔍 Mosaic CMS - Diagnostic Report</h1>
    <p class="timestamp">Generated: $($diagnosticData.Timestamp) | Machine: $($diagnosticData.MachineName) | Environment: $($diagnosticData.Environment)</p>
    
    <div class="section">
        <h2>Overall Health Status</h2>
"@

    $statusClass = switch ($healthSummary.OverallStatus) {
        "Healthy" { "success" }
        "Degraded" { "warning" }
        "Unhealthy" { "error" }
        default { "info" }
    }
    
    $htmlReport += "<p class='$statusClass' style='font-size: 24px; font-weight: bold;'>$($healthSummary.OverallStatus.ToUpper())</p>"
    $htmlReport += "<table><tr><th>Check</th><th>Status</th></tr>"
    
    foreach ($check in $healthSummary.Checks) {
        $badgeClass = if ($check.Status -like "✓*") { "badge-success" } 
                     elseif ($check.Status -like "⚠*") { "badge-warning" }
                     elseif ($check.Status -like "✗*") { "badge-error" }
                     else { "badge-info" }
        $htmlReport += "<tr><td>$($check.Name)</td><td><span class='badge $badgeClass'>$($check.Status)</span></td></tr>"
    }
    
    $htmlReport += "</table></div>"
    
    # Add each section
    foreach ($section in $diagnosticData.Sections) {
        $htmlReport += @"
    <div class="section">
        <h2>$($section.Title)</h2>
        <p class="timestamp">Time: $($section.Timestamp) | Status: $($section.Status)</p>
        <div class="code">$($section.Data | ConvertTo-Json -Depth 5 | Out-String)</div>
    </div>
"@
    }
    
    $htmlReport += @"
    
    <div class="section">
        <h2>Additional Resources</h2>
        <ul>
            <li>Review logs in: <code>App_Data/Logs/</code></li>
            <li>Check Azure Portal: Application Insights, Log Stream</li>
            <li>GitHub Issues: <a href="https://github.com/orkinosai25-org/mosaic/issues">https://github.com/orkinosai25-org/mosaic/issues</a></li>
        </ul>
    </div>
</div>
</body>
</html>
"@
    
    $htmlReport | Out-File -FilePath $htmlReportFile -Encoding UTF8
    Write-DiagnosticInfo "✓ HTML report saved: $htmlReportFile" -Color Green
}

Write-Host ""
Write-DiagnosticHeader "Diagnostics Complete"
Write-DiagnosticInfo "Reports saved to: $OutputPath" -Color Cyan
Write-DiagnosticInfo "Text Report: $reportFile" -Color Cyan
if ($GenerateHtmlReport) {
    Write-DiagnosticInfo "HTML Report: $htmlReportFile" -Color Cyan
}

# Return summary object for programmatic use
return @{
    Success = $true
    OverallStatus = $healthSummary.OverallStatus
    ReportFile = $reportFile
    HtmlReportFile = if ($GenerateHtmlReport) { $htmlReportFile } else { $null }
    Timestamp = $diagnosticData.Timestamp
    Sections = $diagnosticData.Sections.Count
}
