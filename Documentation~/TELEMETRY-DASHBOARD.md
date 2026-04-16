# Telemetry Dashboard Setup

Firebase Analytics dashboard configuration for the `bizsim_review_*` event
family logged by `FirebaseReviewAnalyticsAdapter` (shipped in the Firebase
Adapter sample).

---

## Event catalog

All event names are **frozen** -- do not rename them. Firebase DebugView,
BigQuery exports, and downstream dashboards depend on the exact strings.

| Event name | Fired when | Key parameters |
|---|---|---|
| `bizsim_review_trigger_evaluated` | Trigger engine produces a decision | `decision`, `reason`, `session_count`, `trigger_reason` |
| `bizsim_review_preload_started` | `PreloadReviewInfo()` called | `session_count` |
| `bizsim_review_preload_succeeded` | Preload completed successfully | `session_count` |
| `bizsim_review_preload_failed` | Preload failed | `error_code`, `session_count` |
| `bizsim_review_flow_requested` | Review flow requested (past trigger gate) | `session_count`, `trigger_reason`, `variant_id`, `days_since_install` |
| `bizsim_review_flow_completed` | Review flow completed (quota-invisible) | `source`, `elapsed_ms`, `session_count`, `trigger_reason`, `variant_id`, `days_since_install` |
| `bizsim_review_error` | Review flow error | `error_code`, `retryable`, `session_count`, `trigger_reason` |
| `bizsim_review_killswitch_blocked` | Kill switch blocked the request | `session_count`, `app_version` |
| `bizsim_review_consent_blocked` | Consent gate blocked the request | `session_count` |
| `bizsim_review_offline_blocked` | Offline guard blocked the request | `session_count` |
| `bizsim_review_cooldown_blocked` | Cooldown (local) blocked the request | `session_count`, `days_since_install` |

---

## Funnel diagram

```
                    +---------------------------+
                    | trigger_evaluated (Allow)  |
                    +---------------------------+
                                |
                    +-----------v-----------+
                    |   preload_started     |  (optional)
                    +-----------+-----------+
                                |
                    +-----------v-----------+
                    |   preload_succeeded   |
                    +-----------+-----------+
                                |
                    +-----------v-----------+
                    |   flow_requested      |
                    +-----------+-----------+
                                |
                    +-----------v-----------+
                    |   flow_completed      |
                    +---------------------------+

  Block branches (exit from trigger_evaluated):

                    +---------------------------+
                    | trigger_evaluated (Block)  |
                    +-------------+-------------+
                                  |
            +---------------------+---------------------+
            |                     |                     |
  +---------v---------+ +---------v---------+ +---------v---------+
  | killswitch_blocked| | consent_blocked   | | offline_blocked   |
  +-------------------+ +-------------------+ +-------------------+

            +---------------------+
            |                     |
  +---------v---------+ +---------v---------+
  | cooldown_blocked  | |      error        |
  +-------------------+ +-------------------+
```

---

## Firebase dashboard JSON template

Paste into Firebase Console > Analytics > Custom Definitions, or import via
the Firebase Management API. This creates a custom audience and four
exploration reports.

### Custom audience: Review Flow Users

```json
{
  "displayName": "Review Flow Users",
  "description": "Users who triggered a review flow evaluation",
  "eventFilter": [
    {
      "eventName": "bizsim_review_trigger_evaluated"
    }
  ]
}
```

### BigQuery SQL: Review funnel conversion

```sql
-- Review funnel: trigger -> preload -> request -> complete
-- Run against the Firebase BigQuery export (analytics_YYYYMMDD tables)

WITH events AS (
  SELECT
    user_pseudo_id,
    event_name,
    event_timestamp,
    (SELECT value.string_value FROM UNNEST(event_params) WHERE key = 'decision') AS decision,
    (SELECT value.string_value FROM UNNEST(event_params) WHERE key = 'trigger_reason') AS trigger_reason,
    (SELECT value.string_value FROM UNNEST(event_params) WHERE key = 'variant_id') AS variant_id,
    (SELECT value.int_value FROM UNNEST(event_params) WHERE key = 'session_count') AS session_count
  FROM `YOUR_PROJECT.analytics_YOUR_DATASET.events_*`
  WHERE event_name LIKE 'bizsim_review_%'
    AND _TABLE_SUFFIX BETWEEN FORMAT_DATE('%Y%m%d', DATE_SUB(CURRENT_DATE(), INTERVAL 30 DAY))
                           AND FORMAT_DATE('%Y%m%d', CURRENT_DATE())
)

SELECT
  COUNT(DISTINCT CASE WHEN event_name = 'bizsim_review_trigger_evaluated' AND decision = 'Allow' THEN user_pseudo_id END) AS trigger_allow,
  COUNT(DISTINCT CASE WHEN event_name = 'bizsim_review_preload_started' THEN user_pseudo_id END) AS preload_started,
  COUNT(DISTINCT CASE WHEN event_name = 'bizsim_review_preload_succeeded' THEN user_pseudo_id END) AS preload_succeeded,
  COUNT(DISTINCT CASE WHEN event_name = 'bizsim_review_flow_requested' THEN user_pseudo_id END) AS flow_requested,
  COUNT(DISTINCT CASE WHEN event_name = 'bizsim_review_flow_completed' THEN user_pseudo_id END) AS flow_completed,
  COUNT(DISTINCT CASE WHEN event_name = 'bizsim_review_error' THEN user_pseudo_id END) AS errors
FROM events
```

### BigQuery SQL: Block reason breakdown

```sql
-- Block reason distribution (last 30 days)

SELECT
  event_name,
  COUNT(*) AS occurrences,
  COUNT(DISTINCT user_pseudo_id) AS unique_users
FROM `YOUR_PROJECT.analytics_YOUR_DATASET.events_*`
WHERE event_name IN (
  'bizsim_review_killswitch_blocked',
  'bizsim_review_consent_blocked',
  'bizsim_review_offline_blocked',
  'bizsim_review_cooldown_blocked'
)
AND _TABLE_SUFFIX BETWEEN FORMAT_DATE('%Y%m%d', DATE_SUB(CURRENT_DATE(), INTERVAL 30 DAY))
                       AND FORMAT_DATE('%Y%m%d', CURRENT_DATE())
GROUP BY event_name
ORDER BY occurrences DESC
```

### BigQuery SQL: A/B variant comparison

```sql
-- Compare review funnel by variant_id (requires passing variantId to RequestReview)

WITH events AS (
  SELECT
    user_pseudo_id,
    event_name,
    (SELECT value.string_value FROM UNNEST(event_params) WHERE key = 'variant_id') AS variant_id
  FROM `YOUR_PROJECT.analytics_YOUR_DATASET.events_*`
  WHERE event_name IN ('bizsim_review_flow_requested', 'bizsim_review_flow_completed')
    AND _TABLE_SUFFIX BETWEEN FORMAT_DATE('%Y%m%d', DATE_SUB(CURRENT_DATE(), INTERVAL 30 DAY))
                           AND FORMAT_DATE('%Y%m%d', CURRENT_DATE())
)

SELECT
  variant_id,
  COUNT(DISTINCT CASE WHEN event_name = 'bizsim_review_flow_requested' THEN user_pseudo_id END) AS requested,
  COUNT(DISTINCT CASE WHEN event_name = 'bizsim_review_flow_completed' THEN user_pseudo_id END) AS completed,
  SAFE_DIVIDE(
    COUNT(DISTINCT CASE WHEN event_name = 'bizsim_review_flow_completed' THEN user_pseudo_id END),
    COUNT(DISTINCT CASE WHEN event_name = 'bizsim_review_flow_requested' THEN user_pseudo_id END)
  ) AS completion_rate
FROM events
WHERE variant_id IS NOT NULL AND variant_id != ''
GROUP BY variant_id
ORDER BY requested DESC
```

---

## Setup instructions

1. **Enable BigQuery export** in Firebase Console > Project Settings > Integrations.
2. **Import the Firebase Adapter sample** from Package Manager.
3. **Wire the adapter** in your bootstrap script:
   ```csharp
   ReviewController.Instance.SetAnalyticsAdapter(new FirebaseReviewAnalyticsAdapter());
   ```
4. **Verify events** in Firebase DebugView (enable debug mode on device):
   ```bash
   adb shell setprop debug.firebase.analytics.app YOUR_PACKAGE_NAME
   ```
5. **Create the BigQuery queries** above, replacing `YOUR_PROJECT` and `YOUR_DATASET`
   with your Firebase project's BigQuery dataset identifiers.
6. Optionally create a **Looker Studio** dashboard connected to these queries for
   ongoing monitoring.
