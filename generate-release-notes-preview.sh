#!/bin/sh
# requires https://git-cliff.org/
# show what would be in this release if we cut a release now and tagged it

export CLIFF_CURRENT_REF=$(git rev-parse --abbrev-ref HEAD)
export CLIFF_CURRENT_SHA_SHORT=$(git rev-parse --short HEAD)
export CLIFF_CURRENT_SHA=$(git rev-parse HEAD)
export CLIFF_PREVIOUS_TAG=$(git describe --tags --abbrev=0 2>/dev/null || echo "unknown")
export CLIFF_PREVIOUS_SHA_SHORT=$(git rev-parse --short "$CLIFF_PREVIOUS_TAG^{commit}" 2>/dev/null || echo "unknown")
export CLIFF_PREVIOUS_SHA=$(git rev-parse "$CLIFF_PREVIOUS_TAG^{commit}" 2>/dev/null || echo "unknown")

git-cliff --unreleased --strip header --config cliff.toml | tee release-note-preview.md
