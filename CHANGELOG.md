# Changelog

## [1.1.3] - 2026-07-08

### Added
- SortingDialog now remembers the last used sort options (criteria, order, recursive, case-sensitive, Vietnamese) across invocations.
- Tests covering dialog option state restore and default fallback.

## [1.1.2] - 2026-07-08

### Fixed
- Vietnamese order is now preserved when case-sensitive sorting is enabled.
- Version comparison now normalizes trailing `.0` components, preventing false-positive update prompts.
- `GetNewestVersionTag` uses normalized version comparison to avoid ranking equivalent tags differently.
- `SortingEngine.SortGroup` no longer crashes when passed `null` sorting options.

### Added
- Behavior tests for Vietnamese sorting, descending order, recursive sorting, and all remaining sort criteria (Username, URL, Notes, CreatedTime, ModifiedTime, case sensitivity).
