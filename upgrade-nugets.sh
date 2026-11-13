#!/bin/sh
# Upgrade nuget packages to latest, then run the tests and commit.
# Run upgrade-dotnet.sh first so the dotnet major is current.
set -e # exit on error

if ! git diff --cached --quiet; then
  echo "Aborted: staged changes detected in git"
  exit 1
fi

if ! command -v dotnet-outdated
then
	# https://github.com/dotnet-outdated/dotnet-outdated
	dotnet tool install --global dotnet-outdated-tool
fi

# Packages versioned with the runtime are capped at its major. Many also
# target netstandard, so dotnet-outdated would otherwise offer e.g. 11.x
# on net10.0. The maximum is inclusive and needs at least major.minor, so
# a huge minor means any 10.x; omitted parts are treated as unbounded.
dotnet_major=$(grep '^dotnet-core ' .tool-versions | awk '{print $2}' | cut -d. -f1)

# --no-restore because dotnet-outdated otherwise restores each project as it
# edits it, and a project restored before the projects it references are
# edited fails with a spurious downgrade (NU1605). dotnet test below
# restores the finished result.
cd src
dotnet outdated -u --no-restore --maximum-version "$dotnet_major.99999" \
  -inc Microsoft.AspNetCore -inc Microsoft.Extensions Epr.RegulatorService.sln
# Held back:
# - ApplicationInsights 3 stops the app starting: the logging that feeds it,
#   Serilog.Sinks.ApplicationInsights and
#   Microsoft.Extensions.Logging.ApplicationInsights (no 3.x release), both
#   require Microsoft.ApplicationInsights [2.23.0, 3.0.0).
#   https://www.nuget.org/packages/Serilog.Sinks.ApplicationInsights#dependencies-body-tab
#   https://www.nuget.org/packages/Microsoft.Extensions.Logging.ApplicationInsights
# - FluentValidation, see the pin comment in the Web csproj.
# - CsvHelper 34 is a rewrite (312 breaking changes) with no security fix
#   over 33, and the CSV export is user-facing.
#   https://www.nuget.org/packages/CsvHelper/34.0.0
# Moving any of these needs code changes, so is left for a separate change.
dotnet outdated -u --no-restore -exc Microsoft.AspNetCore -exc Microsoft.Extensions \
  -exc Microsoft.ApplicationInsights -exc FluentValidation -exc CsvHelper Epr.RegulatorService.sln
cd ..

if git diff --quiet -- 'src/*.csproj' src/Directory.Build.props; then
  echo "No package updates available, nothing to do"
  exit 0
fi

echo "Running regression tests..."
(cd src && dotnet test)

echo "Committing upgrades..."
# shellcheck disable=SC2046
git commit $(git diff --name-only -- 'src/*.csproj' src/Directory.Build.props | sed 's/^/--include /') \
--message "chore: Dependency upgrades (nuget)" \
--message "Done with 'upgrade-nugets.sh'"
