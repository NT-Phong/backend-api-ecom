# Commerce Persistence Guidance

- Keep one EF configuration per file and preserve explicit relationship/delete behavior.
- Prove relational constraints, indexes, concurrency, and migrations with PostgreSQL rather than EF InMemory.
- Review access paths before adding indexes; prefer partial active-row indexes where justified.
- Never rewrite a shared/applied migration for enum or schema changes.
- Do not generate or apply a migration without explicit approval and the Commerce roadmap gates.
- For any approved migration, inspect data mapping, destructive SQL, down behavior, and snapshot consistency.
