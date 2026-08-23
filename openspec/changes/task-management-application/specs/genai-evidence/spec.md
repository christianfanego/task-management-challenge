# GenAI Evidence Specification

## Purpose

Define truthful, constrained evidence of GenAI use for the review-ready application.

## Requirements

### Requirement: Record only real GenAI evidence
`docs/genai-evidence.md` MUST contain only real prompts, representative outputs, accepted or rejected suggestions, human corrections, validation, tests, edge cases, authentication decisions, and final assessment. It MUST NOT contain fabricated transcripts or unrun command claims.

#### Scenario: Evidence supports a real interaction (GENAI-001)
- GIVEN an actual prompt and representative output used during work
- WHEN the evidence document records it
- THEN it identifies context, acceptance/rejection, correction, and validation or unavailable verification

#### Scenario: Evidence records rejection and correction (GENAI-002)
- GIVEN a suggestion that is rejected or corrected
- WHEN the evidence records the decision
- THEN it explains the reason, safer final decision, and verification

### Requirement: Final assessment is reviewable
The evidence MUST assess GenAI help, human control of scope/security/architecture, and claims blocked by missing .NET, runners, frontend quality tooling, or browser runtime.

#### Scenario: Assess incomplete verification honestly (GENAI-003)
- GIVEN unavailable development or verification tooling
- WHEN the final assessment is written
- THEN it distinguishes intended behavior from unavailable verification

### Requirement: Evidence acceptance is testable
The documentation review MUST verify provenance for every entry and distinguish expected commands from commands actually run.

#### Scenario: Reject fabricated evidence (GENAI-004)
- GIVEN an entry with no real provenance
- WHEN the evidence document is reviewed
- THEN the entry is removed or rejected before release

#### Scenario: Verify GenAI evidence document (GENAI-DOC-001)
- GIVEN `docs/genai-evidence.md` at the repository root
- WHEN a reviewer performs a manual/structural review
- THEN it contains only real evidence, corrections, validation, and honest unavailable-tool claims

## Acceptance Test Mapping

| Scenario ID | Intended project/suite | Working directory | Framework/tool | Review status |
|---|---|---|---|---|
| GENAI-001 | `docs/genai-evidence.md` review checklist | repository root | Manual/structural review | Manual/structural review, not an automated test; verify provenance and constrained content |
| GENAI-002 | `docs/genai-evidence.md` review checklist | repository root | Manual/structural review | Manual/structural review, not an automated test; verify rejection and correction evidence |
| GENAI-003 | `docs/genai-evidence.md` review checklist | repository root | Manual/structural review | Manual/structural review, not an automated test; verify honest unavailable-tool claims |
| GENAI-004 | `docs/genai-evidence.md` review checklist | repository root | Manual/structural review | Manual/structural review, not an automated test; reject entries without provenance |
| GENAI-DOC-001 | `docs/genai-evidence.md` review checklist | repository root | Manual/structural review | Manual/structural review, not an automated test; inspect the exact scenario checklist |
