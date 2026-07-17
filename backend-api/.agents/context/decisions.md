# Architecture Decisions

## Canonical Agent Memory
Decision: Use `.agents/skills` as the canonical skill and prior-analysis folder.

Implication: Do not use `.github/skills` as the primary source. Existing references should point to `.agents/skills`.

## Progressive Context Loading
Decision: Keep `AGENTS.md` short and use layered memory.

Implication:
- Global behavior belongs in `.agents/rules/`.
- Project facts belong in `.agents/context/`.
- Task-specific procedure and prior reports belong in `.agents/skills/`.
- Examples and scripts are optional support, not startup context.

## Backend Architecture
Decision: Preserve Clean Architecture + CQRS + MediatR + UnitOfWork.

Implication:
- Controllers remain thin.
- Application handlers orchestrate use cases.
- Domain owns invariants.
- Infrastructure implements persistence, IoT, cache, messaging, and security.

## Scale Realtime
Decision: One scale weighing session maps to one physical scale and one active session.

Implication:
- Use `ScaleSession_{sessionId}` as the primary realtime group for weighing screens.
- Use `Scale_{scaleId}` for scale detail screens.
- Treat `Device_{iotDeviceId}` as diagnostics/backward compatibility unless explicitly required.
- Keep `CycleId` and `ScaleSessionId` separate in contracts.
- Avoid database reads on every Code 503 telemetry packet; use realtime context cache.
- Target active live weighing updates at roughly `1s`.
- On successful SignalR subscribe, send the latest Redis snapshot immediately.

## Protected Logout Flow
Decision: Logout validator and Logout handler are protected project workflow areas.

Implication: Do not modify them unless the user explicitly asks.
