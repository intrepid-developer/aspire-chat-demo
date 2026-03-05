---
description: Run a full code review for a commit or recent branch changes
agent: code-review-agent
subtask: true
---
Perform a full code review.

Target:

- If `$ARGUMENTS` is provided, review that commit/range (examples: `HEAD`, `abc123`, `main...HEAD`).
- If no arguments are provided, review the latest commit (`HEAD`).

Review checklist:

1. Correctness and logic issues.
2. Security/auth/input validation concerns.
3. Performance and scalability risks.
4. API/contract compatibility and regression risk.
5. Test coverage gaps and missing assertions.
6. Code quality and maintainability.

Output:

- Overall risk: low/medium/high
- Findings grouped by severity (high, medium, low)
- File references for each finding
- Recommended fixes and suggested test additions
- Brief positives/strengths
