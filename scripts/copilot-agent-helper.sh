#!/bin/bash
# copilot-agent-helper.sh - Main helper script for Copilot agent workflows
# This script orchestrates branch detection and PR summary generation
# to prevent common failures in Copilot agent workflows.

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Configuration
REMOTE="${REMOTE:-origin}"
BASE_BRANCH="${BASE_BRANCH:-}"
ACTION="${1:-help}"

# Function to print messages
print_header() {
    echo ""
    echo -e "${CYAN}╔════════════════════════════════════════════════════════════╗${NC}"
    echo -e "${CYAN}║${NC}  ${GREEN}$1${NC}"
    echo -e "${CYAN}╚════════════════════════════════════════════════════════════╝${NC}"
    echo ""
}

print_section() {
    echo ""
    echo -e "${BLUE}▶ $1${NC}"
    echo ""
}

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

# Function to show help
show_help() {
    cat << EOF
${GREEN}Copilot Agent Helper Script${NC}

This script helps prevent and fix common issues in Copilot agent workflows,
specifically around base branch detection and PR summary generation.

${BLUE}Usage:${NC}
    $0 [command] [options]

${BLUE}Commands:${NC}
    detect-branch       Detect and verify the base branch
    generate-summary    Generate PR summary with fallback mechanisms
    fix-all            Run both branch detection and PR summary generation
    diagnose           Run diagnostics on the repository state
    help               Show this help message

${BLUE}Environment Variables:${NC}
    BASE_BRANCH        Override base branch (auto-detected if not set)
    REMOTE             Git remote name (default: origin)
    MAX_ATTEMPTS       Maximum retry attempts for PR summary (default: 3)
    OUTPUT_FILE        Output file for PR summary (default: pr-summary.md)

${BLUE}Examples:${NC}
    # Auto-detect base branch
    $0 detect-branch

    # Generate PR summary with auto-detection
    $0 generate-summary

    # Fix all issues (recommended before completing PR work)
    $0 fix-all

    # Run diagnostics
    $0 diagnose

    # Specify base branch manually
    BASE_BRANCH=develop $0 fix-all

${BLUE}Common Use Cases:${NC}

    1. ${YELLOW}Before starting Copilot agent work:${NC}
       $0 diagnose

    2. ${YELLOW}When agent fails with "branch not found":${NC}
       $0 detect-branch

    3. ${YELLOW}When PR summary generation fails:${NC}
       $0 generate-summary

    4. ${YELLOW}To fix all issues at once:${NC}
       $0 fix-all

${BLUE}Related Scripts:${NC}
    - detect-base-branch.sh      Auto-detect base branch
    - generate-pr-summary.sh     Generate PR summaries with retry
    - fix-base-branch.sh         Legacy branch fix script

${BLUE}Documentation:${NC}
    - docs/QUICK_FIX_GUIDE.md
    - docs/copilot-agent-troubleshooting.md
    - scripts/README.md

EOF
}

# Function to run diagnostics
run_diagnostics() {
    print_header "Repository Diagnostics"
    
    # Check if in git repo
    if ! git rev-parse --git-dir > /dev/null 2>&1; then
        print_error "Not in a git repository!"
        return 1
    fi
    print_success "In a git repository"
    
    # Current branch
    CURRENT_BRANCH=$(git branch --show-current)
    print_info "Current branch: $CURRENT_BRANCH"
    
    # Check remote
    if git remote get-url "$REMOTE" > /dev/null 2>&1; then
        REMOTE_URL=$(git remote get-url "$REMOTE")
        print_success "Remote '$REMOTE' configured: $REMOTE_URL"
    else
        print_error "Remote '$REMOTE' not configured"
        return 1
    fi
    
    # List available remote branches
    print_section "Available Remote Branches"
    if git branch -r | grep "$REMOTE/" > /dev/null 2>&1; then
        git branch -r | grep "$REMOTE/" | head -10 | while read -r branch; do
            echo "  $branch"
        done
    else
        print_warning "No remote branches visible (may need to fetch)"
    fi
    
    # Check for common base branches
    print_section "Base Branch Detection"
    for branch in main master develop development; do
        if git rev-parse "$REMOTE/$branch" >/dev/null 2>&1; then
            SHA=$(git rev-parse "$REMOTE/$branch")
            print_success "Found $REMOTE/$branch (${SHA:0:8})"
        else
            print_warning "Not found: $REMOTE/$branch"
        fi
    done
    
    # Check repository state
    print_section "Repository State"
    
    if [ -n "$(git status --porcelain)" ]; then
        CHANGED_COUNT=$(git status --porcelain | wc -l)
        print_warning "Working directory has $CHANGED_COUNT uncommitted change(s)"
    else
        print_success "Working directory is clean"
    fi
    
    # Recent commits
    print_section "Recent Commits"
    git log --oneline -5 | while read -r line; do
        echo "  $line"
    done
    
    print_section "Diagnostic Summary"
    print_success "Diagnostics complete"
    return 0
}

# Function to detect base branch
detect_base_branch() {
    print_header "Base Branch Detection"
    
    if [ -f "$SCRIPT_DIR/detect-base-branch.sh" ]; then
        bash "$SCRIPT_DIR/detect-base-branch.sh" "$REMOTE" --export-file
        
        # Source the result
        if [ -f "$SCRIPT_DIR/.base-branch-detected" ]; then
            source "$SCRIPT_DIR/.base-branch-detected"
            print_success "Base branch detected and exported: $BASE_BRANCH"
            return 0
        fi
    else
        print_error "detect-base-branch.sh not found in $SCRIPT_DIR"
        return 1
    fi
    
    return 1
}

# Function to generate PR summary
generate_pr_summary() {
    print_header "PR Summary Generation"
    
    # Ensure base branch is detected
    if [ -z "$BASE_BRANCH" ]; then
        print_info "Base branch not set, running detection first..."
        if ! detect_base_branch; then
            print_error "Failed to detect base branch"
            return 1
        fi
        
        # Source the detected branch
        if [ -f "$SCRIPT_DIR/.base-branch-detected" ]; then
            source "$SCRIPT_DIR/.base-branch-detected"
        fi
    fi
    
    if [ -f "$SCRIPT_DIR/generate-pr-summary.sh" ]; then
        export BASE_BRANCH
        export REMOTE
        bash "$SCRIPT_DIR/generate-pr-summary.sh"
        return $?
    else
        print_error "generate-pr-summary.sh not found in $SCRIPT_DIR"
        return 1
    fi
}

# Function to fix all issues
fix_all() {
    print_header "Copilot Agent Complete Fix"
    
    print_info "This will run all fixes in sequence..."
    echo ""
    
    # Step 1: Detect base branch
    print_section "Step 1: Base Branch Detection"
    if detect_base_branch; then
        print_success "Base branch detection completed"
    else
        print_error "Base branch detection failed"
        return 1
    fi
    
    # Source the detected branch for next steps
    if [ -f "$SCRIPT_DIR/.base-branch-detected" ]; then
        source "$SCRIPT_DIR/.base-branch-detected"
    fi
    
    # Step 2: Verify branch access
    print_section "Step 2: Verify Branch Access"
    if [ -f "$SCRIPT_DIR/fix-base-branch.sh" ]; then
        # Use mktemp for atomic temp file creation to avoid race conditions
        local TEMP_LOG=$(mktemp /tmp/fix-base-branch.XXXXXX)
        
        if bash "$SCRIPT_DIR/fix-base-branch.sh" "$BASE_BRANCH" "$REMOTE" > "$TEMP_LOG" 2>&1; then
            print_success "Branch access verified"
            rm -f "$TEMP_LOG"
        else
            print_warning "Branch verification had warnings"
            print_info "Check log: $TEMP_LOG"
            # Log will remain for user to review, cleanup is responsibility of temp cleaner
        fi
    fi
    
    # Step 3: Generate PR summary
    print_section "Step 3: Generate PR Summary"
    if generate_pr_summary; then
        print_success "PR summary generation completed"
    else
        print_error "PR summary generation failed"
        return 1
    fi
    
    # Summary
    print_header "All Fixes Completed Successfully"
    print_success "Base branch: $BASE_BRANCH"
    print_success "PR summary: ${OUTPUT_FILE:-pr-summary.md}"
    echo ""
    print_info "You can now proceed with your PR work"
    
    return 0
}

# Main script logic
case "$ACTION" in
    detect-branch)
        detect_base_branch
        ;;
    generate-summary)
        generate_pr_summary
        ;;
    fix-all)
        fix_all
        ;;
    diagnose)
        run_diagnostics
        ;;
    help|--help|-h)
        show_help
        ;;
    *)
        print_error "Unknown command: $ACTION"
        echo ""
        show_help
        exit 1
        ;;
esac
