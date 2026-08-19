# NuiIndicator コーディング再現ガイド

この文書は、AI がこのプロジェクトの作者と同じ分離・語彙・コード形状で実装するための実行規則である。ここに記録した判断は、この snapshot では作者の意図として真である。新しい反証がコード、Asset、テストから得られない限り、一般的なベストプラクティスへ置き換えず、コードの見た目と責務をこの規則へ合わせる。ただし、ルート `AGENTS.md` の互換性・検証・禁止事項は常に守る。

## 1. 最初に採用する設計

ゲーム機能は、基本的に機能フォルダの中で `Model`、`Presenter`、`View` に分ける。さらに gameplay object / interactive UI は **runtime instance ごとに独立した MVP state/lifetime** を持たせる。同じ class を複数 instance で再利用してよいが、mutable Model / Presenter instance を共有しない。

```text
<Feature>/
├── <Feature>Model.cs
├── <Feature>Presenter.cs
├── <Feature>View.cs
├── Interface/
│   ├── I<Feature>View.cs
│   └── I<Feature>ConstStatus.cs
└── Scritable/
    └── <Feature>ConstStatusInfo.cs
```

実在する綴り `Scritable` と `scritableObjects` は、既存フォルダとの連続性を保つためそのまま使う。新しい別綴りの並行フォルダを作らない。

### Instance / GameLoop boundary

- player、enemy、bullet、item、obstacle、stateful list item、interactive panel 等、独立して生成・破棄される単位に MVP を割り当てる。
- `GameLoop*` は game-wide phase、start/end、spawn/despawn request、aggregate result だけを担当する。
- movement、HP、collision、individual UI state、individual input reaction を `GameLoop*` へ集約しない。
- instance Model / Presenter は Singleton にせず、その Prefab instance と同じ lifetime で破棄される形を使う。

### Model

- Unity の画面操作を置かず、状態、計算、判定、乱数、状態遷移を担当する。
- 依存はコンストラクタから受け取る。
- Unity 非依存の座標計算では `System.Numerics.Vector2` を優先する。
- 外部へ可変状態そのものを渡さず、`GetDataPack()` で値のスナップショットを返す。
- 複数購読や Subject を所有するなら `IDisposable` を実装し、`Dispose()` で破棄する。
- 同一 seed と入力から同一結果になるロジックは、決定的な Pure C# として保つ。

### View

- `MonoBehaviour` と Inspector 参照、Unity API、Transform、Sprite、Canvas、Button、入力イベントを担当する。
- View 内でゲームルールを判断しない。状態を表示へ反映し、操作を `Observable` または小さなメソッドとして公開する。
- Inspector 参照は `[SerializeField] private` または派生 View が必要なら `[SerializeField] protected` にする。
- 外へ公開する参照は setter を出さず、式形式の読み取りプロパティにする。
- 必須 Component は `RequireComponent` を付ける。

### Presenter

- Model と View をコンストラクタ注入で接続する。
- 毎 frame の接続は `ITickable`、開始時の購読は `IStartable` を使う。
- R3 の購読を持つなら `CompositeDisposable` に `AddTo` し、`IDisposable` で解放する。
- Presenter 自身は `MonoBehaviour` にしない。
- UI、Model、外部 command の順序を組み立てる orchestration は Presenter に置いてよい。長いシナリオは、処理の直前に短い日本語コメントを置く。

### LifetimeScope

- VContainer の `LifetimeScope` を composition root とする。
- 登録は `// View`、`// ScritableObject`、`// Model`、`// Presenter`、必要なら service/feature のまとまりで並べる。
- Scene/Prefab 上の View は `RegisterComponent`、設定 Asset は `RegisterInstance<I...Status>`、Model は `Register<T>(Lifetime.Singleton|Scoped)`、Presenter は `RegisterEntryPoint<T>()` を使う。
- 同型を複数登録する場合は文字列 key と `Keyed` / `WithParameter` を使う。既存例は `Item_100` と `Item_200`。
- Service Locator、実行時の `Find*`、新しい global singleton は追加しない。

## 2. データ設計

### 設定値

調整値は `ScriptableObject` の `<Feature>ConstStatusInfo` または `<Feature>InitializeStatusInfo` に置く。runtime instance 固有の数値は、その instance の `*Info` に集約する。既存 `*StatusInfo` が同じ役割なら重複する `*Info` を作らず、その class を利用する。

```csharp
[CreateAssetMenu(
    fileName = nameof(SampleConstStatusInfo),
    menuName = "Game/Sample/" + nameof(SampleConstStatusInfo))]
public class SampleConstStatusInfo : ScriptableObject, ISampleConstStatus
{
    [SerializeField, Tooltip("判定半径")] private float _radius = 1.0f;
    public float Radius => _radius;
}
```

- Model は具体的な ScriptableObject ではなく `I*Status` を受け取る。
- Interface は同じ値を test stub や Runtime 用 data class から渡せる境界として使う。
- 実行中だけ差し替える初期値は `<Feature>RuntimeInitializeStatus` の Pure C# class にする。
- `CreateAssetMenu` は `Game/<Feature>/<TypeName>` の形にする。
- Inspector 値には意味が伝わる日本語 `Tooltip` と妥当な初期値を置く。

### 実行状態とスナップショット

状態を次の二つに分ける。

- `<Feature>Data`: Model 内部の可変状態。既存コードに合わせて `_position` などの field を Model が更新する。
- `<Feature>DataPack`: 外部へ渡す読み取り用 value。constructor と get-only property を持つ `struct`、新規では可能なら `readonly struct`。

```csharp
public readonly struct SampleDataPack
{
    public Vector2 Position { get; }

    public SampleDataPack(Vector2 position)
    {
        Position = position;
    }
}

public class SampleData
{
    public Vector2 _position;

    public SampleData(Vector2 position)
    {
        _position = position;
    }
}
```

Model は `GetDataPack()` を作り、Presenter は DataPack を受け取って Unity 型へ変換する。このプロジェクトでは Unity 層の `UnityEngine.Vector2` と計算層の `System.Numerics.Vector2` を明示して使い分ける。同名衝突時は完全修飾名か `NumericsVector2` alias を使う。

### 通知

- 継続的な状態通知は R3 の `Subject<T>` を private readonly field で所有する。
- 外部には `Observable<T>` を式形式で公開する。
- 通知元だけが `OnNext` できる形を保つ。
- 同じ値への状態変更は通知せず、`bool` で変更の有無を返すパターンを優先する。

### 永続化

現状の小さな解除フラグは `PlayerPrefs` の定数 key と `GetInt` / `SetInt` で扱う。既存 key の綴りは保存互換なので直さない。構造化データを新規に保存する必要が出た場合は、この単純な方式を無理に拡大せず versioning の仕様を先に決める。

## 3. 非同期、入力、寿命

- 非同期は Coroutine ではなく `UniTask` を使う。
- public async API は `...Async`、CancellationToken 引数を持たせる。
- `MonoBehaviour` の async 処理では、呼び出し token と `this.GetCancellationTokenOnDestroy()` を linked token にする。
- Presenter の状態遷移は所有する `CancellationTokenSource` を cancel/dispose してから新しくする。
- 終了や状態変更に伴う `OperationCanceledException` は cancellation filter で正常終了として扱う。
- `finally` で一時的に変更した操作可否、spawn pause、Collider、移動状態を復元する。
- DOTween は対象へ `DOKill()` を行い、`SetLink(gameObject, LinkBehaviour.KillOnDestroy)` を付ける。
- Input は Input System (`Mouse.current`, `Touchscreen.current`, `Pointer.current`) を使い、旧 `Input` API を混在させない。
- 並行実行を許可しないシナリオには `SemaphoreSlim` や R3 の `AwaitOperation.Drop` を使う既存方式へ合わせる。

## 4. 命名規則

### 型とメンバー

- class / struct / interface / enum: PascalCase。
- interface: `I` 接頭辞。
- private / protected field: `_camelCase`。
- public property / method: PascalCase。
- local / parameter: camelCase。
- bool: `Is...`、`Can...`、`Has...`、`was...` のように真偽が読める名前。
- 定数: 新規ゲームコードでは PascalCase を標準とする。ただし既存の `UPPER_SNAKE_CASE` は rename しない。
- interface 実装の読み取り property は `public T Name => _name;` を好む。
- メソッド名は動詞から始める。既存語彙は `Apply...`, `Change...`, `Reset`, `Visible...`, `Disable...`, `Move...`, `Play...`, `GetDataPack` が中心。
- 非同期メソッドは `Async` suffix。
- 状態 enum は `...Kind`、設定は `...Status` / `...StatusInfo`、読み取りスナップショットは `...DataPack`。

### 互換のため変更しない綴り

次は誤字に見えても、既存 API、Serialize field、Asset/Folder、外部データの語彙として残す。依頼が明示されない限り rename しない。

- `Scritable`, `scritableObjects`
- `Aplly`
- `Culculate`
- `MaterDistance`
- `Seacret`
- `CanWachMap`, `CachedEndingWach`
- `Searvis`
- `pronpt`, `niavi`
- `IplayableDataPack`
- Scene 名や key の `Item_100`, `Item_200`, `Title`, `Game`

新規の独立した識別子は、同じ機能領域に既存語彙があればその綴りまで再利用する。既存語彙と関係しない概念だけ正しい英語にする。既存 API の隣へ重複する「修正版」を生やさない。

## 5. コードの形

- `using` はファイル先頭。System、third-party、Unity、project の厳密な空行グループ化は強制しない。
- `Assets/!lumirr/scripts` のゲームコードは namespace なしを既定とする。
- `RaruLib` 内は `namespace RaruLib { ... }` の block namespace に合わせる。file-scoped namespace は使わない。
- brace は Allman 形式、indent は space 4個。
- 型は省略せず明示する。`var` は右辺から型が自明な一時値や外部 API 戻り値に限定する。
- target-typed `new()` は field 初期化や型が左辺で明白な箇所に使ってよい。
- 小さな get-only property は expression-bodied member にする。
- guard clause と早期 return を使い、深いネストを避ける。
- 1行 `if (...) return;` は単純な guard で既存コードにあるが、新規の複雑な副作用は braces 付きにする。
- `*Info` / test 等、numeric literal が許される箇所では `0.0f`, `1.0f` のように型を明示する。production behavior code の magic number 禁止を優先する。
- 関連する小型 `Data` / `DataPack` / enum は主要 class と同じファイルに置いてよい。再利用境界の interface は専用ファイルへ分ける。
- `#region` は使わない。大きな既存ファイルでは `/********/` separator があるが、新規では責務分割を優先する。

## 6. コメントの書き方

新規・変更コードでは、method と variable に **1〜15文字程度**の短いコメントを付ける。日本語を基本とし、API 名・型名・technical term は英単語を維持する。

### 配置

- class field / class variable: 宣言行の末尾へ文末コメント。
- method: declaration の直前へ行コメント。
- local / loop variable: declaration / loop statement の直前へ行コメント。

```csharp
[SerializeField] private EnemyInfo _enemyInfo; // 敵設定

// 移動更新
public void Move()
{
    // 移動距離
    float moveDistance = _enemyInfo.MoveDistance;
}
```

短い名詞句・動作句を使う。説明が長くなる場合は、名前と責務分割を先に改善する。既存 XML summary は無関係な変更で削除しないが、新規箇所へ長い summary を機械的に追加しない。

### 避けるもの

- 新しい文字化けコメント。
- コメントアウトした大きな旧実装。
- TODO/FIXME だけを残すこと。
- 16文字以上の説明を通常コメントとして連発すること。

既存の文字化けコメントは無関係な変更で触れない。対象行を実際に変更する場合のみ、意図を確認できる範囲で UTF-8 の日本語へ置き換える。

## 7. マジックナンバー

production behavior code では magic number を禁止する。gameplay / UI behavior に意味を持つ numeric value は、その instance の `*Info` から取得する。

- speed、duration、distance、count、threshold、alpha、scale、offset、score、interval 等を Model / View / Presenter / GameLoop に直接書かない。
- numeric literal を `const` / `static readonly` に移しただけでは解決としない。instance authoring value は `*Info` / status Asset へ置く。
- 既存 `*ConstStatusInfo` / `*InitializeStatusInfo` が該当 instance の設定源なら、そこへ field/property を追加する。
- test fixture は意味のある名前へ束縛し、production tuning 値とは分離する。
- tweet 文面や `$"{index}ポイント"` 等の string / string interpolation は source に直接書いてよい。

## 8. エラー処理と logging

- test 系 script 外で `InvalidOperationException` を使用しない。
- production Model / Pure C# game logic は expected error で throw せず、return / `false` / no-op / 既存 `Try...` 形で処理する。
- Model は `UnityEngine.Debug` に依存しない。
- View、Presenter、その他 Unity-dependent script で error を log する場合は `Debug.Log` を使用する。
- `Debug.Log` は必ず `#if UNITY_EDITOR` / `#endif` で囲む。

```csharp
#if UNITY_EDITOR
UnityEngine.Debug.Log("状態不正");
#endif
```

- required Inspector reference の欠落を `?.`、runtime `Find*`、`AddComponent`、auto-generation で隠さない。Prefab / Inspector を修正する。
- Unity object の destroy 競合だけは Unity null semantics を考慮する。

## 9. テストの書き方

- Pure C# は EditMode `[Test]`、Unity lifecycle や destroy 中 await は `[UnityTest]`。
- test 名は `Method_Condition_ExpectedResult`。
- NUnit の `Assert.That` を使う。
- Unity object は `try/finally` で `DestroyImmediate`、disposable は finally または `using` で破棄する。
- interface 境界は test class 内の `private sealed class ...Stub` で差し替える。
- SerializeField の View は `SerializedObject`、必要な横断テストでは小さな reflection helper で参照を設定する。
- float は `Within`、vector/state は `Is.EqualTo`、collection は `Has.Count` を使う。
- 乱数ロジックは同 seed の決定性と境界を検証する。

## 10. 新規機能の実装順

1. `Assets/!MyAssets/Object/Prefab` の既存 grouping / naming を確認し、対象 instance の Prefab location を決める。
2. 機能に最も近い既存 Model/Presenter/View を選ぶ。ただし `GameLoop*` 集中型の旧実装は instance responsibility の exemplar にしない。
3. runtime instance ごとの MVP boundary と lifetime を決める。
4. instance 数値設定を収める `*Info` / 既存 `*StatusInfo` を決める。
5. Pure C# の `Data` / `DataPack` / Model を作る。
6. Prefab authoring と Unity API だけを扱う View を作る。
7. Presenter で instance 内を接続し、R3 購読と cancellation の ownership を決める。
8. VContainer へ instance state が共有されない lifetime で登録する。
9. Prefab/Scene の SerializeField を Unity Editor で割り当てる。Prefab で解決できる layout/visual/component 設定を Script に書かない。
10. method / variable の短い comment、magic number、logging policy を source review する。
11. 最も近い EditMode/PlayMode test を追加する。
12. compile、Console、test、Prefab source、複数 instance の state isolation、必要な runtime 導線を確認する。

## 11. AI 向け禁止事項

- gameplay object / interactive UI を Prefab なしで runtime construction しない。
- Prefab で変更できる hierarchy / layout / visual / component setting を Script で hard-code / repair しない。
- instance-specific responsibility を `GameLoop*` に集約しない。
- production behavior code に magic number を書かない。
- test script 外で `InvalidOperationException` を投げない。
- 「一般的に綺麗」という理由だけで namespace、asmdef、record、DI abstraction、Manager を導入しない。
- MVP を別 architecture に置換しない。
- `Data` / `DataPack` を DTO library や mapper framework に置換しない。
- VContainer 登録を Scene 内検索へ迂回しない。
- R3 を C# event、UniTask を Coroutine へ理由なく置き換えない。
- SerializeField、PlayerPrefs key、enum 順、Prefab/Scene 名、既存 typo を一括 rename しない。
- 作者の既存コード全体を formatter で均一化しない。
- テストだけ新しい命名流派にしない。

## 12. 最小テンプレート

```csharp
using System;
using R3;

public readonly struct SampleDataPack
{
    public float Value { get; }

    // data作成
    public SampleDataPack(float value)
    {
        Value = value;
    }
}

public sealed class SampleModel : IDisposable
{
    private readonly ISampleConstStatus _status; // 設定値
    private readonly Subject<SampleDataPack> _changed = new(); // 変更通知
    private float _value; // 現在値

    public Observable<SampleDataPack> Changed => _changed;

    // Model作成
    public SampleModel(ISampleConstStatus status)
    {
        _status = status;
    }

    // 値変更
    public bool ChangeValue(float value)
    {
        if (_value == value)
        {
            return false;
        }

        _value = value;
        _changed.OnNext(GetDataPack());
        return true;
    }

    // data取得
    public SampleDataPack GetDataPack()
    {
        return new SampleDataPack(_value);
    }

    // 通知破棄
    public void Dispose()
    {
        _changed.Dispose();
    }
}
```

このテンプレートを機械的に全機能へ適用するのではなく、最寄りの既存実装が持つ責務と寿命へ合わせて削る。
