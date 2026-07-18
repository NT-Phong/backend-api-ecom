# Commerce Domain Guidance

- Treat each aggregate root as the only mutation boundary for its state and children.
- Keep one entity per file, private EF constructors, guarded factories, and domain methods for transitions.
- Keep cross-aggregate references ID-based and avoid large bidirectional navigation graphs.
- Use stable `CommerceDomainException.Code` values and emit events only after valid mutations.
- Add or update focused Domain tests for every invariant or state transition changed.
- Do not introduce EF, HTTP, repository, or infrastructure concerns into Domain.
