# Optimized Workflow

```text
Plan -> Implement one bounded batch -> Verify gate -> Report -> Update durable Commerce status
```

Start from route/entity/error, trace the full boundary, inspect one working backend pattern, make the smallest safe change, and verify behavior. Ask only for choices affecting public API, schema, auth, payments, inventory policy, migrations, dependencies, or product intent.
