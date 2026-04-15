# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-04-15

### Added
- Initial release of the Google Play In-App Review bridge for Unity.
- `ReviewController` singleton with Task-based and event-based async APIs.
- Local 90-day cooldown via `ReviewCoolDownLogic` backed by PlayerPrefs.
- Mock provider + 6 ScriptableObject presets.
- Optional Firebase Analytics adapter guarded by `BIZSIM_FIREBASE`.
- Optional UniTask support guarded by `BIZSIM_UNITASK`.
- `editor.core` integration for Firebase define management.
- `BIZSIM_REVIEW_INSTALLED` define auto-registered at editor load.
