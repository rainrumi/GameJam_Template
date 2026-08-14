#!/usr/bin/env python3
from __future__ import annotations

import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

REQUIRED = [
    "AGENTS.md",
    "README.md",
    "ai-coding-profile/README.md",
    "ai-coding-profile/coding-style.md",
    "ai-coding-profile/style-profile.json",
    "ai-coding-profile/exemplars.json",
    "docs/AI_EXECUTION_CONTRACT.md",
    "docs/DESIGN_RATIONALE.md",
    "docs/UNITY_CLI_RUNBOOK.md",
    "docs/VERIFICATION_MATRIX.md",
    "skills/unity-implementation-planner/SKILL.md",
    "skills/unity-implementation-executor/SKILL.md",
]

SKILLS = [
    ("skills/unity-implementation-planner/SKILL.md", "unity-implementation-planner"),
    ("skills/unity-implementation-executor/SKILL.md", "unity-implementation-executor"),
]

FORBIDDEN_STALE_TOKENS = [
    "mcp__uLoopMCP__",
    "mcp__uLoopMCP__compile",
    "mcp__uLoopMCP__get_logs",
]

LINK_RE = re.compile(r"\[[^\]]+\]\(([^)]+)\)")


def validate_required(errors: list[str]) -> None:
    for rel in REQUIRED:
        if not (ROOT / rel).is_file():
            errors.append(f"missing required file: {rel}")


def validate_skill_frontmatter(errors: list[str]) -> None:
    for rel, expected_name in SKILLS:
        path = ROOT / rel
        if not path.is_file():
            continue
        text = path.read_text(encoding="utf-8")
        if not text.startswith("---\n"):
            errors.append(f"{rel}: missing YAML frontmatter")
            continue
        try:
            _, frontmatter, _ = text.split("---", 2)
        except ValueError:
            errors.append(f"{rel}: malformed YAML frontmatter")
            continue
        if f"name: {expected_name}" not in frontmatter:
            errors.append(f"{rel}: expected name: {expected_name}")
        if "description:" not in frontmatter:
            errors.append(f"{rel}: missing description")


def validate_markdown_links(errors: list[str]) -> None:
    for path in ROOT.rglob("*.md"):
        text = path.read_text(encoding="utf-8")
        for target in LINK_RE.findall(text):
            if target.startswith(("http://", "https://", "#", "mailto:")):
                continue
            clean = target.split("#", 1)[0]
            if not clean:
                continue
            resolved = (path.parent / clean).resolve()
            try:
                resolved.relative_to(ROOT.resolve())
            except ValueError:
                errors.append(f"{path.relative_to(ROOT)}: link escapes pack: {target}")
                continue
            if not resolved.exists():
                errors.append(f"{path.relative_to(ROOT)}: broken relative link: {target}")


def validate_no_uloop_mcp(errors: list[str]) -> None:
    for path in list(ROOT.rglob("*.md")) + list(ROOT.rglob("*.py")):
        if path.name == Path(__file__).name:
            continue
        text = path.read_text(encoding="utf-8")
        for token in FORBIDDEN_STALE_TOKENS:
            if token in text:
                errors.append(f"{path.relative_to(ROOT)}: stale uLoop MCP token found: {token}")


def main() -> int:
    errors: list[str] = []
    validate_required(errors)
    validate_skill_frontmatter(errors)
    validate_markdown_links(errors)
    validate_no_uloop_mcp(errors)

    if errors:
        print("Agent pack validation failed:")
        for error in errors:
            print(f"- {error}")
        return 1

    print("Agent pack validation passed.")
    print(f"Root: {ROOT}")
    print(f"Required files: {len(REQUIRED)}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
