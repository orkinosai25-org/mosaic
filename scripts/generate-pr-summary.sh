#!/bin/bash
# generate-pr-summary.sh - Generate PR summary with automatic fallback mechanisms
# This script handles PR description generation with multiple retry strategies
# and fallbacks when the primary method fails.

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
MAX_ATTEMPTS="${MAX_ATTEMPTS:-3}"
RETRY_DELAY="${RETRY_DELAY:-2}"
BASE_BRANCH="${BASE_BRANCH:-}"
REMOTE="${REMOTE:-origin}"
OUTPUT_FILE="${OUTPUT_FILE:-pr-summary.md}"

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

echo -e "${GREEN}=== PR Summary Generation Tool ===${NC}"
echo ""

# Check if we're in a git repository
if ! git rev-parse --git-dir > /dev/null 2>&1; then
    print_error "Not in a git repository!"
    exit 1
fi

print_success "In a git repository"

# Get current branch
CURRENT_BRANCH=$(git branch --show-current)
print_info "Current branch: $CURRENT_BRANCH"

# Detect base branch if not provided
if [ -z "$BASE_BRANCH" ]; then
    print_info "Base branch not specified, attempting auto-detection..."
    
    # Try to use detect-base-branch.sh if available
    SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
    if [ -f "$SCRIPT_DIR/detect-base-branch.sh" ]; then
        print_info "Running base branch detection..."
        if bash "$SCRIPT_DIR/detect-base-branch.sh" "$REMOTE" --export-file > /tmp/base-branch-detect.log 2>&1; then
            # Source the exported variables
            if [ -f "$SCRIPT_DIR/.base-branch-detected" ]; then
                source "$SCRIPT_DIR/.base-branch-detected"
                print_success "Base branch auto-detected: $BASE_BRANCH"
            fi
        else
            print_warning "Auto-detection script failed, trying fallback methods..."
            cat /tmp/base-branch-detect.log
        fi
    fi
    
    # Fallback: try common base branches
    if [ -z "$BASE_BRANCH" ]; then
        for branch in main master develop; do
            if git rev-parse "$REMOTE/$branch" >/dev/null 2>&1; then
                BASE_BRANCH="$branch"
                print_success "Using base branch: $BASE_BRANCH"
                break
            fi
        done
    fi
    
    # Last resort
    if [ -z "$BASE_BRANCH" ]; then
        print_error "Could not detect base branch. Please set BASE_BRANCH environment variable."
        echo ""
        echo "Usage: BASE_BRANCH=main $0"
        exit 1
    fi
fi

# Check if BASE_REF was set by detect-base-branch.sh
if [ -z "$BASE_REF" ]; then
    BASE_REF="$REMOTE/$BASE_BRANCH"
fi

# Verify base branch is accessible
print_info "Verifying base branch access..."
if ! git rev-parse "$BASE_REF" >/dev/null 2>&1; then
    print_warning "Base branch $BASE_REF not found locally, attempting to fetch..."
    
    if git fetch "$REMOTE" "$BASE_BRANCH" 2>/dev/null; then
        BASE_REF="$REMOTE/$BASE_BRANCH"
        print_success "Successfully fetched $BASE_REF"
    else
        # In shallow clones, we might need to use the grafted parent
        if [ -f .git/shallow ]; then
            print_warning "Repository is shallow, using grafted parent as base"
            GRAFT_SHA=$(cat .git/shallow 2>/dev/null | head -1)
            if [ -n "$GRAFT_SHA" ]; then
                BASE_REF="$GRAFT_SHA"
                print_success "Using grafted parent as base: ${GRAFT_SHA:0:8}"
            else
                print_error "Failed to determine base reference"
                exit 1
            fi
        else
            print_error "Failed to fetch base branch $BASE_REF"
            exit 1
        fi
    fi
else
    print_success "Base branch accessible: $BASE_REF"
fi

# Function to generate basic PR summary from git diff/log
generate_basic_summary() {
    local base_ref="$1"
    local output_file="$2"
    
    # Validate base_ref to prevent command injection
    # Allow valid git reference characters: alphanumeric, /, -, _, .
    if [[ ! "$base_ref" =~ ^[a-zA-Z0-9/_.-]+$ ]]; then
        print_error "Invalid base reference format: $base_ref"
        return 1
    fi
    
    print_info "Generating basic PR summary from git history..."
    
    {
        echo "# Pull Request Summary"
        echo ""
        echo "## Changes Overview"
        echo ""
        
        # Get commit messages
        echo "### Commits"
        echo ""
        git log --oneline "$base_ref..HEAD" | while read -r line; do
            echo "- $line"
        done
        echo ""
        
        # Get file statistics
        echo "### Files Changed"
        echo ""
        git diff --stat "$base_ref...HEAD" | tail -n 1
        echo ""
        
        # List changed files
        echo "### Modified Files"
        echo ""
        git diff --name-status "$base_ref...HEAD" | while read -r status file; do
            case "$status" in
                A) echo "- ➕ Added: \`$file\`" ;;
                M) echo "- ✏️  Modified: \`$file\`" ;;
                D) echo "- ➖ Deleted: \`$file\`" ;;
                R*) echo "- 🔄 Renamed: \`$file\`" ;;
                *) echo "- ⚡ Changed: \`$file\`" ;;
            esac
        done
        echo ""
        
        # Add description section
        echo "## Description"
        echo ""
        echo "_Please provide a description of the changes in this PR._"
        echo ""
        
        echo "## Testing"
        echo ""
        echo "- [ ] Build passes"
        echo "- [ ] Tests pass"
        echo "- [ ] Manual testing completed"
        echo ""
        
        echo "---"
        echo "*Auto-generated summary - please review and update as needed*"
        
    } > "$output_file"
    
    if [ -f "$output_file" ] && [ -s "$output_file" ]; then
        return 0
    else
        return 1
    fi
}

# Function to generate enhanced summary with AI/GitHub CLI
generate_enhanced_summary() {
    local base_ref="$1"
    local output_file="$2"
    
    print_info "Attempting to generate enhanced PR summary..."
    
    # Check if GitHub CLI is available
    if command -v gh >/dev/null 2>&1; then
        print_info "GitHub CLI detected, attempting AI-assisted summary..."
        
        # Get diff content and save to temporary file to avoid command injection
        # Limit to 50KB to avoid overwhelming the AI model with large diffs
        # Use mktemp for atomic temporary file creation to avoid race conditions
        local DIFF_FILE=$(mktemp /tmp/pr-diff.XXXXXX)
        
        # Set up cleanup trap to ensure temp file is always removed
        # Use EXIT for more reliable cleanup across different shell environments
        trap "rm -f $DIFF_FILE" EXIT
        
        git diff "$base_ref...HEAD" | head -c 50000 > "$DIFF_FILE"
        
        # Try to use GitHub CLI to generate summary (if copilot extension is available)
        # Using stdin to safely pass diff content
        if gh copilot suggest "Generate a concise PR summary for the following changes:" < "$DIFF_FILE" > /tmp/gh-copilot-summary.txt 2>&1; then
            if [ -s /tmp/gh-copilot-summary.txt ]; then
                print_success "Generated AI-assisted summary"
                cp /tmp/gh-copilot-summary.txt "$output_file"
                return 0
            fi
        fi
    fi
    
    return 1
}

# Function to validate generated summary
validate_summary() {
    local file="$1"
    
    if [ ! -f "$file" ]; then
        return 1
    fi
    
    if [ ! -s "$file" ]; then
        return 1
    fi
    
    # Check if file has meaningful content (more than just a title)
    local line_count=$(wc -l < "$file")
    if [ "$line_count" -lt 5 ]; then
        return 1
    fi
    
    return 0
}

# Main generation logic with retries and fallbacks
echo ""
print_info "Starting PR summary generation (max $MAX_ATTEMPTS attempts)..."
echo ""

ATTEMPT=1
SUCCESS=false

while [ $ATTEMPT -le $MAX_ATTEMPTS ] && [ "$SUCCESS" = "false" ]; do
    print_info "Attempt $ATTEMPT of $MAX_ATTEMPTS..."
    
    # Try enhanced method first
    if [ $ATTEMPT -eq 1 ]; then
        if generate_enhanced_summary "$BASE_REF" "$OUTPUT_FILE"; then
            if validate_summary "$OUTPUT_FILE"; then
                print_success "Enhanced summary generated successfully!"
                SUCCESS=true
                break
            else
                print_warning "Enhanced summary validation failed"
            fi
        else
            print_warning "Enhanced summary generation failed, falling back to basic method"
        fi
    fi
    
    # Fall back to basic method
    if generate_basic_summary "$BASE_REF" "$OUTPUT_FILE"; then
        if validate_summary "$OUTPUT_FILE"; then
            print_success "Basic summary generated successfully!"
            SUCCESS=true
            break
        else
            print_error "Summary validation failed"
        fi
    else
        print_error "Summary generation failed"
    fi
    
    # If not the last attempt, wait before retrying
    if [ $ATTEMPT -lt $MAX_ATTEMPTS ] && [ "$SUCCESS" = "false" ]; then
        print_info "Retrying in $RETRY_DELAY seconds..."
        sleep $RETRY_DELAY
        # Exponential backoff
        RETRY_DELAY=$((RETRY_DELAY * 2))
    fi
    
    ATTEMPT=$((ATTEMPT + 1))
done

echo ""

if [ "$SUCCESS" = "true" ]; then
    echo -e "${GREEN}=== Success ===${NC}"
    print_success "PR summary generated: $OUTPUT_FILE"
    echo ""
    print_info "Preview:"
    echo "---"
    head -20 "$OUTPUT_FILE"
    if [ $(wc -l < "$OUTPUT_FILE") -gt 20 ]; then
        echo "..."
        echo "(truncated, see $OUTPUT_FILE for full content)"
    fi
    echo "---"
    echo ""
    print_info "You can now use this summary in your PR description"
    exit 0
else
    echo -e "${RED}=== Failed ===${NC}"
    print_error "Failed to generate PR summary after $MAX_ATTEMPTS attempts"
    echo ""
    print_info "Manual fallback:"
    echo "  1. Review your changes: git diff $BASE_REF...HEAD"
    echo "  2. Review commits: git log $BASE_REF..HEAD"
    echo "  3. Manually write your PR description"
    echo ""
    exit 1
fi
