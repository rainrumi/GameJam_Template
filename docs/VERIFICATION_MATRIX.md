# Unity Change Verification Matrix

`AGENTS.md` の verification contract を変更種別へ具体化する。表の gate は「最低線」であり、影響が広い場合は追加する。

| Change type | Minimum compile/load | Automated verification | Runtime / authoring evidence |
|---|---|---|---|
| Pure Model calculation | Unity compile/load | targeted EditMode `[Test]` | 原則不要。runtime integration が変わる場合は追加 |
| Random/deterministic logic | Unity compile/load | seed determinism + boundary EditMode tests | 必要なら representative runtime path |
| Presenter orchestration | Unity compile/load | relevant EditMode or UnityTest | state transition / subscription behavior |
| R3 subscription/dispose | Unity compile/load | dispose / duplicate notification test | relevant lifecycle if scene-dependent |
| UniTask cancellation | Unity compile/load | `[UnityTest]` / cancellation test | destroy/state-change path |
| View code | Unity compile/load | View test where practical | target Scene in Play Mode or project verification command |
| SerializeField added/changed | Unity compile/load | serialized setup test where practical | live Inspector/Prefab wiring + missing reference absence |
| Scene hierarchy | Unity reload | existing scene test if any | live hierarchy query + save + runtime observation |
| Prefab structure | Unity reload | prefab test if any | live Prefab/instance inspection + path/source check + save + usage path |
| Runtime gameplay/UI instance | Unity compile/load | relevant instance test where practical | Prefab source under existing `Assets/!MyAssets/Object/Prefab` grouping; no script-built substitute |
| Instance-scoped MVP | Unity compile/load | state isolation / dispose test where practical | observe at least two instances and confirm mutable Model/Presenter state/lifetime is not shared |
| GameLoop responsibility change | Unity compile/load | orchestration test | confirm instance movement/state/UI responsibility remains in instance MVP |
| Numeric tuning/config | Unity compile/load | Model test through `*Info` / status interface | changed behavior uses authored Info/Asset value; no production magic number |
| Error/logging path | Unity compile/load | targeted failure-path test | no production `InvalidOperationException`; Unity-dependent `Debug.Log` is Editor-guarded |
| ScriptableObject config | Unity import/load | Model test through status interface | actual Asset assignment/value query if relevant |
| LifetimeScope registration | Unity compile/load | resolve/integration test where available | Scene startup without DI resolution failure |
| Package/manifest | package resolution + Unity compile | relevant suites | Editor/package availability query |
| Input behavior | Unity compile/load | input test if infrastructure exists | real target input flow/state reaction |
| UI/layout/animation | Unity compile/load | logic tests as applicable | target state + screenshot and/or property query |
| Save/PlayerPrefs compatibility | Unity compile/load | old/new data compatibility tests | representative load/save path |
| Build pipeline/profile | Unity compile/load | relevant tests | `unity build` and produced artifact |

## Required evidence rules

### Compile/load

`Unity loaded the changed project without new compile/import failure` を意味する。IDE の C# compile だけでは Unity-specific compile/load の代替にならない。

### Tests

- targeted test first。
- command exit code と test report の両方を確認。
- test が存在しない Pure C# behavior は、追加できない理由がない限り test を追加する。
- unrelated pre-existing failure は区別するが、relevant gate を壊しているなら verification limitation として残す。

### Runtime

runtime evidence は変更した behavior の観察である。

不十分な例:

- Play Mode に入れた。
- Scene が開いた。
- screenshot が撮れた。

十分な例:

- `GameState` が期待状態へ遷移し、Presenter の購読が 1 回だけ発火した。
- destroy 中の async flow が cancellation され、temporary interaction state が `finally` で復元された。
- added SerializeField が target Prefab に割り当てられ、Play Mode で missing reference exception が発生しなかった。

## Source compliance

変更した production source は Unity 実行前に次を確認する。

- class field は短い文末コメント、method / local / loop variable は 1〜15文字程度の行コメントを持つ。
- gameplay/UI の意味を持つ numeric value は instance `*Info` / existing `*StatusInfo` から取得し、behavior code に magic number を置かない。
- test script 外に `InvalidOperationException` がない。
- production `Debug.Log` がすべて `#if UNITY_EDITOR` / `#endif` 内にある。
- Prefab で authoring 可能な hierarchy/layout/visual/component concern を runtime code が生成・repair していない。

## Risk escalation

次の場合は最低線より verification を 1 段階広げる。

- serialized field / type / namespace / assembly boundary を変更した。
- Scene/Prefab 参照を変更した。
- async lifetime / cancellation ownership を変更した。
- save compatibility を変更した。
- public interface を変更した。
- shared Model/service を複数 feature が利用している。
- package version / project settings / build profile を変更した。
