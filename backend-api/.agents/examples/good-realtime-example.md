# Good Realtime Example

Scale realtime should preserve the established group and cache model.

```text
Code 503 telemetry
-> IoTConnectionManager
-> ScaleMeasurementTelemetryHandler
-> ScaleCacheService latest snapshot
-> TelemetryNotificationService
-> ScaleSession_{sessionId}
```

## Rules
- Use `ScaleSession_{sessionId}` as the primary active weighing group.
- Keep `Scale_{scaleId}` and `Cycle_{cycleId}` as secondary groups when needed.
- Treat `Device_{iotDeviceId}` as diagnostics/backward compatibility unless a feature needs it.
- Replay the latest Redis snapshot on subscribe when available.
- Avoid database reads on every telemetry packet; use realtime context cache and short fallback TTLs.
