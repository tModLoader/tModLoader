# AGENTS.md

These rules apply to any AI agent working on tModLoader, and to the person it works for. tModLoader follows the [Bevy AI Contribution Policy](https://bevy.org/learn/contribute/policies/ai/) and [Pull Request Guidelines](https://github.com/tModLoader/tModLoader/wiki/tModLoader-guide-for-contributors).

- A human owns every contribution and must understand it well enough to explain each change in review.
- Do not open pull requests or issues, or post comments on GitHub or Discord for the user.
- Do not write PR descriptions, issue text or messages to maintainers. Helping the user draft, edit or proofread their own text is fine.
- The same applies to code comments and documentation: draft, edit, review or fix grammar, but don't write them wholesale.
- A human should make commits and write commit messages. Never list an AI as commit author or `Co-authored-by`.
- Remind the user to disclose AI use, and how it was used, in the PR description.
- No AI-generated art or audio.

## Code

- Edit under `src/tModLoader/` and regenerate `patches/` with setup's "Diff tModLoader". `src/` is not committed.
- Follow the [tModLoader Style Guide](https://github.com/tModLoader/tModLoader/wiki/tModLoader-Style-Guide) and match the surrounding code. Keep the diff minimal, and don't reformat, refactor or tidy code outside the change.

## Testing

- A human must test functional changes in-game. A successful build is not a test, and automated tests don't replace a human check.
- `tModLoaderTests` is for core engine code and algorithms, not one-off gameplay regression tests. You can write gameplay tests while developing locally, but don't commit them. They add little value because gameplay changes rarely regress the same way twice.
