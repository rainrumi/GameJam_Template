# Unity CLI Runbook for AI Agents

この文書は Unity CLI の操作知識を project contract に落としたもの。CLI は experimental で更新されるため、**ここにある command 例より、実機の `unity --help` と接続 Editor の command schema を優先する**。

## 1. Source of truth

1. `unity --help`
2. `unity <command> --help`
3. `unity command --project-path <project> --format json`
4. `unity list --project-path <project> --format json`
5. Unity 公式 CLI docs / official `Unity-Technologies/skills` の `unity-cli` skill
6. この runbook の例

## 2. Machine-readable first

AI が output を判定するときは原則 `--format json`。

```bash
unity pipeline list --format json
unity status --format json
unity command --project-path <project> --format json
```

長時間処理の progress と final result を逐次読む場合は `--format ndjson` を検討する。

- stdout: data/result。
- stderr: errors/diagnostics。
- exit code: failure category。

human output の文言 scraping より structured field を優先する。

## 3. Project targeting

複数 project / Editor があり得る環境では `--project-path` を明示する。

```bash
unity command --project-path /abs/path/to/project --format json
```

current directory の暗黙解決だけに依存しない。

## 4. Live Editor decision tree

### A. GUI Editor が ready

1. `unity status --format json`。
2. `unity command --project-path ... --format json` で capability discovery。
3. Scene/Prefab/Asset authoring は exposed command を使う。
4. save/recompile 後は command list / state を再取得する。

### B. Headless resident Editor

batch mode で常駐させた Editor は `unity status` に現れない構成があり得る。`status` が空でも、headless resident を想定する環境では `unity command` / `unity list --project-path` の到達性も確認する。

### C. Editor は開いているが接続不能

`unity pipeline list --format json` で Safe Mode を先に確認する。

Safe Mode なら Pipeline package は load されないため、Scene command へ接続できない。C# compile error を source で直し、Editor を正常化してから再接続する。

### D. Pipeline が未導入

- C# source の変更、`unity test`、`unity run`、`unity build` は task に応じて利用できる。
- live Scene/Prefab/Inspector authoring はできない。
- Pipeline の install/upgrade は dependency change。task scope が許可しない限り黙って追加しない。

## 5. Scene / Prefab / Asset

connected Editor があるなら raw YAML より live Editor operation を優先する。

理由:

- active in-memory Scene と disk file の不一致を避けられる。
- GUID / fileID を手作業で壊しにくい。
- dirty/save/import state を Unity 自身に管理させられる。

手順:

1. command discovery。
2. target Scene / hierarchy / object を query。
3. change。
4. Inspector reference / component state を query。
5. save。
6. 必要なら reload/reopen 後に再 query。
7. runtime verification。

## 6. Source compilation

warm Editor では exposed `recompile` 相当 command があるか discovery する。名称・引数は固定しない。

cold verification では `unity test` が compile + test を通す最も有用な gate。テスト対象がない場合は installed CLI の `unity run --help` を確認し、batch project load / executeMethod / project command を使う。

`unity run` は batch mode 等を CLI 側が管理するため、reserved Unity flag を重ねて指定しない。

## 7. Tests

例:

```bash
unity test <project> --mode EditMode --filter "Namespace.Tests" --output ./TestResults/editmode.xml
unity test <project> --mode PlayMode --filter "Namespace.PlayTests" --output ./TestResults/playmode.xml
```

- targeted test を先に実行する。
- output report path を明示する。
- exit code `6` は test failure を含む command failure。
- report file が生成されていれば、pass/fail count と failure detail を読む。
- JUnit / coverage 等の option は installed CLI の help と project package availability を確認して使う。

## 8. Runtime verification

connected Editor では `unity command` が公開する Play Mode、state query、screenshot、custom command、`eval` 等を discovery する。

優先順位:

1. machine-readable project-specific verification command / test。
2. state query / `eval`。
3. Play Mode + hierarchy/state inspection。
4. screenshot/visual evidence。

visual-only 判定で済ませられる task 以外は、可能なら structured state evidence を併用する。

## 9. `eval`

`eval` / `eval_file` は package/editor により availability が異なる。

Use:

- runtime state query
- temporary diagnostic
- one-off assertion
- API reachability check

Do not use as:

- persistent production source replacement
- serialized authoring の恒常的な裏口
- regression test の唯一の資産

再利用価値の高い verification は test または project-specific `[CliCommand]` へ移す。

## 10. Build

build output 自体に影響する変更、build pipeline の task、user が build を要求した場合は `unity build` を verification gate にする。

単なる gameplay C# 変更のたびに full player build を必須化しない。verification cost は変更リスクに対応させる。

## 11. Output integrity

- JSON envelope の `success`, `data`, `errors`, `warnings` を確認する。
- stderr を無視しない。
- test report を読む。
- tool output の末尾だけで判断しない。
- global Editor.log は他 project/session 情報を含み得るため、必要な compile/runtime error pattern と session に絞る。
- secret、token、credential を report や commit message に転記しない。

## 12. Reference baseline

この pack は 2026-08-14 時点で以下を参照している。

- Unity: `Meet the Unity CLI: manage Unity from your terminal` (2026-07-20)
- Unity Docs: `Use the Unity command-line interface (CLI)`
- Unity Docs: `Unity command-line interface (CLI) reference`
- Unity Docs: `Unity Pipeline package`
- GitHub: `Unity-Technologies/skills`, `skills/unity-cli`

CLI は experimental のため、将来 command/flag が変わっても runtime discovery contract を維持する。
