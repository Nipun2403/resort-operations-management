<!-- gitnexus:start -->

# GitNexus — Code Intelligence

This project is indexed by GitNexus as **Hotel_Management_Full** (9979 symbols, 24545 relationships, 300 execution flows). Use the GitNexus MCP tools to understand code, assess impact, and navigate safely.

> Index stale? Run `node .gitnexus/run.cjs analyze` from the project root — it auto-selects an available runner. No `.gitnexus/run.cjs` yet? `npx gitnexus analyze` (npm 11 crash → `npm i -g gitnexus`; #1939).

## Always Do

- **MUST run impact analysis before editing any symbol.** Before modifying a function, class, or method, run `impact({target: "symbolName", direction: "upstream"})` and report the blast radius (direct callers, affected processes, risk level) to the user.
- **MUST run `detect_changes()` before committing** to verify your changes only affect expected symbols and execution flows. For regression review, compare against the default branch: `detect_changes({scope: "compare", base_ref: "main"})`.
- **MUST warn the user** if impact analysis returns HIGH or CRITICAL risk before proceeding with edits.
- When exploring unfamiliar code, use `query({search_query: "concept"})` to find execution flows instead of grepping. It returns process-grouped results ranked by relevance.
- When you need full context on a specific symbol — callers, callees, which execution flows it participates in — use `context({name: "symbolName"})`.
- For security review, `explain({target: "fileOrSymbol"})` lists taint findings (source→sink flows; needs `analyze --pdg`).

## Never Do

- NEVER edit a function, class, or method without first running `impact` on it.
- NEVER ignore HIGH or CRITICAL risk warnings from impact analysis.
- NEVER rename symbols with find-and-replace — use `rename` which understands the call graph.
- NEVER commit changes without running `detect_changes()` to check affected scope.

## Resources

| Resource                                               | Use for                                  |
| ------------------------------------------------------ | ---------------------------------------- |
| `gitnexus://repo/Hotel_Management_Full/context`        | Codebase overview, check index freshness |
| `gitnexus://repo/Hotel_Management_Full/clusters`       | All functional areas                     |
| `gitnexus://repo/Hotel_Management_Full/processes`      | All execution flows                      |
| `gitnexus://repo/Hotel_Management_Full/process/{name}` | Step-by-step execution trace             |

## CLI

| Task                                         | Read this skill file                                        |
| -------------------------------------------- | ----------------------------------------------------------- |
| Understand architecture / "How does X work?" | `.claude/skills/gitnexus/gitnexus-exploring/SKILL.md`       |
| Blast radius / "What breaks if I change X?"  | `.claude/skills/gitnexus/gitnexus-impact-analysis/SKILL.md` |
| Trace bugs / "Why is X failing?"             | `.claude/skills/gitnexus/gitnexus-debugging/SKILL.md`       |
| Rename / extract / split / refactor          | `.claude/skills/gitnexus/gitnexus-refactoring/SKILL.md`     |
| Tools, resources, schema reference           | `.claude/skills/gitnexus/gitnexus-guide/SKILL.md`           |
| Index, status, clean, wiki CLI commands      | `.claude/skills/gitnexus/gitnexus-cli/SKILL.md`             |

<!-- gitnexus:end -->

<!-- serena:start -->

# Serena MCP — Semantic Code Toolkit

This project also has the **Serena MCP** server available. Use Serena's semantic tools (symbol search, find-references, precise symbol-level editing) alongside GitNexus for code understanding and modification.

## Always Do

- Use Serena MCP tools for symbol-level navigation and editing (e.g. finding symbol definitions/references, targeted reads, and precise inserts/replacements) instead of broad manual file edits, especially in large or unfamiliar files.
- Combine Serena's symbol-level view with GitNexus's impact/relationship analysis when planning a change — GitNexus tells you the blast radius, Serena helps you make the edit precisely.
- Prefer Serena's find-references/find-symbol tools over grepping when locating where a symbol is defined or used.

<!-- serena:end -->

<!-- git-workflow:start -->

# Git Commit Workflow

To ensure smooth rollback at any point, commits must be made frequently and granularly.

## Always Do

- **MUST commit after every step, feature update, fix, or meaningful change** — not just at the end of a task. Each logical unit of work gets its own commit.
- **MUST write clear, descriptive commit messages** that state what changed and why, so history can be used for rollback/bisection.
- **MUST commit before starting a risky or exploratory change** so there is always a clean checkpoint to roll back to.
- Run `detect_changes()` (per the GitNexus workflow above) before each commit to confirm the change only touches the expected scope.

## Never Do

- NEVER batch multiple unrelated changes into a single commit.
- NEVER leave uncommitted work at the end of a step, feature, or fix — commit before moving on.

<!-- minimal-change-discipline:start -->

# Minimal Change & Planning Discipline

## Always Do

- **MUST run GitNexus and Serena MCP tools before doing anything** — before writing or editing a single line, use GitNexus (`query`, `context`, `impact`) and Serena (symbol search / find-references) to fully understand the code first.
- **MUST use GitNexus and Serena extensively during the planning phase**, before touching any code, to identify the exact symbols, files, and lines that need to change — the plan must be pinpoint, not approximate.
- **MUST implement the minimal amount of change required** to satisfy the user's request — touch only the specific files, functions, and lines that are strictly necessary; do not refactor, rename, reformat, or "improve" unrelated code along the way.
- Use `impact` and Serena's reference search together to confirm the smallest possible edit surface before making any change, and re-confirm with `detect_changes()` afterward that only the intended, minimal scope was affected.

## Never Do

- NEVER start editing before completing a GitNexus + Serena planning pass.
- NEVER make broader changes than what is strictly required to fulfill the request, even if other issues are noticed along the way — note them for the user instead of fixing them unprompted.
<!-- minimal-change-discipline:end -->

<!-- ponytail-subagents:start -->

# Ponytail & Subagents

This project also uses **Ponytail** (https://github.com/DietrichGebert/ponytail) — a ruleset that makes the agent write the least code necessary, reinforcing the Minimal Change & Planning Discipline above.

## Always Do

- **MUST apply the Ponytail decision ladder before writing any code**, in order, stopping at the first rung that holds:
  1. Does this need to exist at all? → if no, skip it (YAGNI)
  2. Is it already in this codebase? → reuse it, don't rewrite
  3. Does the stdlib do it? → use it
  4. Is there a native platform feature? → use it
  5. Is there an installed dependency that does it? → use it
  6. Can it be done in one line? → one line
  7. Only if none of the above apply → write the minimum code that works
- **MUST use subagents** for exploration, search, and read-only investigation work (e.g. broad codebase exploration, wide searches, isolated verification tasks) to keep the main context focused and to parallelize investigation before edits are made.
- **MUST keep Ponytail's ruleset active for any subagent spawned**, so subagents follow the same minimal-code, minimal-edit discipline as the main agent — do not let subagents over-build or over-edit just because they operate in an isolated context.
- Never trade away validation, error handling, security, or accessibility for the sake of brevity — Ponytail is lazy about the solution, never about correctness or safety.

<!-- ponytail-subagents:end -->

<!-- git-workflow:end -->

