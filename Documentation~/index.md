# BizSim Google Play In-App Review

Last reviewed: 2026-04-16

## Overview

This package provides a production-ready Unity bridge for the Google Play In-App Review API
(v2.0.2). It wraps the native `ReviewManager` via a JNI bridge, exposes a main-thread-safe
singleton controller, and ships with a smart trigger engine, configurable cooldown, GDPR
consent gate, diagnostic snapshots, and a mock provider for editor testing.

The package compiles only for Android and Editor platforms. On non-Android builds and in the
Unity Editor, the mock provider is used automatically so you can iterate without a device.

## Contents

| File | Description |
|------|-------------|
| [getting-started.md](getting-started.md) | Step-by-step installation and first API call |
| [api-reference.md](api-reference.md) | Full public API surface with types, methods, and parameters |
| [configuration.md](configuration.md) | ReviewSettings asset fields and Editor window walkthrough |
| [architecture.md](architecture.md) | JNI bridge diagram, thread model, provider selection |
| [troubleshooting.md](troubleshooting.md) | Common errors with root causes and fixes |
| [DATA_SAFETY.md](DATA_SAFETY.md) | Play Store Data Safety form input |

## Additional documentation

| File | Description |
|------|-------------|
| [TELEMETRY-DASHBOARD.md](TELEMETRY-DASHBOARD.md) | Firebase Analytics event reference and dashboard setup |
| [UPGRADE-1.x.md](UPGRADE-1.x.md) | Migration guide for 1.x releases |

## Links

- [README](../README.md) — Quick-start experience and feature overview
- [CHANGELOG](../CHANGELOG.md) — Release history
- [GitHub Repository](https://github.com/BizSim-Game-Studios/com.bizsim.google.play.review)
