# Cleanup baseline

This folder records a secret-free, read-only inventory before the Ecom clone is reduced to a reusable backend starter.

It intentionally does not copy configuration values, connection strings, tokens, certificates, Docker environment values, or application logs.

Baseline policy:

- Source deletion and generated-output cleanup are separate changes.
- `bin`, `obj`, and `logs` are excluded from this initial baseline.
- Each later cleanup phase must update the affected inventory and run a stale-reference scan before its deletion set is accepted.

