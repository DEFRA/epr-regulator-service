#!/bin/sh
# requires https://git-cliff.org/
# Generate release notes for the last commit tagged with Release-*
git-cliff --latest --strip header --config cliff.toml
