#!/bin/sh
# Upgrade dotnet to the latest SDK release of $target_major, moving every
# place this repo pins the version in step:
#  - .tool-versions (local SDK via mise/asdf)
#  - <TargetFramework> in every csproj
#  - the defradigital/dotnetcore* base images in the Dockerfile (latest
#    release for the major, so also run this to pick up patched images)
#  - dotnetVersion in the Azure DevOps pipeline
set -e # exit on error

# Capped rather than "latest" because the build can only use a major once
# DEFRA's shared tooling supports it: the dotnetVersionN values in
# https://github.com/DEFRA/epr-webapps-code-deploy-templates/blob/be9c7ff6175b5625ce473f82053ab9739a0a2a0f/templates/sdk-script.yaml#L11-L20
# and the dotnetN.0 tags of the base images:
# https://hub.docker.com/r/defradigital/dotnetcore/tags
# https://hub.docker.com/r/defradigital/dotnetcore-development/tags
# Bump this when both support the next major.
target_major=10

if ! git diff --cached --quiet; then
  echo "Aborted: staged changes detected in git"
  exit 1
fi

latest=$(mise ls-remote dotnet-core "$target_major" | grep -P '^\d+\.\d+\.\d+$' | sort --version-sort | tail -n 1)
if [ -z "$latest" ]; then
  echo "Aborted: couldn't find any dotnet-core $target_major.x releases"
  exit 1
fi
echo "Updating to dotnet-core@$latest"

target_framework="net${target_major}.0"
echo "Updating TargetFramework to $target_framework"

sed -i "s|^dotnet-core .*|dotnet-core $latest|" .tool-versions
find src -name '*.csproj' -not -path '*/bin/*' -not -path '*/obj/*' -exec \
  sed -i "s|<TargetFramework>net[0-9]*\.0</TargetFramework>|<TargetFramework>${target_framework}</TargetFramework>|g" {} +

# Base images are pinned to an exact release (e.g. 2.1.8-dotnet10.0) rather
# than the floating dotnetN.0 tag, so a patched image is an explicit change
# instead of a cached layer with known CVEs lingering in builds.
for image in dotnetcore dotnetcore-development; do
  # Registry API needs an anonymous pull token even for public images
  token=$(curl -sf "https://auth.docker.io/token?service=registry.docker.io&scope=repository:defradigital/$image:pull" | jq -r .token)
  tag=$(curl -sf -H "Authorization: Bearer $token" "https://registry-1.docker.io/v2/defradigital/$image/tags/list" \
    | jq -r .tags[] | grep -E "^[0-9]+\.[0-9]+\.[0-9]+-dotnet${target_major}\.0$" | sort --version-sort | tail -n 1)
  if [ -z "$tag" ]; then
    echo "Aborted: couldn't find a defradigital/$image release for dotnet $target_major"
    exit 1
  fi
  echo "Updating base image to defradigital/$image:$tag"
  sed -i "s|defradigital/$image:[^ ]*|defradigital/$image:$tag|g" src/EPR.RegulatorService.Frontend.Web/Dockerfile
done
sed -i "s|value: dotnetVersion[0-9]*|value: dotnetVersion${target_major}|" pipelines/ci_pipeline.yaml

if git diff --quiet -- .tool-versions src pipelines; then
  echo "Already on latest dotnet, nothing to do"
  exit 0
fi

mise install

echo "Running regression tests..."
(cd src && dotnet test)

echo "Committing upgrades..."
# shellcheck disable=SC2046
git commit $(git diff --name-only -- .tool-versions src pipelines | sed 's/^/--include /') \
--message "chore: Upgrade build to dotnet $latest" \
--message "Done with 'upgrade-dotnet.sh'"
