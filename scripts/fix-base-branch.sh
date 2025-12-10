#!/bin/bash
# fix-base-branch.sh - Fix common Copilot agent base branch issues
# This script helps resolve the "base branch not found" error that occurs
# when working with shallow clones in Copilot agent workflows.

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print status (defined early for use in detection)
print_status() {
    echo -e "${GREEN}✓${NC} $1"
}

print_error() {
    echo -e "${RED}✗${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}⚠${NC} $1"
}

# Configuration
REMOTE="${2:-origin}"
BASE_BRANCH="${1:-}"

# Auto-detect base branch if not provided
if [ -z "$BASE_BRANCH" ]; then
    SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
    if [ -f "$SCRIPT_DIR/detect-base-branch.sh" ]; then
        print_warning "No base branch specified, attempting auto-detection..."
        if bash "$SCRIPT_DIR/detect-base-branch.sh" "$REMOTE" --export-file > /tmp/base-branch-detect.log 2>&1; then
            if [ -f "$SCRIPT_DIR/.base-branch-detected" ]; then
                source "$SCRIPT_DIR/.base-branch-detected"
            fi
        fi
    fi
    
    # Fallback to 'main' if detection failed
    if [ -z "$BASE_BRANCH" ]; then
        BASE_BRANCH="main"
    fi
fi

echo -e "${GREEN}=== Copilot Agent Base Branch Fix Tool ===${NC}"
echo ""
echo "This script will:"
echo "  1. Check if the base branch ($BASE_BRANCH) is accessible"
echo "  2. Fetch it if needed"
echo "  3. Verify the repository state"
echo ""

# Check if we're in a git repository
if ! git rev-parse --git-dir > /dev/null 2>&1; then
    print_error "Not in a git repository!"
    exit 1
fi

print_status "In a git repository"

# Check current branch
CURRENT_BRANCH=$(git branch --show-current)
echo "Current branch: $CURRENT_BRANCH"

# Check if base branch exists as a remote ref
echo ""
echo "Checking for base branch..."
if git rev-parse "$REMOTE/$BASE_BRANCH" >/dev/null 2>&1; then
    print_status "Base branch $REMOTE/$BASE_BRANCH is accessible"
    BASE_SHA=$(git rev-parse "$REMOTE/$BASE_BRANCH")
    echo "  SHA: $BASE_SHA"
elif git rev-parse "refs/remotes/$REMOTE/$BASE_BRANCH" >/dev/null 2>&1; then
    print_status "Base branch refs/remotes/$REMOTE/$BASE_BRANCH is accessible"
    BASE_SHA=$(git rev-parse "refs/remotes/$REMOTE/$BASE_BRANCH")
    echo "  SHA: $BASE_SHA"
else
    print_warning "Base branch not found locally. Fetching..."
    
    # Try to fetch the base branch
    if git fetch "$REMOTE" "$BASE_BRANCH:refs/remotes/$REMOTE/$BASE_BRANCH" 2>/dev/null; then
        print_status "Successfully fetched $REMOTE/$BASE_BRANCH"
        BASE_SHA=$(git rev-parse "$REMOTE/$BASE_BRANCH")
        echo "  SHA: $BASE_SHA"
    else
        print_error "Failed to fetch $BASE_BRANCH from $REMOTE"
        echo ""
        echo "Troubleshooting tips:"
        echo "  1. Check that '$BASE_BRANCH' is the correct base branch name"
        echo "  2. Verify you have network access"
        echo "  3. Confirm you have permission to access the repository"
        echo ""
        echo "Available remote branches:"
        git branch -r | grep "$REMOTE/" | head -10
        exit 1
    fi
fi

# Check if we can compare with the base branch
echo ""
echo "Testing comparison with base branch..."
if git diff --shortstat "$REMOTE/$BASE_BRANCH...HEAD" >/dev/null 2>&1; then
    print_status "Can successfully compare with base branch"
    
    # Show the comparison
    CHANGES=$(git diff --shortstat "$REMOTE/$BASE_BRANCH...HEAD")
    if [ -n "$CHANGES" ]; then
        echo "  Changes: $CHANGES"
    else
        print_warning "No changes detected between $REMOTE/$BASE_BRANCH and HEAD"
    fi
else
    print_error "Cannot compare with base branch"
    exit 1
fi

# Show commit range
echo ""
echo "Commits ahead of base branch:"
COMMIT_COUNT=$(git log --oneline "$REMOTE/$BASE_BRANCH..HEAD" | wc -l)
if [ "$COMMIT_COUNT" -eq 0 ]; then
    print_warning "No commits ahead of base branch"
else
    print_status "$COMMIT_COUNT commit(s) ahead"
    git log --oneline "$REMOTE/$BASE_BRANCH..HEAD" | head -5
    if [ "$COMMIT_COUNT" -gt 5 ]; then
        echo "  ... and $((COMMIT_COUNT - 5)) more"
    fi
fi

# Check for merge base
echo ""
echo "Finding merge base..."
if MERGE_BASE=$(git merge-base "$REMOTE/$BASE_BRANCH" HEAD 2>/dev/null); then
    print_status "Merge base found: ${MERGE_BASE:0:8}"
    
    # Check if we're up to date with base
    if [ "$MERGE_BASE" = "$(git rev-parse "$REMOTE/$BASE_BRANCH")" ]; then
        print_status "Branch is up to date with $REMOTE/$BASE_BRANCH"
    else
        BEHIND_COUNT=$(git log --oneline "$MERGE_BASE..$REMOTE/$BASE_BRANCH" | wc -l)
        if [ "$BEHIND_COUNT" -gt 0 ]; then
            print_warning "Branch is $BEHIND_COUNT commit(s) behind $REMOTE/$BASE_BRANCH"
            echo "  Consider rebasing or merging the base branch"
        fi
    fi
else
    print_warning "Could not find merge base (branches may have diverged significantly)"
fi

# Repository health check
echo ""
echo "Repository health check..."

# Check for uncommitted changes
if [ -n "$(git status --porcelain)" ]; then
    print_warning "Uncommitted changes detected"
    echo "  Run 'git status' for details"
else
    print_status "Working directory is clean"
fi

# Check repository integrity
if git fsck --no-progress >/dev/null 2>&1; then
    print_status "Repository integrity check passed"
else
    print_warning "Repository integrity check found issues (run 'git fsck' for details)"
fi

echo ""
echo -e "${GREEN}=== Summary ===${NC}"
echo "Base branch: $REMOTE/$BASE_BRANCH ✓"
echo "Current branch: $CURRENT_BRANCH"
echo "Commits ahead: $COMMIT_COUNT"
echo ""
echo "You can now:"
echo "  • Compare changes: git diff $REMOTE/$BASE_BRANCH...HEAD"
echo "  • View commits: git log $REMOTE/$BASE_BRANCH..HEAD"
echo "  • Create PR: Push your branch and open a pull request"
echo ""
echo -e "${GREEN}All checks passed!${NC}"

exit 0
