#!/bin/sh
# requires https://git-cliff.org/
# Generate release notes
# Usage: generate-release-notes.sh [--preview]
# - With `--preview` it shows what's new since last release tag - use this if you haven't yet tagged the next release
# - Without `--preview` it shows what was new in the latest release tag (since the previous release tag) - use this if you have already tagged the release

SCRIPT_DIR=$(cd "$(dirname "$0")" && pwd)

export CLIFF_CURRENT_REF=$(git rev-parse --abbrev-ref HEAD)
export CLIFF_CURRENT_SHA_SHORT=$(git rev-parse --short HEAD)
export CLIFF_CURRENT_SHA=$(git rev-parse HEAD)
export CLIFF_PREVIOUS_TAG=$(git describe --tags --abbrev=0 2>/dev/null || echo "unknown")
export CLIFF_PREVIOUS_SHA_SHORT=$(git rev-parse --short "$CLIFF_PREVIOUS_TAG^{commit}" 2>/dev/null || echo "unknown")
export CLIFF_PREVIOUS_SHA=$(git rev-parse "$CLIFF_PREVIOUS_TAG^{commit}" 2>/dev/null || echo "unknown")

if [ "$1" = "--preview" ]; then
    MODE="--unreleased"
else
    MODE="--latest"
fi

git-cliff $MODE --strip header --config "$SCRIPT_DIR/cliff.toml"
