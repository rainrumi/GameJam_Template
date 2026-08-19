# Design Rationale and Source Mapping

この文書は、この agent contract が「どの設計思想をどこから継承し、Unity CLI 用に何を変更したか」を記録する。

## 1. 先輩設計から継承したもの

### Senior A: unity-eightAID skill pack

継承した中核:

- 実装前に周辺コード、Scene、Prefab、serialization、既存 architecture を読む。
- 変更範囲を依頼された behavior に限定する。
- Inspector wiring を runtime fallback で隠さず authoring contract として扱う。
- 実装 TODO と検証 TODO を分離せず、同一 task の完了条件にする。
- file edit → compile → Console/error → tests → runtime behavior → report の順で完走する。
- planner と implementation/check workflow を skill として分離する。
- required reference を silent no-op で隠さない Fail Fast 方針。

UnityCLI 版では、uLoop 固有 tool call を削除し、Unity CLI / Pipeline の runtime discovery と first-class `test`, `run`, live command に置換した。

### Senior B: void2610/my-unity-template CLAUDE.md

継承した中核:

- tool output の成功サマリだけで完了判定せず、warning/error/failure evidence を確認する。
- editor authoring と runtime verification の役割を明確にする。
- compile だけでなく、実際に runtime behavior を確認する。
- 再現可能な verification をテスト資産へ昇格させる。
- 新しい運用・設計判断を documentation に残す。
- analyzer/formatter/compile/test の「実行した事実」ではなく「結果を読んだ事実」を重要視する。

UnityCLI 版では、uLoop / LiminalPalette という特定製品への依存は持ち込んでいない。代わりに次の構造へ一般化した。

- editor operation → Unity Pipeline の discovered `[CliCommand]`
- live query → discovered `eval` / project-specific command
- repeatable regression verification → Unity Test Framework test
- stable project automation → project-specific `[CliCommand]`
- evidence parsing → Unity CLI structured JSON/NDJSON + test report + relevant logs

## 2. ai-coding-profile が senior template より優先する点

先輩の設計書は execution contract の参考であり、coding style の source of truth ではない。この project では `ai-coding-profile` と実コード evidence が coding decision を支配する。

具体的な conflict resolution:

| Topic | Senior example | This project contract |
|---|---|---|
| View resolution / DI | `FindFirstObjectByType<T>()` を使う例あり | runtime `Find*` を DI として新規導入しない。VContainer registration / constructor injection を維持 |
| New constants | `ALL_UPPER` の例あり | 新規 game code は PascalCase。既存 UPPER_SNAKE_CASE は preserve-only |
| Tween | LitMotion を使う senior project あり | ai-coding-profile の既存 DOTween lifetime pattern を維持 |
| Namespace | generic C# style の可能性 | `Assets/!lumirr/scripts` は namespace なし、RaruLib は block namespace |
| Architecture | generic MVP | profile の Model/View/Presenter + LifetimeScope registration shape を具体的に再現 |

この差分は意図的である。「先輩の書き方をコピー」ではなく、**execution contract を移植し、project-local implementation style は evidence から復元する**という設計。

## 3. 実装後レビューによる project-wide correction

一ゲーム実装後の運用レビューから、既存 profile の模倣だけでは次の偏りが生じることが確認されたため、これらは古い exemplar より優先する correction とした。

- **Prefab-first**: runtime に出現する gameplay object / interactive UI は `Assets/!MyAssets/Object/Prefab` の既存規則へ従う Prefab を source of truth にする。Prefab で解決できる authoring concern を Script で runtime repair しない。
- **Instance-scoped MVP**: 同じ class は再利用してよいが、spawn された各 instance が独立した Model / Presenter state/lifetime を持つ。`GameLoop*` は game-wide orchestration へ限定する。
- **Short comments**: class field は文末、method / local variable は行コメントとし、1〜15文字程度の日本語・英単語で役割を示す。
- **No magic numbers**: gameplay/UI numeric value は対応 instance の `*Info` / 既存 `*StatusInfo` へ移し、production behavior code へ埋め込まない。
- **Error/logging**: test 外 `InvalidOperationException` を禁止する。Model は return/no-op を使い、Unity-dependent code の `Debug.Log` は `UNITY_EDITOR` guard 内だけで使用する。

この correction の目的は「より一般的な設計へ寄せる」ことではなく、実装後に観測された失敗モードを contract で再発防止することである。 Model の expected error については、旧 template の general fail-fast 方針より今回の return/no-op 方針を優先する。一方、Prefab/Inspector の required reference 不足は authoring failure として隠さない。

## 4. Unity CLI 固有の再編成

Unity CLI は次の 3 layer として扱う。

### Layer A — CLI process contract

- machine-readable JSON/NDJSON
- stdout / stderr separation
- differentiated exit codes
- `unity --help` / per-command `--help`
- `test`, `run`, `build` 等の first-class commands

### Layer B — Unity Pipeline live Editor contract

- `unity pipeline list`
- `unity command`
- `unity list`
- `unity status`
- project-specific `[CliCommand]`
- discovered `eval` / `eval_file`

ここでは command catalog を AGENTS に固定しない。接続中 Editor が自己記述する schema を毎回取得する。

### Layer C — Project verification assets

- Unity Test Framework EditMode / PlayMode tests
- existing debug/verification command
- reusable project-specific `[CliCommand]`
- test report / runtime state / screenshot evidence

Senior B の「検証作業が回帰資産を育てる」という思想は Layer C に移した。

## 5. Safe Mode を execution contract に含めた理由

Unity Pipeline は normal package として load されるため、C# compile error で Editor が Safe Mode に入ると live command 接続が失われる。この状態で「Editor がいない」と誤判定すると、agent が Scene/Prefab raw file を blind edit する危険がある。

そのため接続 failure は次の分類を必須にした。

1. no Editor
2. Pipeline missing
3. wrong project target
4. Safe Mode / compile failure
5. auth/configuration failure
6. command-specific failure

これは UnityCLI 用 execution contract の重要な差分。

## 6. `.system(1).zip` の扱い

提供された `.system(1).zip` を確認したところ、内容は `imagegen`, `openai-docs`, `review-agent`, `skill-creator`, `skill-installer` 等の general system skills で、`unity-cli/SKILL.md` 自体は含まれていなかった。

この pack では `.system(1)` から次の **skill-authoring principles** を採用した。

- `SKILL.md` を concise な procedure に保つ。
- detailed knowledge は `references/docs` へ分離する。
- task の fragility に応じて freedom を調整する。
- deterministic validation が有効なら script 化する。
- review は defect-first / evidence-first にする。

Unity CLI command semantics は、Unity 公式の次を優先して再確認した。

- Unity blog: `Meet the Unity CLI: manage Unity from your terminal`
- Unity Docs: `Use the Unity command-line interface (CLI)`
- Unity Docs: `Unity command-line interface (CLI) reference`
- Unity Docs: `Unity Pipeline package`
- GitHub: `Unity-Technologies/skills/skills/unity-cli`

## 7. なぜ AGENTS.md に command catalog を全部入れないか

Unity CLI は experimental で、installed version によって command/flag が増減し得る。さらに `unity command` は project の Pipeline package と custom `[CliCommand]` によって動的に変わる。

したがって contract は「この command を必ず使う」ではなく、次を固定する。

1. capability を discovery する。
2. machine-readable result を使う。
3. operation failure を exit code と structured error で分類する。
4. actual project state を observation してから act する。
5. verification evidence を読んでから report する。

**変わりやすい API 名ではなく、変わりにくい意思決定手順を固定する**のがこの構成の中心。
