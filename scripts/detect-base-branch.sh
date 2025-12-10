#!/bin/bash
# detect-base-branch.sh - Automatically detect the repository's base branch
# This script handles the common issue where 'main' branch is not available locally
# in shallow clones, and intelligently detects the correct base branch.

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print messages
print_info() {
    echo -e "${BLUE}ℹ${NC} $1"
}

print_success() {
    echo -e "${GREEN}✓${NC} $1"
}

print_error() {
    echo -e "${RED}✗${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}⚠${NC} $1"
}

# Configuration
REMOTE="${1:-origin}"
PREFERRED_BRANCHES=("main" "master" "develop" "development")

echo -e "${GREEN}=== Base Branch Detection Tool ===${NC}"
echo ""

# Check if we're in a git repository
if ! git rev-parse --git-dir > /dev/null 2>&1; then
    print_error "Not in a git repository!"
    exit 1
fi

print_success "In a git repository"

# Function to check if a branch exists
branch_exists() {
    local branch="$1"
    local remote="$2"
    
    # Check various possible references
    if git rev-parse "$remote/$branch" >/dev/null 2>&1; then
        return 0
    elif git rev-parse "refs/remotes/$remote/$branch" >/dev/null 2>&1; then
        return 0
    elif git rev-parse "refs/heads/$branch" >/dev/null 2>&1; then
        return 0
    fi
    
    return 1
}

# Function to fetch a branch if needed
fetch_branch() {
    local branch="$1"
    local remote="$2"
    
    print_info "Fetching $remote/$branch..."
    
    if git fetch "$remote" "$branch:refs/remotes/$remote/$branch" 2>/dev/null; then
        return 0
    elif git fetch "$remote" "$branch" 2>/dev/null; then
        return 0
    fi
    
    return 1
}

# Try to detect the base branch
BASE_BRANCH=""

print_info "Detecting base branch..."
echo ""

# First, try to get the default branch from remote
if command -v gh >/dev/null 2>&1; then
    print_info "Attempting to detect default branch using GitHub CLI..."
    DEFAULT_BRANCH=$(gh repo view --json defaultBranchRef -q .defaultBranchRef.name 2>/dev/null || echo "")
    
    if [ -n "$DEFAULT_BRANCH" ]; then
        print_success "GitHub CLI reports default branch: $DEFAULT_BRANCH"
        PREFERRED_BRANCHES=("$DEFAULT_BRANCH" "${PREFERRED_BRANCHES[@]}")
    fi
fi

# Try each preferred branch in order
for branch in "${PREFERRED_BRANCHES[@]}"; do
    print_info "Checking for '$branch'..."
    
    if branch_exists "$branch" "$REMOTE"; then
        print_success "Found existing reference to $REMOTE/$branch"
        BASE_BRANCH="$branch"
        break
    else
        print_warning "Branch '$branch' not found locally, attempting to fetch..."
        
        if fetch_branch "$branch" "$REMOTE"; then
            print_success "Successfully fetched $REMOTE/$branch"
            BASE_BRANCH="$branch"
            break
        else
            print_warning "Could not fetch '$branch' from $REMOTE"
        fi
    fi
done

# If still no base branch found, try to detect from remote branches
if [ -z "$BASE_BRANCH" ]; then
    print_warning "Could not find standard base branches, checking available remote branches..."
    
    # Get list of remote branches (excluding copilot branches initially)
    REMOTE_BRANCHES=$(git branch -r 2>/dev/null | grep "$REMOTE/" | grep -v "HEAD" | grep -v "copilot/" | sed "s|.*$REMOTE/||" | head -10)
    
    if [ -n "$REMOTE_BRANCHES" ]; then
        echo "Available remote branches (excluding copilot branches):"
        echo "$REMOTE_BRANCHES" | while read -r branch; do
            echo "  - $branch"
        done
        echo ""
        
        # Take the first available remote branch as fallback
        FIRST_BRANCH=$(echo "$REMOTE_BRANCHES" | head -1)
        print_warning "Using '$FIRST_BRANCH' as base branch (first available non-copilot branch)"
        BASE_BRANCH="$FIRST_BRANCH"
    else
        # If only copilot branches exist, check all branches
        ALL_REMOTE_BRANCHES=$(git branch -r 2>/dev/null | grep "$REMOTE/" | grep -v "HEAD" | sed "s|.*$REMOTE/||" | head -10)
        
        if [ -n "$ALL_REMOTE_BRANCHES" ]; then
            echo "Available remote branches:"
            echo "$ALL_REMOTE_BRANCHES" | while read -r branch; do
                echo "  - $branch"
            done
            echo ""
            
            # In shallow clone with only copilot branch, we need to assume a base
            print_warning "Only copilot branches found - likely a shallow clone"
        fi
    fi
fi

# If still no base branch, try to infer from recent merge commits
if [ -z "$BASE_BRANCH" ]; then
    print_warning "No remote branches available, checking merge commits for hints..."
    
    # Look for merge commit messages that might indicate base branch
    MERGE_INFO=$(git log --oneline --grep="Merge pull request" -5 2>/dev/null | head -1)
    
    if [ -n "$MERGE_INFO" ]; then
        print_info "Found merge commit: $MERGE_INFO"
        
        # Common patterns: "Merge pull request #N from org/branch"
        # The target is usually mentioned in PR context
        for branch in main master develop; do
            # Assume most common base branch if we found a merge
            print_warning "Inferring base branch as '$branch' based on merge history"
            BASE_BRANCH="$branch"
            
            # Create a grafted reference if in shallow clone
            if [ -f .git/shallow ]; then
                print_info "Repository is shallow, will use grafted parent as base reference"
            fi
            break
        done
    fi
fi

# Last resort: use 'main' as conventional default
if [ -z "$BASE_BRANCH" ]; then
    print_warning "Could not detect base branch through any method"
    print_info "Using conventional default: 'main'"
    BASE_BRANCH="main"
fi

# Final check
if [ -z "$BASE_BRANCH" ]; then
    print_error "Could not detect base branch!"
    echo ""
    echo "Troubleshooting:"
    echo "  1. Ensure you have network connectivity"
    echo "  2. Run: git fetch $REMOTE"
    echo "  3. Check available branches: git branch -r"
    echo "  4. Manually specify base branch: export BASE_BRANCH=your-branch-name"
    exit 1
fi

# Verify the detected base branch is accessible
echo ""
print_info "Verifying detected base branch..."

if branch_exists "$BASE_BRANCH" "$REMOTE"; then
    BASE_REF="$REMOTE/$BASE_BRANCH"
    BASE_SHA=$(git rev-parse "$BASE_REF" 2>/dev/null || git rev-parse "refs/remotes/$BASE_REF" 2>/dev/null || echo "")
    
    # Verify we got a valid SHA
    if [ -z "$BASE_SHA" ]; then
        print_error "Could not resolve SHA for $BASE_REF"
        exit 1
    fi
    
    print_success "Base branch verified: $BASE_REF"
    echo "  SHA: $BASE_SHA"
    echo ""
else
    # In shallow clones, we might not have the base branch but we can still use it
    print_warning "Base branch $REMOTE/$BASE_BRANCH not found locally"
    
    if [ -f .git/shallow ]; then
        print_info "Repository is shallow - base branch reference may not be available"
        print_info "Using grafted parent as comparison point"
        
        # Find the grafted commit (shallow boundary)
        # The shallow file contains SHAs of commits that are boundaries of the shallow clone
        # Use read to safely read the file content
        GRAFT_SHA=""
        if read -r GRAFT_SHA < .git/shallow 2>/dev/null; then
            # Constant for SHA-1 hash length (40 hex characters)
            SHA1_LENGTH=40
            
            # Validate SHA format (40 hex characters for SHA-1)
            if [[ "$GRAFT_SHA" =~ ^[0-9a-f]{${SHA1_LENGTH}}$ ]]; then
                # Verify the SHA is accessible before using it
                if git rev-parse --verify "$GRAFT_SHA" >/dev/null 2>&1; then
                    BASE_REF="$GRAFT_SHA"
                    print_warning "Using grafted parent as base: ${GRAFT_SHA:0:8}"
                else
                    print_error "Grafted commit ${GRAFT_SHA:0:8} is not accessible"
                    exit 1
                fi
            else
                print_error "Invalid SHA format in .git/shallow (expected ${SHA1_LENGTH} hex chars)"
                exit 1
            fi
        else
            print_error "Could not read grafted commit from .git/shallow"
            exit 1
        fi
    else
        print_error "Verification failed for $BASE_BRANCH"
        print_info "Try running: git fetch $REMOTE $BASE_BRANCH"
        exit 1
    fi
fi

# Output the base branch for use in scripts
echo ""
echo -e "${GREEN}=== Result ===${NC}"
echo "BASE_BRANCH=\"$BASE_BRANCH\""
echo "BASE_REF=\"$BASE_REF\""
echo ""

# Export for use in same shell session
export BASE_BRANCH
export BASE_REF

# Optionally write to a file for sourcing
if [ "${2}" == "--export-file" ] || [ "${3}" == "--export-file" ]; then
    OUTPUT_FILE="${BASH_SOURCE[0]%/*}/.base-branch-detected"
    echo "export BASE_BRANCH=\"$BASE_BRANCH\"" > "$OUTPUT_FILE"
    echo "export BASE_REF=\"$BASE_REF\"" >> "$OUTPUT_FILE"
    print_success "Exported to $OUTPUT_FILE (source this file to use in other scripts)"
fi

print_success "Base branch detection complete!"
exit 0
