# OrkinosaiCMS Utility Scripts

This directory contains utility scripts for common development and troubleshooting tasks.

## Available Scripts

### diagnostics.ps1 (PowerShell - Recommended for Troubleshooting)

**Purpose:** Comprehensive diagnostic tool to collect application health information, logs, configuration, and test connectivity. Designed to help platform engineers and Copilot Agent rapidly diagnose issues even when the main application is non-functional.

**Usage:**
```powershell
# Run full diagnostics with HTML report
./scripts/diagnostics.ps1 -GenerateHtmlReport

# Skip connectivity tests (faster, use when external services unavailable)
./scripts/diagnostics.ps1 -SkipConnectivityTests

# Custom output location
./scripts/diagnostics.ps1 -OutputPath "C:\temp\diag" -GenerateHtmlReport

# Skip log collection (use when logs are too large)
./scripts/diagnostics.ps1 -SkipLogCollection

# View help
./scripts/diagnostics.ps1 -Help
Get-Help ./scripts/diagnostics.ps1 -Full
```

**What it collects:**
1. System information (OS, runtime, Azure/GitHub environment detection)
2. Application settings from all appsettings.json files (with sensitive data masking)
3. Environment variables (with sensitive data protection)
4. Application logs from multiple locations (LogFiles/, App_Data/Logs/)
5. Windows Event Logs (ASP.NET errors)
6. Database connectivity test results
7. Azure Blob Storage configuration validation
8. Stripe payment configuration check
9. Git deployment information (commit hash, branch)
10. Crash analysis with stack traces from recent errors
11. Comprehensive health check summary

**Output:**
- Text report: `diagnostic-output/diagnostic-report-YYYYMMDD-HHMMSS.txt`
- HTML report: `diagnostic-output/diagnostic-report-YYYYMMDD-HHMMSS.html` (optional)

**When to use:**
- When the application is failing to start or crashing
- Before contacting support or filing a bug report
- To collect evidence for troubleshooting HTTP 500/503 errors
- As part of scheduled health checks
- In Azure App Service Kudu console for production diagnostics
- In GitHub Actions for CI/CD diagnostics

**Special features:**
- Automatically masks sensitive data (passwords, keys, secrets)
- Works in Azure App Service (Kudu), GitHub Actions, and local environments
- Detects environment and adjusts checks accordingly
- Color-coded output and HTML reports
- Can be extended with new checks as issues are discovered

**📖 Full documentation:** See [README-DIAGNOSTICS.md](./README-DIAGNOSTICS.md)

**🔗 GitHub Actions Integration:** Trigger via workflow: `.github/workflows/run-diagnostics.yml`

---

### copilot-agent-helper.sh (Recommended)

**Purpose:** Main helper script that orchestrates all Copilot agent fixes, including base branch detection and PR summary generation.

**Usage:**
```bash
# Run all fixes (recommended for most use cases)
./scripts/copilot-agent-helper.sh fix-all

# Run diagnostics to check repository state
./scripts/copilot-agent-helper.sh diagnose

# Only detect base branch
./scripts/copilot-agent-helper.sh detect-branch

# Only generate PR summary
./scripts/copilot-agent-helper.sh generate-summary

# Show help
./scripts/copilot-agent-helper.sh help
```

**What it does:**
1. Automatically detects the base branch (main, master, develop, etc.)
2. Verifies branch accessibility and fetches if needed
3. Generates PR summaries with automatic fallback mechanisms
4. Provides comprehensive diagnostics for troubleshooting
5. Handles all common Copilot agent failure scenarios

**When to use:**
- **ALWAYS run before completing PR work** to prevent agent failures
- When you encounter "branch not found" errors
- When PR summary generation fails
- Before pushing changes to verify repository state
- As part of CI/CD pre-flight checks

### detect-base-branch.sh

**Purpose:** Automatically detect the repository's base branch without manual specification.

**Usage:**
```bash
# Auto-detect base branch
./scripts/detect-base-branch.sh

# Export detected branch to file for sourcing
./scripts/detect-base-branch.sh origin --export-file

# Use custom remote
./scripts/detect-base-branch.sh upstream
```

**What it does:**
1. Checks for common base branches (main, master, develop)
2. Uses GitHub CLI to detect default branch if available
3. Fetches missing branches automatically
4. Exports BASE_BRANCH and BASE_REF for use in other scripts
5. Provides clear error messages with troubleshooting steps

**When to use:**
- When working in a new repository
- When unsure which base branch to use
- Before running git diff or log commands
- As part of automated workflows

### generate-pr-summary.sh

**Purpose:** Generate PR descriptions with automatic retry and fallback mechanisms.

**Usage:**
```bash
# Generate with auto-detected base branch
./scripts/generate-pr-summary.sh

# Specify base branch
BASE_BRANCH=develop ./scripts/generate-pr-summary.sh

# Customize output file and retries
BASE_BRANCH=main OUTPUT_FILE=my-pr.md MAX_ATTEMPTS=5 ./scripts/generate-pr-summary.sh
```

**What it does:**
1. Auto-detects base branch if not specified
2. Attempts AI-assisted summary generation (if GitHub CLI available)
3. Falls back to basic git-based summary on failure
4. Retries with exponential backoff
5. Validates generated summaries
6. Provides preview of generated content

**When to use:**
- When PR description generation fails in Copilot agent
- Before creating or updating a pull request
- To get a quick overview of changes
- When you need a template for manual PR description

### fix-base-branch.sh

**Purpose:** Diagnose and fix the "base branch not found" error that commonly occurs when working with Copilot agent workflows in shallow git clones.

**Usage:**
```bash
# Use default settings (main branch, origin remote)
./scripts/fix-base-branch.sh

# Specify a different base branch
./scripts/fix-base-branch.sh develop

# Specify both base branch and remote
./scripts/fix-base-branch.sh main upstream
```

**What it does:**
1. Checks if the base branch is accessible locally
2. Fetches the base branch from remote if needed
3. Verifies you can compare your branch with the base branch
4. Shows commit count and changes
5. Performs repository health checks
6. Provides summary and next steps

**When to use:**
- After cloning with `--depth=1` or shallow clone
- When you get "ambiguous argument 'refs/heads/main'" errors
- Before creating a pull request to verify branch state
- When troubleshooting Copilot agent failures
- To check if your branch is up to date with the base branch

**Example output:**
```
=== Copilot Agent Base Branch Fix Tool ===

This script will:
  1. Check if the base branch (main) is accessible
  2. Fetch it if needed
  3. Verify the repository state

✓ In a git repository
Current branch: copilot/my-feature

Checking for base branch...
✓ Base branch origin/main is accessible
  SHA: db3f263b95a2c2d08d48e16ae6a5cfdbb5631785

Testing comparison with base branch...
✓ Can successfully compare with base branch
  Changes:  4 files changed, 716 insertions(+)

Commits ahead of base branch:
✓ 2 commit(s) ahead
5af1a24 Add comprehensive Copilot agent troubleshooting documentation
99f2ca4 Initial plan

Finding merge base...
✓ Merge base found: db3f263b
✓ Branch is up to date with origin/main

Repository health check...
✓ Working directory is clean
✓ Repository integrity check passed

=== Summary ===
Base branch: origin/main ✓
Current branch: copilot/my-feature
Commits ahead: 2

You can now:
  • Compare changes: git diff origin/main...HEAD
  • View commits: git log origin/main..HEAD
  • Create PR: Push your branch and open a pull request

All checks passed!
```

**Troubleshooting:**

If the script fails to fetch the base branch:
1. Check your internet connection
2. Verify the branch name is correct (default is 'main', some repos use 'master' or 'develop')
3. Ensure you have access to the repository
4. Try: `git remote -v` to see your remote configuration

If you see integrity warnings:
- Run `git fsck` for detailed diagnostics
- Consider re-cloning the repository if issues persist

## Adding New Scripts

When adding a new script to this directory:

1. **Make it executable:**
   ```bash
   chmod +x scripts/your-script.sh
   ```

2. **Add proper header:**
   ```bash
   #!/bin/bash
   # script-name.sh - Brief description
   # Detailed explanation of what the script does
   ```

3. **Include usage instructions:**
   - Add comments explaining parameters
   - Include example usage
   - Show expected output

4. **Update this README:**
   - Add your script to the "Available Scripts" section
   - Document purpose, usage, and when to use it
   - Include example output

5. **Use consistent style:**
   - Color output for better UX (see fix-base-branch.sh for example)
   - Include error handling with `set -e`
   - Provide helpful error messages
   - Show progress and status

6. **Test thoroughly:**
   - Test on a clean clone
   - Test error conditions
   - Verify output is helpful

## Script Development Best Practices

- **Idempotent:** Scripts should be safe to run multiple times
- **Defensive:** Check prerequisites before proceeding
- **Informative:** Explain what's happening and why
- **Helpful:** Provide next steps and troubleshooting tips
- **Safe:** Use `set -e` to exit on errors
- **Portable:** Use POSIX-compliant commands when possible

## Common Script Patterns

### Checking git repository
```bash
if ! git rev-parse --git-dir > /dev/null 2>&1; then
    echo "Error: Not in a git repository!"
    exit 1
fi
```

### Colored output
```bash
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}Success!${NC}"
echo -e "${RED}Error!${NC}"
echo -e "${YELLOW}Warning!${NC}"
```

### Function for status messages
```bash
print_status() {
    echo -e "${GREEN}✓${NC} $1"
}

print_error() {
    echo -e "${RED}✗${NC} $1"
}
```

## Testing Scripts

Before committing scripts:

1. **Test in clean environment:**
   ```bash
   # In a temporary directory
   git clone --depth=1 <repo-url>
   cd <repo>
   ./scripts/your-script.sh
   ```

2. **Test error conditions:**
   - Test with wrong parameters
   - Test when git commands fail
   - Test when prerequisites are missing

3. **Verify output:**
   - Check that colors display correctly
   - Ensure error messages are helpful
   - Confirm success messages are clear

4. **Check portability:**
   - Test on different shells (bash, zsh)
   - Verify on different OS (if applicable)

## Support

If you encounter issues with any script:
1. Check the script's documentation above
2. Review the [QUICK_FIX_GUIDE.md](../docs/QUICK_FIX_GUIDE.md)
3. File an issue with:
   - Script name and version
   - Full error output
   - Steps to reproduce
   - Your environment (OS, Git version, etc.)

---

*Last updated: 2025-11-29*
