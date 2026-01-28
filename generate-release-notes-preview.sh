#!/bin/sh
# requires https://git-cliff.org/
# show what would be in this release if we cut a release now and tagged it

echo 'git-cliff --unreleased --strip header --config cliff.toml' > release-note-preview.md

git-cliff --unreleased --strip header --config cliff.toml | tee release-note-preview.md
