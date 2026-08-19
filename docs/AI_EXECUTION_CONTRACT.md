# AI Execution Contract

この文書は `AGENTS.md` の「意思決定・検証・報告」を、実装エージェントが実行可能な phase に分解した runbook である。

## Contract model

AI の作業を次の 8 phase として扱う。

| Phase | 目的 | Exit condition |
|---|---|---|
| 0. Resolve | 指示と規約の解決 | task scope と適用規約が確定 |
| 1. Observe | 既存実装と Unity 状態の観察 | nearest precedent と Unity state を把握 |
| 2. Decide | 実装方針の決定 | 変更点と verification gate が対応付く |
| 3. Change | 最小変更 | task scope 外の差分を増やさず実装完了 |
| 4. Load | Unity に反映 | compile/import failure がない |
| 5. Test | deterministic verification | relevant automated test の結果を取得 |
| 6. Runtime | 実動作確認 | user-facing / lifecycle behavior を観察 |
| 7. Report | 証拠に基づく報告 | status と残リスクを明示 |

## Phase 0 — Resolve

1. root と対象 subtree の `AGENTS.md` を読む。
2. task 内の明示要件を抽出する。
3. `ai-coding-profile/README.md` → `coding-style.md` → `style-profile.json` → `exemplars.json` の順で必要箇所を確認する。
4. 計画書がある場合は要件、TODO、verification TODO、前提を読む。
5. 変更禁止領域、serialized compatibility、external dependency 変更の有無を識別する。

### Rule conflict

- safety / verification は root AGENTS が上位。
- code shape は nearest existing feature が上位。
- profile と古い legacy file が衝突する場合、legacy file 自体を局所修正するときだけ legacy shape を優先する。
- senior template と local profile が衝突する場合、local profile を採用する。例: この project では新規定数は PascalCase、runtime `Find*` を DI に使わない。

## Phase 1 — Observe

### Repository

- 対象 class だけでなく、呼び出し元、依存先、test、LifetimeScope、関連 ScriptableObject を読む。
- `exemplars.json` から同種課題の primary exemplar を先に読む。
- Scene/Prefab 変更が関係するなら、live Editor が利用可能か確認して実物を観察する。

### Unity CLI preflight

最低限、必要に応じて以下を取得する。

```bash
unity --version
unity --help
unity pipeline list --format json
```

connected Editor を使う場合:

```bash
unity command --project-path <project-path> --format json
# or
unity list --project-path <project-path> --format json
```

この出力で capability を発見してから操作を選ぶ。固定 command 名は契約ではない。

### Baseline

既に failure が存在する可能性がある task では、変更前の relevant baseline を取る。

- relevant test filter
- current compile state
- target Scene / Prefab reference state
- reproducer command / runtime state

baseline を取れない場合は、その事実を後で「pre-existing と断定できない制約」として扱う。

## Phase 2 — Decide

実装 TODO と verification gate を 1:1 に対応付ける。

例:

| Change | Evidence |
|---|---|
| Model calculation | EditMode test + compile |
| Presenter subscription | EditMode/UnityTest + dispose path |
| View async lifetime | PlayMode/UnityTest + runtime error absence |
| SerializeField追加 | live Inspector/Prefab wiring + compile + runtime |
| Runtime instance / UI追加 | Prefab path/source + live instance inspection + compile + runtime |
| Instance MVP変更 | targeted test + two-instance state/lifetime isolation + runtime |
| Scene hierarchy変更 | live hierarchy query + save + reopen/runtime |
| package変更 | dependency resolution + compile + tests |
| build設定 | `unity build` artifact |

テスト可能な Pure C# behavior を manual-only にしない。

## Phase 3 — Change

- 変更は依頼された behavior に限定する。
- 既存 API / serialized data を守る。
- Scene/Prefab/Asset authoring は接続 Editor を優先する。runtime gameplay object / interactive UI は `Assets/!MyAssets/Object/Prefab` の既存規則へ従う Prefab を source of truth にする。
- production source は repository file として変更する。`eval` は永続実装の代替にしない。
- profile の naming、format、instance-scoped MVP、VContainer、R3、UniTask、Data/DataPack を再現する。
- Prefab で解決できる hierarchy / layout / visual / component setting を Script で runtime repair しない。
- `GameLoop*` は game-wide orchestration に限定し、instance behavior/state を集約しない。
- changed method / variable の短い comment、production magic number 不在、error/logging policy を source review する。
- test 外 `InvalidOperationException` を禁止し、Model は return/no-op、Unity-dependent logging は editor-only `Debug.Log` とする。

## Phase 4 — Load

### Warm Editor

connected Editor がある場合、`unity command` で exposed compile/recompile/save/status command を discovery して使用する。compile/reload 後は connection が一時的に切れる可能性があるため、再 discovery して ready state を確認する。

### Cold / batch

Pipeline がなくても source/test 検証は Unity CLI の first-class command を利用できる。

```bash
unity test <project> --mode EditMode --filter "<filter>" --output <report.xml>
```

テスト対象がなく project load 自体を検証する必要がある場合は、インストール済み CLI の help を確認して `unity run` を用いる。`unity run` が管理する reserved flag を重複指定しない。

### Safe Mode recovery

Editor が存在するのに command connection が成立しない場合:

1. `unity pipeline list --format json`。
2. Safe Mode fields を確認。
3. compiler error を source で修正。
4. Editor restart/reload 後に discovery を再実行。

global Editor.log を無差別に貼り付けない。必要な compile error pattern と対象 session/path へ絞る。

## Phase 5 — Test

1. 最小 filter の relevant test を先に実行する。
2. failure は test assertion failure、compile failure、setup failure、timeout を分類する。
3. command exit code に加え test report artifact を読む。
4. fix 後は targeted test を再実行する。
5. 変更の影響範囲が広い場合、関連 suite まで拡張する。

テストがない場合:

- Pure C# behavior なら原則 test を追加する。
- Unity lifecycle / serialized wiring なら `[UnityTest]` または既存 test utility を検討する。
- 一回限りの state inspection は `eval` が公開されていれば利用できる。
- 再発防止価値がある verification は Unity Test Framework test または安定した project-specific `[CliCommand]` へ昇格させる。

## Phase 6 — Runtime

runtime change の場合、「Play Mode に入れた」だけでは足りない。

- target Scene / state へ到達する。
- user-visible result、state transition、input reaction、lifetime behavior を観察する。
- runtime exception がないことを確認する。
- UI / layout / visual change は state query と screenshot を必要に応じて併用する。
- deterministic state query が可能なら screenshot より machine-readable result を優先する。

## Phase 7 — Report

### Status semantics

- `VERIFIED`: task に必要な全 gate を実行し、成功証拠を確認した。
- `PARTIALLY_VERIFIED`: 実装は完了したが、必要 gate の一部を環境制約等で実行できなかった。
- `BLOCKED`: task の主要部分または required authoring/verification が外部条件で完了できない。

### Report shape

```text
Status: VERIFIED | PARTIALLY_VERIFIED | BLOCKED

Changed:
- ...

Verification:
- <command / method>: <result>
- Compile/import: ...
- Tests: ...
- Runtime: ...

Residual:
- 未検証 gate / pre-existing failure / compatibility risk
```

「たぶん」「問題なさそう」を verification status の代わりに使わない。
