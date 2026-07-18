# Agent-System Forward Test - 2026-07-18

Three independent read-only agents received realistic prompts and the project-local skill name. They did not receive the rubric, expected answer, or prior diagnosis.

| Case | Score | Result |
| --- | ---: | --- |
| CreateOrder inventory concurrency review | 12/12 | Correct Commerce routing, exact source evidence, transaction/oversell analysis, PostgreSQL test requirements, and explicit verification gaps |
| Existing backend API flow review | 12/12 | Selected and traced registration end-to-end, found evidence-backed transaction, PII, OTP, race, and role issues, and listed intentionally excluded context |
| Effective-price exclusion-constraint plan | 11/12 | Preserved migration gates and produced data-precheck/down/test design; loaded slightly more planning/status context than the minimum |

Scoring used `eval-rubric.md`: Routing, Discovery, Scope, Evidence, Verification, and Safety, each from 0 to 2. All cases had full Scope and Safety scores.

## Observed Strengths

- Agents started from root guidance, router, and the correct primary skill.
- Live source and current diff overrode roadmap assumptions.
- Unimplemented Application/API/concurrency work remained explicitly unverified.
- Read-only tasks made no file changes and did not cross migration or configuration gates.
- Reports included exact files, lines, command/test gaps, and intentionally excluded context.

## Follow-up Signal

The migration case shows that planning tasks can still load several related references. Keep monitoring context count in future evals; tighten the router only if repeated runs show unnecessary loading without better correctness.

These results validate three cases, not the entire 12-case suite. Run remaining cases as the corresponding feature work begins and preserve their raw task outputs.
