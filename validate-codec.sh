#!/bin/bash

# Enable error handling and debugging
set -euo pipefail

# ANSI color codes
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color
GRAY='\033[0;90m'

# Logging functions
log_header() {
    printf "\n${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}\n"
    printf "${BLUE}▶ %s${NC}\n" "$1"
    printf "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}\n"
}

log_step() {
    printf "${GREEN}➤ %s${NC}\n" "$1"
}

log_info() {
    printf "${GRAY}  ℹ %s${NC}\n" "$1"
}

log_warning() {
    printf "${YELLOW}  ⚠ %s${NC}\n" "$1"
}

log_error() {
    printf "${RED}  ✖ %s${NC}\n" "$1"
}

log_success() {
    printf "${GREEN}  ✔ %s${NC}\n" "$1"
}

log_timing() {
    local start_time=$1
    local end_time
    end_time=$(date +%s.%N)
    local duration
    duration=$(awk "BEGIN {print $end_time - $start_time}")
    local command=$2
    printf "${YELLOW}  ⏱ %s took %.2f seconds${NC}\n" "$command" "$duration"
}

# Function to indent output
indent() {
    while IFS= read -r line; do
        printf "${GRAY}    │ %s${NC}\n" "$line"
    done
}

# Parse command line arguments
suite_name=""
from_branch=""
to_branch=""

print_usage() {
    log_error "Invalid usage"
    echo "Usage: $0 --suite <suite_name> --fromcodec <branch_name> --tocodec <branch_name>"
    echo "Example: $0 --suite Jobs --fromcodec releases/jobs --tocodec master"
    exit 1
}

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --suite)
            suite_name="$2"
            shift 2
            ;;
        --fromcodec)
            from_branch="$2"
            shift 2
            ;;
        --tocodec)
            to_branch="$2"
            shift 2
            ;;
        *)
            log_error "Unknown parameter: $1"
            print_usage
            ;;
    esac
done

# Validate required parameters
if [ -z "$suite_name" ] || [ -z "$from_branch" ] || [ -z "$to_branch" ]; then
    log_error "All --suite, --fromcodec and --tocodec parameters are required"
    print_usage
fi

# go to workspace and clone Egg.Shell
init_workspace() {
    # Create workspace directory
    log_step "Creating workspace directory (./CodecValidationWorkSpace)"
    mkdir -p CodecValidationWorkSpace
    pushd CodecValidationWorkSpace > /dev/null
    log_success "Workspace created"

    # Clone repository if it doesn't exist
    if [ ! -d "Egg.Shell" ]; then
        log_step "Cloning Egg.Shell into workspace"
        git clone ../.git Egg.Shell | indent
        log_success "Repository cloned"
    fi
    popd > /dev/null
}

# Function to check if branch exists, if not try to create from origin
check_branch() {
    local branch_name="$1"
    if [[ ! $(git branch --list $branch_name) ]]; then
        log_warning "Branch $branch_name does not exist. Creating from origin."
        git fetch origin "$branch_name":"$branch_name"
    fi
}

# Function to check commits between branches
check_commits() {
    local from_branch="$1"
    local to_branch="$2"

    log_header "Checking target branch is ahead of source branch"
    log_info "Comparing commits between:"
    log_info "From: $from_branch"
    log_info "To: $to_branch"

    check_branch "$from_branch"
    check_branch "$to_branch"

    # Get commits that are in from_branch but not in to_branch
    log_step "Checking for missing commits"
    local missing_commits
    missing_commits=$(git rev-list "$from_branch" --not "$to_branch" --no-merges)

    if [ -n "$missing_commits" ]; then
        log_error "Found commits in '$from_branch' that are not in '$to_branch':"
        echo "$missing_commits" | while IFS= read -r commit_hash; do
            # Get commit details for each missing commit
            commit_details=$(git log -1 --pretty=format:"%h %s (%an)" "$commit_hash")
            printf "${RED}    │ %s${NC}\n" "$commit_details"
        done
        log_error "Aborting pipeline due to missing commits"
        log_error "Please either:"
        log_error "  1. Merge $from_branch into $to_branch"
        log_error "  2. Rebase $to_branch onto $from_branch"
        exit 1
    else
        log_success "All commits from '$from_branch' are present in '$to_branch'"
    fi
}

# Function to build and extract schema for a given branch
build_and_extract_schema() {
    local branch_name="$1"
    local output_file="../../../$2"

    log_header "Building and Extracting Schema for branch: $branch_name"

    pushd CodecValidationWorkSpace/Egg.Shell > /dev/null

    log_step "Updating repository state"
    git reset --hard HEAD | indent
    git fetch origin | indent
    git checkout -f "$branch_name" | indent
    git reset --hard "origin/$branch_name" | indent
    log_success "Repository updated to $branch_name"

    # Apply cherry-picks if needed
    if [ ! -f "LibCodecValidation/CodecLib.fs" ]; then
        log_step "Applying necessary patches"
        local cherry_picks=(
		    "a42f24c0afa00a77f1652571e3035a4c1bb294ee"
        )

        for commit in "${cherry_picks[@]}"; do
            log_info "Applying cherry-pick: ${commit:0:8}"
            git cherry-pick "$commit" | indent
        done
        log_success "Patches applied successfully"
    fi

    # Verify CodecLib.fs exists
    if [ ! -f "LibCodecValidation/CodecLib.fs" ]; then
        log_error "LibCodecValidation/CodecLib.fs not found after setup"
        exit 1
    fi

    # Replace CodecLib.fs with the one from LibCodecValidation
    log_step "Replacing LibLangFsharp/CodecLib.fs with LibCodecValidation/CodecLib.fs"
    sed -i 's|"CodecLib\.fs"|"../../LibCodecValidation/CodecLib.fs"|g' LibLangFsharp/src/LibLangFsharp.fsproj

    # Find types project
    log_step "Searching for types project"
    if [ -f "Suite$suite_name/Ecosystem/$suite_name.Types/$suite_name.Types.fsproj" ]; then
        types_fsproj_path="../../Suite$suite_name/Ecosystem/$suite_name.Types/$suite_name.Types.fsproj"
    elif [ -f "Suite$suite_name/Ecosystem/Types/$suite_name.Types.fsproj" ]; then
        types_fsproj_path="../../Suite$suite_name/Ecosystem/Types/$suite_name.Types.fsproj"
    elif [ -f "Suite$suite_name/Ecosystem/Types/Types.fsproj" ]; then
        types_fsproj_path="../../Suite$suite_name/Ecosystem/Types/Types.fsproj"
    else
        log_error "Types project not found at expected locations"
        exit 1
    fi

    log_info "Found types project at $types_fsproj_path"

    # Build and run validation
    log_step "Building and running Meta/LibCodecValidationHost"
    pushd Meta/LibCodecValidationHost > /dev/null

    # Update project files
    sed -i "s|__TYPES_FSPROJ_LOCATION_WITH_RESPECT_TO_LIB_CODEC_VALIDATION_HOST__|${types_fsproj_path}|g" ./LibCodecValidationHost.fsproj
    sed -i "s|__TYPES_ASSEMBLY_NAME__|${suite_name}.Types|g" ./Program.fs

    log_info "Generating schema file: $output_file"
    local start_time=$(date +%s.%N)
    if ! dotnet run -- "$output_file"; then
        log_error "Schema generation failed"
        exit 1
    fi
    log_timing "$start_time" "Running LibCodecValidationHost"

    popd > /dev/null
    popd > /dev/null

    log_success "Generated schema file: $output_file for branch: $branch_name"
}

evaluate_validation_result() {
    local old_codec_file="../CodecValidationWorkSpace/schema1.bin"
    local new_codec_file="../CodecValidationWorkSpace/schema2.bin"

    log_header "Evaluating Codec Evolution"
    log_info "From: $from_branch"
    log_info "To: $to_branch"

    pushd LibCodecValidation > /dev/null

    log_step "Running validation tool (LibCodecValidation)"

    local start_time=$(date +%s.%N)
    if ! dotnet run -- "$old_codec_file" "$new_codec_file"; then
        log_error "Codec validation failed"
        exit 1
    fi
    log_timing "$start_time" "Running LibCodecValidation"

    popd > /dev/null

    log_success "Validation completed"
}

# Main execution
main() {
    log_header "Starting Codec Validation Pipeline"
    log_info "Suite: $suite_name"
    log_info "From Branch: $from_branch"
    log_info "To Branch: $to_branch"

    # Check commits
    # check_commits "$from_branch" "$to_branch"

    # Initialize workspace
    init_workspace

    # Process branches
    if ! build_and_extract_schema "$from_branch" schema1.bin; then
        log_error "Failed to build and extract schema for $from_branch"
        exit 1
    fi

    if ! build_and_extract_schema "$to_branch" schema2.bin; then
        log_error "Failed to build and extract schema for $to_branch"
        exit 1
    fi

    # Evaluate codec evolution
    if ! evaluate_validation_result "$from_branch" "$to_branch"; then
        log_error "Failed to evaluate codec evolution"
        exit 1
    fi

    log_header "Pipeline Completed Successfully"
}

# Run main function
main
