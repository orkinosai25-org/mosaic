# Copilot Agent Integration Guide

## Overview

This guide shows how to integrate the automated Copilot agent helper scripts into your workflows, CI/CD pipelines, and development processes.

## For Developers

### Before Starting Work

Run diagnostics to ensure your environment is ready:

```bash
./scripts/copilot-agent-helper.sh diagnose
```

### Before Creating a PR

Automatically generate a PR summary:

```bash
./scripts/copilot-agent-helper.sh generate-summary
# Output: pr-summary.md (use this content in your PR description)
```

### When You Encounter Branch Issues

```bash
# Auto-detect and fix base branch issues
./scripts/copilot-agent-helper.sh detect-branch

# Or fix everything at once
./scripts/copilot-agent-helper.sh fix-all
```

## For CI/CD Pipelines

### GitHub Actions Integration

Add this step to your workflow to ensure base branch is available:

```yaml
name: Your Workflow

on: [push, pull_request]

jobs:
  build:
    runs-on: ubuntu-latest
    
    steps:
      - name: Checkout code
        uses: actions/checkout@v4
        with:
          fetch-depth: 0  # Full history prevents branch detection issues
      
      - name: Detect base branch
        run: |
          ./scripts/detect-base-branch.sh origin --export-file
          source ./scripts/.base-branch-detected
          echo "BASE_BRANCH=$BASE_BRANCH" >> $GITHUB_ENV
          echo "BASE_REF=$BASE_REF" >> $GITHUB_ENV
      
      - name: Your build steps
        run: |
          # Use $BASE_BRANCH and $BASE_REF in your commands
          echo "Building against base branch: $BASE_BRANCH"
```

### For Shallow Clones

If you must use shallow clones (for performance), the scripts handle this automatically:

```yaml
- name: Checkout code (shallow)
  uses: actions/checkout@v4
  with:
    fetch-depth: 1  # Shallow clone

- name: Fix base branch detection
  run: |
    # Scripts automatically detect and handle shallow clones
    ./scripts/copilot-agent-helper.sh detect-branch
```

### Pre-PR Workflow

Add a workflow to automatically generate PR summaries:

```yaml
name: Auto Generate PR Summary

on:
  pull_request:
    types: [opened, synchronize]

jobs:
  generate-summary:
    runs-on: ubuntu-latest
    
    steps:
      - name: Checkout PR branch
        uses: actions/checkout@v4
        with:
          fetch-depth: 0
      
      - name: Generate PR Summary
        run: |
          ./scripts/generate-pr-summary.sh
      
      - name: Comment PR with summary
        uses: actions/github-script@v7
        with:
          script: |
            const fs = require('fs');
            const summary = fs.readFileSync('pr-summary.md', 'utf8');
            
            await github.rest.issues.createComment({
              issue_number: context.issue.number,
              owner: context.repo.owner,
              repo: context.repo.repo,
              body: `## Auto-Generated PR Summary\n\n${summary}`
            });
```

## For Copilot Agent Workflows

### Pre-flight Checks

Add these checks before running Copilot agent tasks:

```yaml
- name: Copilot Pre-flight Checks
  run: |
    # Ensure environment is ready
    ./scripts/copilot-agent-helper.sh diagnose
    
    # Detect and verify base branch
    ./scripts/copilot-agent-helper.sh detect-branch
```

### Post-failure Recovery

If a Copilot agent fails, run this to help debug and fix:

```yaml
- name: Recover from Copilot failure
  if: failure()
  run: |
    # Run diagnostics
    ./scripts/copilot-agent-helper.sh diagnose
    
    # Try to fix common issues
    ./scripts/copilot-agent-helper.sh fix-all
    
    # Generate fallback PR summary
    ./scripts/generate-pr-summary.sh
```

## Environment Variables

### detect-base-branch.sh

- `REMOTE` - Git remote name (default: `origin`)

### generate-pr-summary.sh

- `BASE_BRANCH` - Override base branch detection
- `REMOTE` - Git remote name (default: `origin`)
- `MAX_ATTEMPTS` - Maximum retry attempts (default: `3`)
- `RETRY_DELAY` - Initial retry delay in seconds (default: `2`)
- `OUTPUT_FILE` - Output file path (default: `pr-summary.md`)

### copilot-agent-helper.sh

Inherits all variables from the above scripts, plus:
- All environment variables are passed to sub-scripts

## Example Workflows

### Local Development

```bash
# Start of work session
./scripts/copilot-agent-helper.sh diagnose

# Make changes...
git add .
git commit -m "Your changes"

# Before pushing
./scripts/copilot-agent-helper.sh generate-summary

# Review pr-summary.md and use it in your PR
```

### Automated PR Creation

```bash
#!/bin/bash
# create-pr.sh - Automated PR creation with summary

# Ensure base branch is detected
./scripts/copilot-agent-helper.sh detect-branch
source ./scripts/.base-branch-detected

# Generate PR summary
./scripts/generate-pr-summary.sh

# Create PR with generated summary
gh pr create \
  --title "Your PR Title" \
  --body-file pr-summary.md \
  --base "$BASE_BRANCH"
```

### CI/CD with Fallback

```yaml
- name: Build and Test
  run: |
    # Detect base branch with fallback
    if ! ./scripts/detect-base-branch.sh origin --export-file; then
      echo "BASE_BRANCH=main" >> $GITHUB_ENV
      echo "Using fallback base branch: main"
    else
      source ./scripts/.base-branch-detected
      echo "BASE_BRANCH=$BASE_BRANCH" >> $GITHUB_ENV
    fi
    
    # Use detected base branch
    echo "Comparing against: $BASE_BRANCH"
```

## Troubleshooting Integration Issues

### Issue: Scripts not executable

```bash
chmod +x scripts/*.sh
```

### Issue: Authentication failures

Scripts use git operations that may require authentication. Ensure:
- GitHub Actions: Use `GITHUB_TOKEN` or PAT
- Local: Set up git credentials or SSH keys

```yaml
- name: Setup Git credentials
  env:
    GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
  run: |
    git config --global credential.helper store
    echo "https://$GITHUB_TOKEN@github.com" > ~/.git-credentials
```

### Issue: Base branch not found

The scripts handle this automatically, but if you need to specify:

```bash
BASE_BRANCH=develop ./scripts/copilot-agent-helper.sh fix-all
```

### Issue: Shallow clone limitations

While scripts handle shallow clones, some operations work better with full history:

```yaml
- uses: actions/checkout@v4
  with:
    fetch-depth: 0  # Full history
```

Or unshallow after checkout:

```bash
git fetch --unshallow
```

## Best Practices

1. **Always run diagnostics first** in automated workflows
2. **Use full git history** when possible for better accuracy
3. **Capture script output** for debugging
4. **Set appropriate retry limits** based on your use case
5. **Version control your workflow files** to track changes

## Support

If you encounter issues:

1. Run diagnostics: `./scripts/copilot-agent-helper.sh diagnose`
2. Check logs in `/tmp/` for detailed error messages
3. Review [QUICK_FIX_GUIDE.md](./QUICK_FIX_GUIDE.md)
4. See [copilot-agent-troubleshooting.md](./copilot-agent-troubleshooting.md)

## Contributing

To add new integration patterns or improve existing ones:

1. Test thoroughly in your environment
2. Document the pattern in this file
3. Add examples with clear comments
4. Submit a PR with your changes

---

*Last updated: 2025-11-29*
