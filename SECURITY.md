# Security Policy

## Supported Versions

| Version | Supported |
|---------|-----------|
| 1.0.x   | Yes       |

## Reporting a Vulnerability

If you discover a security vulnerability in this package, please report it responsibly:

1. **Do not** open a public GitHub issue
2. Email: **security@bizsim.com**
3. Include: package name, version, description of the vulnerability, and steps to reproduce

We will acknowledge your report within 48 hours and provide a fix timeline within 7 days.

## Scope

This package wraps the Google Play In-App Review API with the following security considerations:

- **Review flow** is launched via local IPC to Google Play — no network calls are made by this package
- **No user content** is collected, transmitted, or stored by this package — the review UI is rendered by the Play Store app and review text is submitted directly by the user to Google
- **Local cooldown state** (last-review timestamp) is stored in `PlayerPrefs` — no personally identifying information is persisted
- **ProGuard rules** are embedded to prevent reverse engineering of the Java bridge
