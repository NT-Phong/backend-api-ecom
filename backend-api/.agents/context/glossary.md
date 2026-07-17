# Glossary

## Architecture Terms
- CQRS: Separate command and query request models handled by MediatR.
- Command: A request that changes state.
- Query: A request that reads state.
- Handler: MediatR handler that executes a command or query.
- Validator: FluentValidation class that validates a command/query before handler execution.
- UnitOfWork: Transaction and repository boundary for persistence.
- `[EnableUnitOfWork]`: Attribute that enables the UnitOfWork behavior for a request.
- `TResult`: Project result wrapper for success, handled failures, and validation failures.
- `ApiResponse`: API response envelope returned by controllers.

## Domain Terms
- Zone: Operational area grouping ponds and related resources.
- Pond: Aquaculture pond managed within a zone/season.
- Season: Production or farming season.
- DeviceHub: IoT hub/gateway that connects device telemetry.
- WarningDeviceHub: Hub for warning devices.
- SensorDevice: Sensor device reporting telemetry.
- Scale: Physical weighing device.
- ScaleSession: Active weighing session; confirmed rule is one physical scale has one active weighing session.
- ScaleRecord: Stored weighing record.
- Oxy alert: Oxygen-related pond alert flow.

## Realtime Terms
- `ScaleSession_{sessionId}`: Primary SignalR group for an active weighing screen.
- `Scale_{scaleId}`: SignalR group for scale detail screens.
- `Device_{iotDeviceId}`: Diagnostic/backward compatibility group unless a feature explicitly requires it.
- Code 503 telemetry: Scale live-weight telemetry packet referenced by prior scale optimization reports.
