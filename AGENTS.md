# Unity AI Execution Contract

このリポジトリで AI coding agent が Unity の実装、修正、Scene/Prefab/Asset 編集、テスト、レビュー補助を行うときは、この契約に従う。

このファイルの目的は、単に「それらしいコードを書く」ことではない。**既存実装を根拠に意思決定し、変更を Unity 上で検証し、検証した事実だけを報告する**ことを完了条件にする。

詳細な実行手順は [`docs/AI_EXECUTION_CONTRACT.md`](docs/AI_EXECUTION_CONTRACT.md)、Unity CLI の運用は [`docs/UNITY_CLI_RUNBOOK.md`](docs/UNITY_CLI_RUNBOOK.md)、変更種別ごとの最低検証は [`docs/VERIFICATION_MATRIX.md`](docs/VERIFICATION_MATRIX.md) を参照する。

---

## 1. 規約の優先順位

競合する指示がある場合は、次の順で判断する。

1. ユーザーが今回明示した要求。
2. 対象ファイルに最も近い `AGENTS.md`。
3. このルート `AGENTS.md` の安全、検証、報告契約。
4. 対象機能に最も近い既存実装と、実際の Scene / Prefab / Asset / Test。
5. `Assets/!lumirr/scripts` で反復しているプロジェクト標準。
6. [`ai-coding-profile/coding-style.md`](ai-coding-profile/coding-style.md) と [`style-profile.json`](ai-coding-profile/style-profile.json)。
7. `Assets/!lumirr/RaruLib/src` の legacy 局所規約。ただし RaruLib を編集するときだけ優先する。
8. 一般的な Unity / C# のベストプラクティス。

`ai-coding-profile` は `strict_author_style_replication` として扱う。一般論で「より綺麗」に置き換えない。新しい反証が対象機能の実装、Asset、Test から得られた場合だけ、より近い実証を優先する。

プロファイルの snapshot に記載された Unity version は観測時点の情報であり、現在の必須 version と決めつけない。実作業では `ProjectSettings/ProjectVersion.txt` を読む。

---

## 2. AI Execution Contract

### 2.1 完了の定義

変更は、次をすべて満たしたときだけ完了扱いにする。

1. **Understand** — 要件、対象範囲、既存設計、互換性制約を把握した。
2. **Decide** — 変更方針を既存実装と根拠に基づいて決めた。
3. **Act** — 依頼範囲だけを変更した。
4. **Compile/Load** — Unity が変更を読み込み、compile/import failure が残っていないことを確認した。
5. **Test** — 変更に対応する自動テストを実行した。テストが存在しない場合は、作成可能性を検討したうえで代替検証を実施した。
6. **Runtime Verify** — runtime behavior に影響する変更は、対象導線を実際に動かして確認した。
7. **Inspect Evidence** — exit code、structured output、test report、対象ログ、runtime observation を確認した。
8. **Report** — 実施した変更と検証、未検証事項、残リスクを事実ベースで報告した。

ファイルを編集しただけでは完了ではない。compile だけでも runtime 変更の完了ではない。

### 2.2 証拠のない成功報告を禁止する

- 実行していない検証を「問題なし」と書かない。
- command の exit code だけを見て、生成された test report や relevant error を無視しない。
- stdout の末尾数行、成功サマリ、スクリーンショット 1 枚だけで全体成功と判断しない。
- 必須 gate が実行できなかった場合は `VERIFIED` と報告しない。`PARTIALLY_VERIFIED` または `BLOCKED` とする。
- 既存の無関係な failure と今回導入した failure を分離して報告する。

### 2.3 変更範囲

- 依頼に必要な最小範囲へ限定する。
- 関係ない formatter 適用、rename、namespace 導入、architecture 置換、Asset 再保存を行わない。
- 新規共通処理を作る前に、既存 helper、service、extension、base class、command、test utility を検索する。
- Serialize field、PlayerPrefs key、enum 順、Scene/Prefab 名、互換 typo を、依頼なしに一括変更しない。

---

## 3. Unity CLI を使う契約

Unity CLI はこのプロジェクトの標準的な Unity 操作・検証インターフェースとして扱う。ただし CLI 自体は変化し得るため、**インストール済み CLI と接続中 Editor の自己記述を source of truth にする**。

### 3.1 Runtime discovery first

作業開始時、必要に応じて次を確認する。

```bash
unity --version
unity --help
unity pipeline list --format json
```

接続中 Editor を操作する場合は、固定された command 名を思い込まず、対象 project を明示して command schema を取得する。

```bash
unity command --project-path <project-path> --format json
# または
unity list --project-path <project-path> --format json
```

- programmatic parsing には `--format json` を優先する。
- 長時間 command で streaming progress が必要なら `--format ndjson` を使う。
- CLI の実際の `--help` と接続 Editor が公開する schema が、この文書の例より優先される。
- project が複数ある環境では `--project-path` を省略して対象を推測しない。

### 3.2 Exit code は分類に使い、証拠の代わりにしない

Unity CLI の代表的な exit code は次の意味で扱う。

- `0`: command success。
- `1`: general error。
- `2`: usage error。flags / arguments の誤りを先に直す。
- `3`: authentication / authorization failure。
- `4`: required configuration / precondition 不足。
- `6`: command operation failure。test failure や Editor failure を含む。
- `130`: SIGINT / user cancel。
- `143`: SIGTERM / runner timeout 等。

non-zero を source code の defect と即断しない。usage、auth、configuration、test failure、Editor failure を exit code と stderr / structured error から切り分ける。

### 3.3 Scene / Prefab / Asset authoring

接続可能な Unity Editor と Pipeline が存在する場合、Scene、Prefab、GameObject、Inspector、Unity Asset の変更は **live Editor 経由を優先**する。

- まず `unity command` / `unity list` で利用可能 command を発見する。
- Scene / Prefab / Asset を raw YAML として手編集しない。
- active Scene と disk 上の推測ファイル名を同一視しない。
- authoring 後は dirty state を保存し、再読込後も参照が維持されることを確認する。
- required SerializeField、missing component、Prefab override、Scene reference を実物で確認する。

Pipeline が利用できない場合、C# source の編集と `unity test` / `unity run` 等による batch 検証は続行できる。ただし Scene/Prefab/Inspector の authoring が必要な task では、raw YAML を安易な fallback にせず、未完了 gate として報告する。

`com.unity.pipeline` の新規導入・upgrade は project dependency の変更である。既に導入済みでない場合、task scope または project policy が許可していない限り、検証のためだけに黙って導入しない。

### 3.4 Safe Mode

Editor が開いているはずなのに `unity command` / `unity list` / `unity status` が接続できない場合、「Editor がない」と決めつけない。

1. `unity pipeline list --format json` を確認する。
2. Safe Mode が検出された場合、原因の C# compile error を source で修正する。
3. Editor を正常状態へ戻してから command discovery をやり直す。

Safe Mode 中は Pipeline package が load されない前提で扱う。接続不能を理由に Scene/Prefab の blind edit へ移行しない。

### 3.5 `eval` の位置付け

`unity command eval` / `eval_file` が接続 Editor で公開されている場合、観察、診断、短い検証、live state query に利用してよい。

- availability は必ず discovery する。存在を前提にしない。
- production source code の永続実装を `eval` だけで済ませない。
- persistent behavior は repository source / Asset として実装し、その後に Unity を recompile / reload して検証する。
- `eval` で一時変更した runtime state を、永続修正の証拠として扱わない。

---

## 4. コーディング方法 — ai-coding-profile の再現

### 4.1 Architecture: MVP + VContainer

新規ゲーム機能は、対象領域に反証がない限り MVP を採用する。

#### Model

- `MonoBehaviour` にせず Pure C# を基本とする。
- mutable state、計算、判定、乱数、状態遷移、game rule を持つ。
- dependency は constructor injection で受ける。
- Unity 非依存の 2D 計算では `System.Numerics.Vector2` を優先する。
- 外部へ mutable state を直接公開せず、`GetDataPack()` で snapshot を返す。
- Subject 等の寿命を所有する場合は `IDisposable` を実装する。
- same seed + same input から同一結果になるロジックは deterministic な Pure C# として保つ。

#### View

- `MonoBehaviour` を継承する。
- Inspector reference、Unity API、Transform、Sprite、Canvas、Button、Input event、表示更新を担当する。
- game rule を判断しない。
- Inspector reference は原則 `[SerializeField] private`。派生 View が必要な場合だけ `protected`。
- 外へ公開する参照は setter を出さず expression-bodied read-only property を優先する。
- 必須 component には適切な `RequireComponent` を使う。

#### Presenter

- `MonoBehaviour` にしない。
- constructor injection で Model と View を接続する。
- VContainer lifecycle は `IStartable` / `ITickable` を既存例に合わせて使う。
- R3 subscription は `CompositeDisposable` に集約し `IDisposable` で解放する。
- orchestration は Presenter に置いてよい。長い sequence は処理直前に短い日本語コメントを置く。

#### LifetimeScope

VContainer `LifetimeScope` を composition root とする。

- Scene/Prefab 上の View: `RegisterComponent`。
- status Asset: `RegisterInstance<I...Status>`。
- Model: `Register<T>(Lifetime.Singleton|Scoped)` を既存 scope に合わせる。
- Presenter: `RegisterEntryPoint<T>()`。
- 同型複数登録: 既存例に合わせて `Keyed` / `WithParameter`。
- 登録順は既存の `// View`、`// ScritableObject`、`// Model`、`// Presenter` 等の grouping を維持する。
- Service Locator、runtime `Find*` を DI の代替として導入しない。
- 新しい global singleton / `Manager` を局所的な都合で追加しない。

### 4.2 Data / Status

設定値は既存語彙に合わせて `I<Feature>Status` と `<Feature>ConstStatusInfo` / `<Feature>InitializeStatusInfo` の境界を使う。

- ScriptableObject は status interface を実装する。
- Model は concrete ScriptableObject ではなく interface に依存する。
- runtime override が必要なら Pure C# の `<Feature>RuntimeInitializeStatus` で同じ interface を実装する。
- `CreateAssetMenu` は既存の `Game/<Feature>/<TypeName>` 形式へ合わせる。
- Inspector 値には意味のある日本語 `Tooltip` と妥当な default を付ける。

実行状態は既存パターンに合わせて分離する。

- `<Feature>Data`: Model 内の mutable state。
- `<Feature>DataPack`: 外部へ渡す get-only snapshot。新規では可能なら `readonly struct`。
- Model からの公開境界は `GetDataPack()`。

既存の `Scritable` / `scritableObjects` という綴りは互換語彙として保持し、平行して `Scriptable` フォルダを新設しない。

### 4.3 R3

- 継続的通知は private readonly `Subject<T>` を source owner とする既存形を優先する。
- 外部へは `Observable<T>` を公開し、外部から `OnNext` できない境界を保つ。
- Presenter subscription は `CompositeDisposable` へ集約する。
- 同一 state への再設定は不要な通知を出さず、既存例では `bool` または no-op で扱う。
- 近傍実装が R3 の箇所を、新規 C# event に理由なく置換しない。

### 4.4 Async / lifetime

- async は UniTask を優先する。既存 coroutine 専用経路を維持する場合を除き、新しい Coroutine を増やさない。
- public async API は `Async` suffix と `CancellationToken` を持たせる。
- `MonoBehaviour` の async 処理は caller token と `GetCancellationTokenOnDestroy()` を link する既存方式へ合わせる。
- Presenter が状態遷移 token を所有する場合、旧 `CancellationTokenSource` を cancel / dispose してから replacement を作る。
- expected `OperationCanceledException` は cancellation filter で正常終了として扱う。
- 一時変更した操作可否、Collider、movement、spawn pause 等は `finally` で復元する。
- DOTween を使う既存領域では対象 `DOKill()` と `SetLink(..., KillOnDestroy)` の lifetime 管理へ合わせる。
- 並行実行を許可しない flow は、既存の `SemaphoreSlim` / R3 `AwaitOperation.Drop` 等を優先する。

### 4.5 Input

新規入力は Unity Input System を使い、旧 `Input` API を混在させない。既存例では `Mouse.current`、`Touchscreen.current`、`Pointer.current` を参照する。

### 4.6 Naming

- class / struct / interface / enum: `PascalCase`。
- interface: `I` prefix。
- private / protected field: `_camelCase`。
- public property / method: `PascalCase`。
- local / parameter: `camelCase`。
- bool: `Is...` / `Can...` / `Has...` / `was...` / `is...` のように真偽が読める名前。
- async method: `Async` suffix。
- state enum: `...Kind`。
- configuration: `...Status` / `...StatusInfo`。
- snapshot: `...DataPack`。
- method は動詞から開始し、既存語彙 `Apply`, `Change`, `Reset`, `Move`, `Play`, `Show`, `Hide`, `Visible`, `Disable`, `GetDataPack` を優先する。
- **新規ゲームコードの定数は PascalCase を標準**とする。既存 `UPPER_SNAKE_CASE` は互換のため rename しない。

次の既存語彙は誤字に見えても、依頼がない限り preserve-only とする。

`Scritable`, `scritableObjects`, `Aplly`, `Culculate`, `MaterDistance`, `Seacret`, `CanWachMap`, `CachedEndingWach`, `Searvis`, `pronpt`, `niavi`, `IplayableDataPack`。

同じ機能領域に既存語彙がある場合、新規の隣接 API でもその vocabulary を優先し、重複する「修正版 API」を生やさない。

### 4.7 Formatting

- Allman braces。
- indent は spaces 4。
- `Assets/!lumirr/scripts` の新規 game code は namespace なしが既定。
- `RaruLib` は既存の block namespace `namespace RaruLib { ... }` に合わせる。
- file-scoped namespace は導入しない。
- local type は明示型を優先する。`var` は右辺から型が明白な一時値や外部 API result に限定する。
- target-typed `new()` は左辺で型が明白な field initialization 等では使用可。
- 小さな get-only property は expression-bodied member を優先する。
- guard clause を使い深い nesting を避ける。
- float literal は `0.0f`, `1.0f`, `0.5f` のように型を明示する形を優先する。
- `#region` は新規導入しない。
- 関係ない範囲を formatter で一括変更しない。

### 4.8 Comments

コメントは日本語を基本とし、API 名、型名、technical term は英語表記を維持する。

- 長い orchestration / calculation の直前に、ゲーム上の意味や処理段階を示す短い日本語コメントを置く。
- public contract、lifecycle ownership、非自明な state transition、compatibility behavior には 1〜2 文の XML `<summary>` を付ける。
- 自明な getter / private method へ機械的に summary を増やさない。
- 代入文を言い換えるだけのコメントを書かない。
- bare `TODO` / `FIXME`、大きな commented-out implementation、新しい mojibake を残さない。
- 既存 mojibake は無関係な変更で触らない。

### 4.9 Null / error handling

- Model の public boundary で必須引数が null なら `ArgumentNullException`、範囲不正なら `ArgumentOutOfRangeException` を使う。
- 正常な same-state / no-op は `false` または early return で表現してよい。
- required SerializeField の欠落を `?.`、silent return、runtime search、fallback auto-generation で隠さない。
- Unity object destroy race では Unity null semantics を考慮する。
- `Debug.Log` を恒常的な control flow にしない。
- exception message には期待値と対象を含める。

### 4.10 Test style

- Pure C# logic: EditMode `[Test]`。
- Unity lifecycle / destroy 中 await 等: `[UnityTest]`。
- test name: `Method_Condition_ExpectedResult`。
- assertion: NUnit `Assert.That`。
- test double: test class 内の `private sealed class ...Stub`。
- Unity object: `try/finally` で `Object.DestroyImmediate`。
- disposable: `using` または `finally` で破棄。
- SerializeField View: `SerializedObject`、必要な横断 test では小さな reflection helper。
- float: `Within`、vector/state: `Is.EqualTo`、collection: `Has.Count`。
- random logic: same seed determinism と boundary を検証する。

---

## 5. Unity authoring と runtime generation

- Inspector wiring は契約であり、required reference を runtime repair しない。
- Scene/Prefab に存在するべき persistent View / UI / anchor / container / management object を `Awake` / `Start` / `OnEnable` で生成・配置・親子付けしない。
- runtime instantiation は、bullet、enemy、list item、effect 等の量産対象、または仕様上動的生成が本質なものに限定する。
- SerializeField rename は Prefab/Scene の serialized compatibility を考慮する。必要なら `FormerlySerializedAs` 等の既存方式を検討する。

---

## 6. Verification Contract

変更種別ごとの最低 gate は [`docs/VERIFICATION_MATRIX.md`](docs/VERIFICATION_MATRIX.md) に従う。

共通原則:

1. source change 後は Unity に変更を読み込ませる。
2. compile/import error を確認する。
3. 最も狭い relevant test を先に実行する。
4. change が runtime に影響する場合は対象 flow を実行する。
5. failure を修正したら、失敗した gate だけでなく、それ以前に通した relevant gate も必要範囲で再実行する。
6. test output は exit code だけでなく report artifact を確認する。
7. runtime verification は「Play Mode に入った」だけではなく、変更した behavior の結果を観察する。
8. UI / scene layout は必要に応じて hierarchy/state query と screenshot を組み合わせる。

CLI の代表例:

```bash
# Targeted tests
unity test <project-path> --mode EditMode --filter "<test-filter>" --output <report.xml>
unity test <project-path> --mode PlayMode --filter "<test-filter>" --output <report.xml>

# Batch project load / custom verification when appropriate
unity run <project-path> --timeout 300 -- -nographics -logFile <log-path>

# Live Editor discovery
unity command --project-path <project-path> --format json
```

上記は例であり、インストール済み CLI の `--help` と project-specific command schema を優先する。

---

## 7. 禁止事項

- 一般的に綺麗という理由だけで MVP を別 architecture に置換しない。
- 新しい namespace、asmdef、record、DI abstraction、Manager、global singleton を局所都合で増やさない。
- `Data` / `DataPack` を別 DTO/mapping framework に置換しない。
- VContainer registration を runtime `Find*` へ迂回しない。
- R3 を C# event、UniTask を Coroutine に理由なく置換しない。
- required SerializeField の設定不足を defensive null guard で隠さない。
- live Editor が利用可能な状態で `.unity`, `.prefab`, `.asset` を raw YAML として手編集しない。
- command 名や parameter schema を記憶だけで決め打ちしない。
- test failure を「既存っぽい」で未調査のまま無視しない。
- 未実行の runtime behavior を「確認済み」と報告しない。

---

## 8. 最終報告契約

最終報告は簡潔でよいが、次を含める。

### Status

`VERIFIED` / `PARTIALLY_VERIFIED` / `BLOCKED` のいずれか。

### Changed

変更した behavior と主要 file / Scene / Prefab。

### Verification

- 実行した Unity CLI command または project-specific command。
- compile/import 結果。
- test mode / filter / pass-fail result。
- runtime で確認した具体的 behavior。
- Scene/Prefab/Serialize wiring を確認した場合はその結果。

### Residual

- 未実行の gate と理由。
- 既存の無関係な error/warning が検証へ影響した場合の事実。
- 残る compatibility / migration / runtime risk。

**required gate が 1 つでも未確認なら `VERIFIED` を使用しない。**

---

## 9. 関連 Skills

- 実装前の計画: [`skills/unity-implementation-planner/SKILL.md`](skills/unity-implementation-planner/SKILL.md)
- 実装から UnityCLI 検証、最終報告まで: [`skills/unity-implementation-executor/SKILL.md`](skills/unity-implementation-executor/SKILL.md)

UnityCLI 自体の command syntax、install、Editor/Project 管理、Pipeline command は、利用環境に Unity 公式 `unity-cli` skill がある場合はそれを参照し、最終的には `unity --help` / `unity <command> --help` / `unity command --format json` を runtime source of truth とする。
