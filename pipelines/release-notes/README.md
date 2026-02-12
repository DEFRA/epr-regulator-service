# Automated release note generation scripts

[`generate-release-notes.sh`](generate-release-notes.sh) uses [git cliff](https://git-cliff.org/) to parse commit logs and generate release notes from them.

You'll need to have the git-cliff binary on your path to generate the release notes.

If git commits follow "[conventional commit](https://www.conventionalcommits.org/en/v1.0.0/)" conventions with some tweaks we can more easily answer the question of "what is going out" in any given release.

Supported conventions:

1. Prefixes for `feat:`, `fix:` etc to group changes into areas
2. Finding jira ticket urls in any commit and including in release notes
3. Adding a `Fixes <jira_url>` to differentiate between work towards a ticket and a completed feature

See [`cliff.toml`](cliff.toml) for current configuration and supported commit message parsing.

## Usage

Release notes for latest release tag:

```shell
./pipelines/release-notes/generate-release-notes.sh
```

Release notes since latest release tag:

```shell
./pipelines/release-notes/generate-release-notes.sh --preview
```

## Example output

```md
$ ./pipelines/release-notes/generate-release-notes.sh --preview

# Release notes for regulator-frontend

Unreleased changes since [v17.1.0](https://github.com/DEFRA/epr-regulator-service/releases/tag/v17.1.0)

- Diff & commits: [v17.1.0...automatic-release-notes](https://github.com/DEFRA/epr-regulator-service/compare/v17.1.0...automatic-release-notes)
- Permalink: [c548556a...ed0ca0df](https://github.com/DEFRA/epr-regulator-service/compare/c548556a421391cbf4b8a382a6a5e39e38cb5b3a...ed0ca0df4c718e8495885053a534898795d1f156)

## 🚀 Features

- Some feature from a semantic commit `feat: Some feature...` https://eaflood.atlassian.net/browse/AAA-0001

## 📋 Additional changes

- Other changes with ticket info in the commit  https://eaflood.atlassian.net/browse/AAA-0002


## ✅ Completed Tickets

- https://eaflood.atlassian.net/browse/AAA-0003

## 🎫 Related Tickets

- https://eaflood.atlassian.net/browse/AAA-0004
```
