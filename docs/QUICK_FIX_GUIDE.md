# Quick Fix Guide for Copilot Agent Failures

## 🚀 NEW: Automated Fix Tool

**The fastest way to fix all common Copilot agent issues:**

```bash
# Run this ONE command to fix everything automatically
./scripts/copilot-agent-helper.sh fix-all
```

This automated tool will:
- ✅ Auto-detect the base branch (main, master, develop, etc.)
- ✅ Handle shallow clones and missing branches
- ✅ Generate PR summaries with automatic fallback
- ✅ Verify repository state
- ✅ Provide clear diagnostics

**Individual commands available:**
```bash
./scripts/copilot-agent-helper.sh diagnose          # Check repository state
./scripts/copilot-agent-helper.sh detect-branch     # Only detect base branch
./scripts/copilot-agent-helper.sh generate-summary  # Only generate PR summary
./scripts/copilot-agent-helper.sh help             # Show all options
```

---

## Quick Diagnosis

**NEW: Automated Fix** (Recommended)
```bash
# Run the automated helper to diagnose and fix all issues
./scripts/copilot-agent-helper.sh fix-all
```

**Manual Diagnosis:**
```bash
git --no-pager log --oneline -5
git --no-pager show-ref | grep main
git --no-pager branch -a
```

## Most Common Issue: Base Branch Not Found

### The Problem
```
fatal: ambiguous argument 'refs/heads/main': unknown revision or path not in the working tree
```

### The Fix (Choose One)

**Option 1: Automated Fix (RECOMMENDED)**
```bash
# Automatically detects base branch and handles shallow clones
./scripts/detect-base-branch.sh origin --export-file
source ./scripts/.base-branch-detected
```

**Option 2: Fetch the base branch**
```bash
git fetch origin main:refs/remotes/origin/main
```

**Option 3: Use remote reference in comparisons**
```bash
# Instead of: git diff main...HEAD
# Use: git diff origin/main...HEAD
git diff origin/main...HEAD
```

**Option 4: Unshallow the clone (if you need full history)**
```bash
git fetch --unshallow
git branch --set-upstream-to=origin/main main
```

## PR Description Generation Failed

### The Problem
```
Failed to get a valid PR summary after 3 attempts.
```

### The Fix

**Option 1: Automated Fix with Fallback (RECOMMENDED)**
```bash
# Automatically generates PR summary with retry and fallback
./scripts/generate-pr-summary.sh

# Or use the complete fix tool
./scripts/copilot-agent-helper.sh fix-all
```

**Option 2: Manual Steps**

1. **First, fix the base branch issue above** (it's often the root cause)

2. **Verify repository state:**
   ```bash
   git status
   git fetch origin
   git log origin/main..HEAD --oneline
   ```

3. **Check for corrupted state:**
   ```bash
   git fsck
   ```

4. **If all else fails, manually create PR description:**
   - Go to the PR on GitHub
   - Edit the description
   - Summarize your changes clearly

## Agent Assignment Blank

### The Problem
Agent doesn't seem to be working or uses default configuration

### The Fix

1. **Check if there's a custom agent configuration** (usually not in repository)
2. **Use the default agent** - it should work for most tasks
3. **Contact your GitHub org admin** if custom agents are needed

## Emergency: Complete Work Manually

If the Copilot agent failed but you need to complete the task:

1. **Checkout the branch:**
   ```bash
   git checkout copilot/<branch-name>
   ```

2. **Make your changes:**
   - Edit files as needed
   - Run tests: `dotnet test OrkinosaiCMS.sln`
   - Build: `dotnet build OrkinosaiCMS.sln --configuration Release`

3. **Commit and push:**
   ```bash
   git add .
   git commit -m "Complete task: <description>"
   git push origin copilot/<branch-name>
   ```

4. **Update the PR:**
   - Go to GitHub
   - Edit PR description
   - Mark as ready for review

## Verification Checklist

After applying fixes, verify:

- [ ] Base branch is accessible: `git log origin/main -1`
- [ ] Changes are visible: `git diff origin/main...HEAD`
- [ ] Build succeeds: `dotnet build OrkinosaiCMS.sln`
- [ ] Tests pass: `dotnet test OrkinosaiCMS.sln`
- [ ] PR is updated on GitHub

## Need More Help?

1. See [copilot-agent-troubleshooting.md](./copilot-agent-troubleshooting.md) for detailed explanations
2. See [github-copilot-agent-guide.md](./github-copilot-agent-guide.md) for comprehensive guide
3. Check GitHub Actions logs for specific error messages
4. Contact your team lead or GitHub org administrator

## Common Commands Reference

```bash
# Check repository state
git status
git --no-pager log --oneline -10
git --no-pager show-ref

# Fetch branches
git fetch origin
git fetch origin main:refs/remotes/origin/main

# Compare branches
git diff origin/main...HEAD
git log origin/main..HEAD --oneline

# Build and test
dotnet restore OrkinosaiCMS.sln
dotnet build OrkinosaiCMS.sln --configuration Release
dotnet test OrkinosaiCMS.sln --configuration Release

# Fix merge conflicts
git fetch origin main
git merge origin/main
# resolve conflicts
git commit
git push
```

## Prevention Tips

1. **Keep your base branch up to date**
2. **Use clear, specific task descriptions**
3. **Break large tasks into smaller ones**
4. **Review changes promptly**
5. **Provide feedback on agent performance**

---

*Last Updated: 2025-11-29*
*For detailed troubleshooting, see copilot-agent-troubleshooting.md*
