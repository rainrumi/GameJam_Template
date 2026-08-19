---
name: unity-implementation-planner
description: Use when planning a non-trivial Unity implementation or bug fix before editing. Produces an evidence-based plan that maps each code/Scene/Prefab/Asset change to Unity CLI verification gates while preserving the repository AGENTS.md and ai-coding-profile conventions.
allowed-tools:
  - Bash
---

# Unity Implementation Planner

Create a plan that another agent can execute without re-deciding the architecture or verification strategy.

## Procedure

1. Read the applicable `AGENTS.md` files.
2. Read `ai-coding-profile/README.md`, then the relevant parts of `coding-style.md`, `style-profile.json`, and `exemplars.json`.
3. Inspect the nearest existing feature, call sites, tests, LifetimeScope, ScriptableObject, Scene/Prefab dependencies, serialized compatibility surface, and the existing grouping/naming under `Assets/!MyAssets/Object/Prefab`.
4. Read `ProjectSettings/ProjectVersion.txt` instead of assuming the profile snapshot version is current.
5. If Unity CLI capabilities matter to the plan, inspect the installed CLI with `unity --version` / `unity --help`. For live Editor work, discover capabilities with `unity command --project-path <project> --format json` or `unity list ...`; do not invent command names.
6. Separate facts from assumptions. Prefer a safe local assumption only when it does not change the architecture or compatibility contract.
7. Map every implementation item to at least one verification item using `docs/VERIFICATION_MATRIX.md`.
8. Do not edit Unity source, Scene, Prefab, Asset, package, or ProjectSettings unless the user also asked for implementation.

## Planning rules

- Prefer the nearest exemplar over generic Unity patterns.
- Use instance-scoped MVP + VContainer. Gameplay objects and interactive UI that spawn independently must receive independent Model/Presenter state; do not treat old GameLoop-heavy code as authority for instance responsibility.
- Preserve `Data` / `DataPack`, status interface, R3, UniTask, naming, namespace, typo-compatibility rules, and the corrected short-comment rule from the profile.
- Do not plan runtime `Find*` as DI.
- Every runtime gameplay object / interactive UI must be planned from a Prefab source under the existing `Assets/!MyAssets/Object/Prefab` convention. Do not plan `new GameObject`, runtime `AddComponent`, or script-built UI when Prefab authoring can express the result.
- Do not plan Scene/Prefab raw YAML editing when a live Editor can author it.
- Do not silently plan `com.unity.pipeline` installation if the project does not already use it; dependency addition must be an explicit scope item.
- Code tasks and authoring tasks are separate TODOs. Prefab-authorable hierarchy/layout/visual/component work belongs to authoring, not code.
- Plan numeric gameplay/UI values through the instance `*Info` / existing `*StatusInfo`; do not plan magic numbers in behavior code.
- Plan error handling without `InvalidOperationException` outside tests. Model uses return/no-op; Unity-dependent code uses editor-only `Debug.Log`.
- Include a source-style TODO to verify method/local comments and class-field trailing comments (about 1–15 characters).
- Verification is part of each feature task, not an afterthought.

## Plan template

```markdown
# <Task> Unity Implementation Plan

## Goal
- <observable outcome>

## Evidence
- Existing feature/exemplar: <paths>
- Relevant tests: <paths>
- Scene/Prefab/Asset: <targets>
- Unity version: <ProjectVersion.txt>
- Unity CLI/Pipeline capability: <observed, not assumed>

## Constraints
- <serialization / save / API / architecture constraints>

## Decisions
- <decision> — <evidence/rationale>

## Implementation
- [ ] <specific code change>
- [ ] <specific Prefab source/path and Scene/Prefab/Inspector authoring change>
- [ ] <specific test addition/update>

## Verification
- [ ] Compile/load: <method>
- [ ] EditMode: <filter or reason not required>
- [ ] PlayMode/UnityTest: <filter or reason not required>
- [ ] Runtime: <observable behavior and evidence>
- [ ] Prefab source/path: <under Assets/!MyAssets/Object/Prefab and follows local grouping>
- [ ] Instance MVP isolation: <two-instance or equivalent evidence that mutable Model/Presenter state is not shared>
- [ ] Scene/Prefab wiring: <query/save/reopen evidence if applicable>
- [ ] Source policy: <comments / no magic numbers / no production InvalidOperationException / editor-only Debug.Log>

## Risks / Assumptions
- <remaining risk or assumption>
```

## Quality gate

Do not leave TODOs such as `implement`, `check`, `make it work`, or `test manually` without a target and observable outcome. A valid plan states **what changes, where, why, and how success will be demonstrated**.
