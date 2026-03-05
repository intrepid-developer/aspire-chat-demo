---
description: Create a git commit with a sensible message and concise details
agent: orchestrator
---
Create a commit for the current working tree.

Requirements:

1. Inspect `git status`, `git diff`, and recent commit messages.
2. Stage relevant tracked/untracked files for the current change.
3. Do not stage likely secrets (`.env`, credential files, tokens, keys).
4. Write a sensible commit message that explains intent (why), not only mechanics.
5. Include concise body details when helpful (what changed and impact).
6. Run the commit.
7. Report:
   - commit hash
   - commit title
   - staged files included
   - any files intentionally excluded

If there are no changes, report that clearly and do not create an empty commit.
