# UnityCLI AI Agent Contract Pack

Unity CLI / Unity Pipeline を前提に、AI coding agent の **意思決定 → 実装 → Unity 検証 → 証拠ベース報告**を契約化するためのファイル一式。

## Contents

```text
AGENTS.md
ai-coding-profile/
  README.md
  coding-style.md
  evidence.md
  exemplars.json
  style-profile.json
  style-profile.schema.json
docs/
  AI_EXECUTION_CONTRACT.md
  DESIGN_RATIONALE.md
  UNITY_CLI_RUNBOOK.md
  VERIFICATION_MATRIX.md
skills/
  unity-implementation-planner/SKILL.md
  unity-implementation-executor/SKILL.md
scripts/
  validate_agent_pack.py
```

## Design goals

- 先輩の設計から「編集だけで完了しない」「Unity 上で検証する」「ツール出力を証拠として確認する」「最終報告に未検証事項を残す」という AI Execution Contract を継承。
- uLoop 固有 command は持ち込まず、Unity 公式 Unity CLI / `com.unity.pipeline` の live Editor command discovery に置換。
- Unity CLI が experimental で更新される前提から、固定 command catalog より `unity --help` / `unity command --format json` を source of truth にする。
- `ai-coding-profile` の `strict_author_style_replication` を AGENTS 内の具体的な coding contract として再現。
- planner と executor の 2 skill へ責務を分割し、詳細 reference を `docs/` に逃がして skill の context cost を抑える。

## Install

この pack の内容を Unity repository root へ配置する。既存 `AGENTS.md` や profile がある場合は上書き前に差分を確認する。

Unity Pipeline を live Editor authoring に使う project では、project policy に従って package を導入する。既に導入済みなら agent は `unity pipeline list --format json` と `unity command --format json` で能力を発見する。

Dependency の新規追加を agent が勝手に行う契約にはしていない。Pipeline 未導入の既存 project では、source/test の batch verification と live Scene authoring の可否を分けて扱う。

## Validate this pack

```bash
python3 scripts/validate_agent_pack.py
```

relative Markdown link、skill frontmatter、必須ファイル、旧 uLoop MCP command の混入を検査する。

## Reference baseline

作成時点: 2026-08-14

- Unity Technologies, “Meet the Unity CLI: manage Unity from your terminal”, 2026-07-20
- Unity Docs, “Use the Unity command-line interface (CLI)”
- Unity Docs, “Unity command-line interface (CLI) reference”
- Unity Docs, “Unity Pipeline package”
- `Unity-Technologies/skills`, `skills/unity-cli`

CLI の command/flag は更新されるため、実行時は installed CLI の help を優先する。
