# GitHub Copilot Agent Troubleshooting Guide

## Issue Summary

This document addresses the failures observed in GitHub Copilot agent workflow run [#19788868587](https://github.com/orkinosai25-org/orkinosaiCMS/actions/runs/19788868587/job/56699122872).

## 🚀 Quick Fix: Automated Tools

**NEW: We've created automated scripts to fix all common issues!**

### One-Command Fix (Recommended)
```bash
# Run this to automatically fix all issues
./scripts/copilot-agent-helper.sh fix-all
```

This will:
- ✅ Auto-detect base branch (handles main, master, develop)
- ✅ Work with shallow clones and grafted repositories
- ✅ Generate PR summaries with retry logic
- ✅ Provide comprehensive diagnostics

### Available Scripts

| Script | Purpose | Command |
|--------|---------|---------|
| **copilot-agent-helper.sh** | Main orchestration tool | `./scripts/copilot-agent-helper.sh fix-all` |
| **detect-base-branch.sh** | Auto-detect base branch | `./scripts/detect-base-branch.sh` |
| **generate-pr-summary.sh** | Generate PR descriptions | `./scripts/generate-pr-summary.sh` |
| **fix-base-branch.sh** | Verify branch access | `./scripts/fix-base-branch.sh` |

### Quick Diagnostics
```bash
# Check repository state and identify issues
./scripts/copilot-agent-helper.sh diagnose
```

For detailed manual fixes and technical background, see the sections below.

---

## Root Causes Identified

### 1. Base Branch Ambiguity (main not found)

**Error Message:**
```
fatal: ambiguous argument 'refs/heads/main': unknown revision or path not in the working tree.
Use '--' to separate paths from revisions, like this:
'git <command> [<revision>...] -- [<file>...]'
```

**Root Cause:**
The Copilot agent workflow performs a shallow clone of the repository and checks out the feature branch (`copilot/implement-branded-blazor-website`). When it attempts to generate a PR summary by running `git diff refs/heads/main HEAD`, the `main` branch doesn't exist locally in the shallow clone.

**Why This Happens:**
- The workflow cloned with `--depth=1` or similar shallow clone options
- Only the feature branch was checked out
- The base branch (`main`) was not fetched or checked out locally
- The workflow tried to reference `refs/heads/main` which doesn't exist as a local branch

**Resolution Strategies:**

### **NEW: Automated Resolution (Recommended)**

**Use the automated helper script to fix all issues:**
```bash
# Fix all issues automatically
./scripts/copilot-agent-helper.sh fix-all

# Or run individual commands
./scripts/copilot-agent-helper.sh detect-branch    # Auto-detect base branch
./scripts/copilot-agent-helper.sh diagnose         # Check repository state
```

**How it works:**
- Automatically detects base branch (main, master, develop, etc.)
- Handles shallow clones and missing branch references
- Uses grafted parent as comparison point when needed
- Provides clear diagnostics and error messages

### **Manual Resolution (Alternative)**

1. **Ensure Base Branch is Fetched:**
   ```bash
   git fetch origin main:refs/remotes/origin/main
   ```

2. **Use Remote Reference:**
   Instead of `refs/heads/main`, use `refs/remotes/origin/main` or `origin/main` when the base branch hasn't been checked out locally.

3. **Fetch Before Comparison:**
   The workflow should fetch the base branch before attempting any diff operations:
   ```bash
   git fetch origin ${BASE_REF}
   git diff origin/${BASE_REF}...HEAD
   ```

4. **Unshallow the Clone:**
   If full history is needed:
   ```bash
   git fetch --unshallow
   ```

### 2. PR Summary Fetch Errors

**Error Message:**
```
Invalid response from PR description request: {}. Retrying attempt 1 of 3
Invalid response from PR description request: {}. Retrying attempt 2 of 3
Invalid response from PR description request: {}. Retrying attempt 3 of 3
Failed to get a valid PR summary after 3 attempts.
```

**Root Cause:**
The agent's response for generating the PR description was malformed or empty, returning only `{}` instead of the expected template with proper structure.

**Why This Happens:**
- The agent may have encountered issues parsing repository content
- The base branch comparison failure (issue #1) may have cascaded into the PR summary generation
- Model output was corrupted or truncated
- The agent response didn't contain expected `template_path` and `template_content` tags

**Observable Symptoms:**
```
copilot: 16�� 1016 3110_—s017019_
(xc� ³16 sett...)000 .0_{=;
```

The agent output contained garbled/corrupted text indicating potential encoding or model output issues.

**Resolution Strategies:**

### **NEW: Automated Resolution (Recommended)**

**Use the automated PR summary generator:**
```bash
# Generate PR summary with automatic retry and fallback
./scripts/generate-pr-summary.sh

# Or use the complete fix tool
./scripts/copilot-agent-helper.sh generate-summary
./scripts/copilot-agent-helper.sh fix-all  # Fix everything at once
```

**Features:**
- Auto-detects base branch
- Retries with exponential backoff (configurable, default 3 attempts)
- Falls back to basic git-based summary on failure
- Validates generated summaries
- Handles shallow clones and missing branches

**Configuration:**
```bash
# Customize behavior with environment variables
MAX_ATTEMPTS=5 OUTPUT_FILE=my-pr.md ./scripts/generate-pr-summary.sh
```

### **Manual Resolution (Alternative)**

1. **Fix Upstream Dependencies:**
   Ensure the base branch issue is resolved first, as this may be a cascading failure.

2. **Validate Repository State:**
   Ensure the repository is in a valid state before invoking the agent:
   - All required branches are accessible
   - No partial/corrupted git state
   - Required files are present

3. **Agent Configuration:**
   - Verify the agent prompt doesn't contain special characters that could cause parsing issues
   - Ensure proper model selection and configuration
   - Check for any rate limiting or API issues

4. **Retry Logic:**
   The workflow already implements retry logic, but may need:
   - Exponential backoff between retries
   - More detailed error logging
   - Ability to recover from partial failures

### 3. Blank Agent Assignment

**Root Cause:**
The problem statement mentions "blank agent assignment," which could refer to:
- The agent not being properly assigned to the task
- The agent response being empty or malformed
- The custom agent configuration not being loaded correctly

**From Logs:**
```
Proceeding without custom agent.
Using custom agent "Default" for the task.
```

This suggests the agent was using a default configuration rather than a specific custom agent that may have been intended.

**Resolution Strategies:**

1. **Verify Agent Configuration:**
   Ensure the repository has proper agent configuration if custom agents are needed.

2. **Check Agent Availability:**
   Verify that the intended agent is available and properly configured in the GitHub Copilot settings.

3. **Fallback Handling:**
   Ensure the default agent can handle the task even when custom agents aren't available.

## Preventive Measures

### For Repository Owners

1. **Add Git Configuration:**
   Consider adding a `.github/workflows/` helper script or documentation for common git operations that handle shallow clones properly.

2. **Document Branch Structure:**
   Maintain clear documentation about which branches are the canonical base branches (typically `main` or `develop`).

3. **Test Workflow Locally:**
   When possible, test Copilot workflows in a similar environment to catch issues early.

### For Workflow Authors

1. **Always Fetch Base Branch:**
   ```yaml
   - name: Fetch base branch
     run: |
       git fetch origin ${{ github.base_ref }}:refs/remotes/origin/${{ github.base_ref }} || true
   ```

2. **Use Remote References:**
   When comparing branches, use `origin/main` instead of `main`:
   ```bash
   git diff origin/main...HEAD
   ```

3. **Handle Missing Branches Gracefully:**
   ```bash
   if git rev-parse origin/main >/dev/null 2>&1; then
     git diff origin/main...HEAD
   else
     echo "Base branch not found, skipping diff"
   fi
   ```

4. **Add Diagnostic Output:**
   ```bash
   echo "Available refs:"
   git show-ref
   echo "Current branch:"
   git branch -v
   ```

## Testing the Fixes

To verify these issues are resolved:

1. **Test Base Branch Resolution:**
   ```bash
   # Simulate shallow clone
   git clone --depth=1 --branch=feature-branch <repo-url>
   cd <repo>
   
   # Try the fix
   git fetch origin main:refs/remotes/origin/main
   git diff origin/main...HEAD
   ```

2. **Test PR Summary Generation:**
   - Ensure the repository is in a valid state
   - Verify all required branches are accessible
   - Check that the agent can successfully generate a PR description

3. **Monitor Workflow Runs:**
   - Watch for similar failures in future runs
   - Check logs for the specific error patterns documented above
   - Verify the retry logic succeeds after implementing fixes

## Related Issues

- GitHub Actions run: [#19788868587](https://github.com/orkinosai25-org/orkinosaiCMS/actions/runs/19788868587/job/56699122872)
- Pull Request: [#5](https://github.com/orkinosai25-org/orkinosaiCMS/pull/5)
- Original task: Implement complete branded Blazor website suite

## Conclusion

The failures were primarily caused by the shallow clone not having access to the base branch (`main`), which cascaded into PR summary generation failures. The fixes require:

1. Ensuring the base branch is fetched before any comparison operations
2. Using remote references (`origin/main`) instead of local references (`main`)
3. Adding proper error handling for missing branches
4. Validating agent responses before considering them successful

These issues are not specific to this repository but are common patterns in CI/CD workflows that need to be handled robustly.
