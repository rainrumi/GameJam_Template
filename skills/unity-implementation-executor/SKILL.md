---
name: unity-implementation-executor
description: Use when implementing, fixing, or completing a Unity task in this repository. Enforces the AI Execution Contract from AGENTS.md, reproduces the ai-coding-profile style, uses Unity CLI/Pipeline for authoring and verification, runs relevant tests and runtime checks, and reports only evidence-backed completion.
allowed-tools:
  - Bash
---

# Unity Implementation Executor

Do not stop at file edits. Implement, load in Unity, test, verify runtime behavior when applicable, then report evidence.

## 1. Resolve the contract

1. Read applicable `AGENTS.md`.
2. If a plan exists, read its goal, implementation TODOs, verification TODOs, assumptions, and risks.
3. Read the nearest exemplar from `ai-coding-profile/exemplars.json`.
4. Read enough surrounding code, tests, LifetimeScope, status Asset, Scene/Prefab context, and `Assets/!MyAssets/Object/Prefab` grouping to understand the local contract.
5. Keep unrelated user changes intact.

## 2. Preflight Unity CLI

Use the installed CLI as the source of truth:

```bash
unity --version
unity --help
unity pipeline list --format json
```

When live Editor authoring/inspection is relevant:

```bash
unity command --project-path <project> --format json
# or
unity list --project-path <project> --format json
```

Do not assume `eval`, `recompile`, `editor_play`, `save_scene`, `screenshot`, or any other command exists until discovery shows it.

If an Editor appears to be running but command discovery fails, diagnose Safe Mode with `unity pipeline list --format json` before deciding there is no live Editor.

## 3. Implement minimally

Follow `AGENTS.md` and the profile exactly:

- Instance-scoped MVP + VContainer. Each independently spawned gameplay object / interactive UI gets independent Model/Presenter state; GameLoop stays game-wide orchestration only.
- Model is Pure C#; Presenter remains non-MonoBehaviour and owns orchestration. Presenter may use editor-only `UnityEngine.Debug.Log` only under the required `UNITY_EDITOR` guard. View owns Unity API/Inspector work.
- Prefab-first runtime objects/UI under the existing `Assets/!MyAssets/Object/Prefab` grouping. Prefab-authorable hierarchy/layout/visual/component concerns stay in Prefab/Inspector.
- status interface + ScriptableObject boundary; instance numeric values live in the corresponding `*Info` / existing `*StatusInfo`.
- mutable `Data` + get-only `DataPack` + `GetDataPack()`.
- R3 ownership/disposal.
- UniTask + CancellationToken + lifetime restoration.
- author naming/format conventions plus short comments: class fields use trailing comments; methods and local/loop variables use preceding line comments, about 1–15 characters.
- compatibility typos and serialized vocabulary are preserved.
- no runtime `Find*` as DI, no new global Manager, no unrelated namespace/asmdef/formatter change.
- no production magic numbers; do not replace them with local constants when the value belongs to instance `*Info`.
- no `InvalidOperationException` outside tests. Model error paths return/no-op; View/Presenter/Unity-dependent logging uses `Debug.Log` wrapped in `#if UNITY_EDITOR`.

For Scene/Prefab/Asset changes, use the connected Editor/Pipeline when available. Do not raw-edit Unity YAML while a reachable Editor can perform the authoring. Do not runtime-create or runtime-repair a GameObject/UI when the same result belongs in a Prefab.

Use `eval` only for transient diagnostics/observation; persistent implementation belongs in repository source/assets.

## 4. Source compliance check

Before Unity verification, inspect changed production code and confirm:

- every runtime gameplay object / interactive UI has a Prefab source in the expected `Assets/!MyAssets/Object/Prefab` grouping;
- instance-specific behavior/state is not newly concentrated into `GameLoop*`;
- spawned instances do not share mutable Model/Presenter state;
- changed methods and variables follow the short comment placement rule;
- gameplay/UI numeric values are read from instance `*Info` / existing `*StatusInfo`, not embedded as magic numbers;
- `InvalidOperationException` appears only in test code;
- each production `Debug.Log` is inside `#if UNITY_EDITOR` / `#endif`.

## 5. Verify in layers

Use `docs/VERIFICATION_MATRIX.md` to choose mandatory gates.

### Load/compile

- Warm Editor: discover and use the appropriate exposed recompile/reload command, then rediscover after domain reload if needed.
- Cold/batch: use `unity test` or an appropriate `unity run` workflow from the installed CLI help.

### Tests

Run the narrowest relevant test first.

```bash
unity test <project> --mode EditMode --filter "<filter>" --output <report.xml>
unity test <project> --mode PlayMode --filter "<filter>" --output <report.xml>
```

Read both the command result and generated report. Classify failures instead of treating every non-zero exit the same.

### Runtime

For runtime-affecting changes, observe the changed behavior itself. Use, in order of preference:

1. existing automated test / verification command;
2. project-specific `[CliCommand]`;
3. structured state query / discovered `eval`;
4. Play Mode + hierarchy/state inspection;
5. screenshot for visual evidence.

A successful Play Mode transition alone is not sufficient evidence.

### Scene/Prefab/Inspector

If authoring changed:

- confirm the actual instance has the intended Prefab source and that the Prefab is stored under the existing `Assets/!MyAssets/Object/Prefab` convention;
- inspect actual hierarchy/component/reference state;
- save via Editor;
- confirm serialized references are assigned;
- if important, re-open/reload and confirm persistence;
- run the usage path;
- when instance MVP changed, create/observe at least two instances when practical and confirm mutable state/lifetime is isolated.

## 6. Failure loop

For any relevant failure:

1. identify whether it is usage/auth/configuration/compile/test/runtime/authoring failure;
2. fix the actual cause within scope;
3. rerun the failed gate;
4. rerun dependent gates whose evidence may have been invalidated by the fix;
5. continue until required gates pass or a real external blocker remains.

Do not hide required-reference failures with defensive null checks just to make the test green.

## 7. Report

Use exactly one status:

- `VERIFIED`
- `PARTIALLY_VERIFIED`
- `BLOCKED`

Report:

- changed behavior and major files/assets;
- Unity CLI/Pipeline operations actually used;
- compile/import result;
- test mode/filter/result and report outcome;
- concrete runtime behavior observed;
- Scene/Prefab/Inspector persistence/wiring evidence when applicable;
- unverified gates, pre-existing failures that affect confidence, and residual compatibility risk.

Never mark `VERIFIED` when a required gate was not executed.
