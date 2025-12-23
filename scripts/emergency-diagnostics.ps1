#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Emergency diagnostic runner - Use when app is broken and you need answers NOW

.DESCRIPTION
    This script provides a simplified interface for running diagnostics in emergency situations.
    It runs the full diagnostics.ps1 script with optimized settings for immediate troubleshooting.

.EXAMPLE
    pwsh emergency-diagnostics.ps1
    Run emergency diagnostics with default settings

.EXAMPLE
    pwsh emergency-diagnostics.ps1 -OpenReport
    Run diagnostics and automatically open the HTML report

.NOTES
    For detailed usage, see QUICK-START-DIAGNOSTICS.md
#>

param(
    [switch]$OpenReport,
    [switch]$SkipTests
)

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "   🚨 EMERGENCY DIAGNOSTICS - MOSAIC CMS 🚨" -ForegroundColor Red
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "This will collect comprehensive diagnostics to help identify" -ForegroundColor Yellow
Write-Host "why your application is broken." -ForegroundColor Yellow
Write-Host ""

# Verify we're in the right location
$scriptPath = $PSScriptRoot
if (-not $scriptPath) {
    $scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
}

$diagnosticsScript = Join-Path $scriptPath "diagnostics.ps1"
if (-not (Test-Path $diagnosticsScript)) {
    Write-Host "❌ ERROR: Cannot find diagnostics.ps1" -ForegroundColor Red
    Write-Host "   Make sure you're running this from the scripts/ directory" -ForegroundColor Yellow
    exit 1
}

Write-Host "▶ Starting diagnostic collection..." -ForegroundColor Cyan
Write-Host ""

# Run diagnostics with optimal settings for troubleshooting
$params = @{
    GenerateHtmlReport = $true
}

if ($SkipTests) {
    $params.SkipConnectivityTests = $true
    Write-Host "⚠️  Skipping connectivity tests (app may be completely down)" -ForegroundColor Yellow
} else {
    Write-Host "ℹ️  Running connectivity tests (will test DB, Blob, etc.)" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "───────────────────────────────────────────────────────" -ForegroundColor Gray
Write-Host ""

try {
    # Run the diagnostics script
    $result = & $diagnosticsScript @params
    
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Green
    Write-Host "   ✅ DIAGNOSTICS COMPLETE" -ForegroundColor Green
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Green
    Write-Host ""
    
    # Show status
    $statusColor = switch ($result.OverallStatus) {
        "Healthy" { "Green" }
        "Degraded" { "Yellow" }
        "Unhealthy" { "Red" }
        default { "White" }
    }
    
    Write-Host "Overall Status: " -NoNewline
    Write-Host $result.OverallStatus.ToUpper() -ForegroundColor $statusColor
    Write-Host ""
    
    # Show recommendations count
    $criticalCount = if ($result.Recommendations -and $result.Recommendations.Critical) { 
        $result.Recommendations.Critical.Count 
    } else { 0 }
    $importantCount = if ($result.Recommendations -and $result.Recommendations.Important) { 
        $result.Recommendations.Important.Count 
    } else { 0 }
    
    if ($criticalCount -gt 0) {
        Write-Host "🔴 CRITICAL ISSUES: $criticalCount" -ForegroundColor Red
        Write-Host "   These require IMMEDIATE attention!" -ForegroundColor Red
        Write-Host ""
    }
    
    if ($importantCount -gt 0) {
        Write-Host "🟡 IMPORTANT ISSUES: $importantCount" -ForegroundColor Yellow
        Write-Host "   These should be addressed soon" -ForegroundColor Yellow
        Write-Host ""
    }
    
    if ($criticalCount -eq 0 -and $importantCount -eq 0) {
        Write-Host "✅ No critical or important issues found" -ForegroundColor Green
        Write-Host "   Check the report for suggestions and details" -ForegroundColor Gray
        Write-Host ""
    }
    
    # Show report locations
    Write-Host "📄 Reports Generated:" -ForegroundColor Cyan
    Write-Host ""
    
    if ($result.HtmlReportFile) {
        Write-Host "   HTML: " -NoNewline -ForegroundColor Gray
        Write-Host $result.HtmlReportFile -ForegroundColor White
    }
    
    Write-Host "   Text: " -NoNewline -ForegroundColor Gray
    Write-Host $result.ReportFile -ForegroundColor White
    Write-Host ""
    
    # Quick actions
    Write-Host "📖 Quick Actions:" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "   1. Read the recommendations in the console output above" -ForegroundColor White
    Write-Host "   2. Open the HTML report for detailed analysis" -ForegroundColor White
    Write-Host "   3. Check QUICK-START-DIAGNOSTICS.md for common fixes" -ForegroundColor White
    Write-Host "   4. Share the report with your team if needed" -ForegroundColor White
    Write-Host ""
    
    # Open report if requested
    if ($OpenReport -and $result.HtmlReportFile) {
        Write-Host "▶ Opening HTML report..." -ForegroundColor Cyan
        
        if ($IsWindows -or $env:OS -match "Windows") {
            Start-Process $result.HtmlReportFile
        } elseif ($IsMacOS) {
            & open $result.HtmlReportFile
        } elseif ($IsLinux) {
            try {
                & xdg-open $result.HtmlReportFile -ErrorAction Stop
            } catch {
                Write-Host "   ℹ️  Cannot auto-open on this system. Please open manually:" -ForegroundColor Yellow
                Write-Host "      $($result.HtmlReportFile)" -ForegroundColor Gray
            }
        }
    } elseif (-not $OpenReport) {
        Write-Host "💡 TIP: Use -OpenReport flag to automatically open the HTML report" -ForegroundColor DarkGray
        Write-Host ""
    }
    
    # Show next steps based on status
    if ($result.OverallStatus -eq "Unhealthy") {
        Write-Host "⚠️  URGENT: Your application is UNHEALTHY" -ForegroundColor Red
        Write-Host "   Review critical issues above and take immediate action" -ForegroundColor Red
        Write-Host ""
        exit 1
    } elseif ($result.OverallStatus -eq "Degraded") {
        Write-Host "⚠️  WARNING: Your application is DEGRADED" -ForegroundColor Yellow
        Write-Host "   Address important issues to restore full functionality" -ForegroundColor Yellow
        Write-Host ""
        exit 2
    } else {
        Write-Host "✅ Your application appears healthy" -ForegroundColor Green
        Write-Host "   Review suggestions for optimization opportunities" -ForegroundColor Gray
        Write-Host ""
        exit 0
    }
    
} catch {
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Red
    Write-Host "   ❌ DIAGNOSTICS FAILED" -ForegroundColor Red
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Red
    Write-Host ""
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Troubleshooting:" -ForegroundColor Yellow
    Write-Host "  1. Ensure PowerShell 7+ is installed (run: pwsh --version)" -ForegroundColor Gray
    Write-Host "  2. Check you're in the scripts/ directory" -ForegroundColor Gray
    Write-Host "  3. Verify diagnostics.ps1 exists and is readable" -ForegroundColor Gray
    Write-Host "  4. Try running diagnostics.ps1 directly:" -ForegroundColor Gray
    Write-Host "     pwsh diagnostics.ps1 -GenerateHtmlReport" -ForegroundColor White
    Write-Host ""
    exit 99
}
