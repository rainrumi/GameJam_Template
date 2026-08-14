# 抽出根拠

## Snapshot

- 調査日: 2026-08-14
- Unity: 6000.3.12f1
- ゲームコード: `Assets/!lumirr/scripts`、128 files、5,021 lines
- 共有コード: `Assets/!lumirr/RaruLib/src`、45 files、2,107 lines
- Editor test: `Assets/Editor`、13 files、2,172 lines
- ゲームコードの `[SerializeField]`: 177 occurrences
- ゲームコードの namespace declaration: 0
- RaruLib の block namespace declaration: 40
- file-scoped namespace declaration: 0
- ゲームコードの XML summary: 42 occurrences
- ゲームコードの日本語行コメント: 292 occurrences
- ゲームコードの `var` local: 5 occurrences
- ゲームコードの明示型 local: 422 occurrences

数値は単純な source pattern count であり、静的解析器の semantic count ではない。スタイルの傾向を再現するための観測値として使う。

## Architecture evidence

### MVP と VContainer

- `Assets/!lumirr/scripts/Playable/player/Movement/PlayerMovementModel.cs`
- `Assets/!lumirr/scripts/Playable/player/Movement/PlayerMovementPresenter.cs`
- `Assets/!lumirr/scripts/Playable/player/Movement/PlayerMovementView.cs`
- `Assets/!lumirr/scripts/Navi/NaviModel.cs`
- `Assets/!lumirr/scripts/Navi/NaviPresenter.cs`
- `Assets/!lumirr/scripts/Navi/NaviView.cs`
- `Assets/!lumirr/scripts/Item/ItemModel.cs`
- `Assets/!lumirr/scripts/Item/ItemPresenter.cs`
- `Assets/!lumirr/scripts/Item/ItemView.cs`
- `Assets/!lumirr/scripts/VContainer/GameLifetimeScope.cs`
- `Assets/!lumirr/scripts/VContainer/ObstaclesLifetimeScope.cs`

Model は Pure C#、View は MonoBehaviour、Presenter は constructor injection と `IStartable` / `ITickable` で接続されている。LifetimeScope の登録順にも View / ScritableObject / Model / Presenter の区分が反復している。

### Status interface と ScriptableObject

- `Assets/!lumirr/scripts/Playable/player/Interface/IPlayerConstStatus.cs`
- `Assets/!lumirr/scripts/Playable/player/Scritable/PlayerConstStatusInfo.cs`
- `Assets/!lumirr/scripts/Playable/player/Interface/IPlayerInitializeStatus.cs`
- `Assets/!lumirr/scripts/Playable/player/Scritable/PlayerInitializeStatusInfo.cs`
- `Assets/!lumirr/scripts/Playable/player/Movement/PlayerRuntimeInitializeStatus.cs`
- `Assets/!lumirr/scripts/Obstacles/Interface/IObstaclesConstStatus.cs`
- `Assets/!lumirr/scripts/Obstacles/Scritable/ObstaclesConstStatusInfo.cs`

同じ interface を Inspector Asset、Runtime data、test stub が実装する。この境界により Model が Unity Asset へ直接依存せず、実行時 reset と test が可能になっている。

### Data / DataPack

- `PlayerData` / `PlayerDataPack`
- `GirlData` / `GirlDataPack`
- `ItemData` / `ItemDataPack`
- `GoalData` / `GoalDataPack`
- `NaviData` / `NaviDataPack`
- `ObstaclesData` / `ObstaclesDataPack`

内部可変データと外部読み取り snapshot の対が、複数機能で反復している。公開入口は `GetDataPack()`。

### R3 と Dispose

- `Assets/!lumirr/scripts/GameState/GameStateModel.cs`
- `Assets/!lumirr/scripts/Item/ItemResetSignal.cs`
- `Assets/!lumirr/scripts/Item/ItemPresenter.cs`
- `Assets/!lumirr/scripts/Playable/player/Movement/PlayerMovementPresenter.cs`

`Subject<T>` は private に所有し、`Observable<T>` を公開する。Presenter の購読は `CompositeDisposable` にまとめる。

### UniTask と寿命

- `Assets/!lumirr/scripts/GameState/GameStatePresenter.cs`
- `Assets/!lumirr/scripts/GameState/GameStateView.cs`
- `Assets/!lumirr/scripts/Novel/View/NovelView.cs`
- `Assets/!lumirr/scripts/Novel/Runner/NovelRunner.cs`
- `Assets/Editor/AsyncCancellationTests.cs`

linked CancellationToken、状態変更時の token 更新、cancellation filter、finally による復元、destroy 中 await の test が確認できる。

## Naming and formatting evidence

- game code には namespace がなく、RaruLib は block namespace。
- private/protected `_camelCase` はゲームコードで多数派。
- property は `public T Name => _field;` が反復。
- local は `var` より明示型が圧倒的に多い。
- enum には PascalCase 系と `UPPER_SNAKE_CASE` 系の両方があり、既存 enum の局所形式を保つ必要がある。
- `Aplly`, `Culculate`, `MaterDistance`, `Seacret`, `CanWachMap` などは API や serialized vocabulary に残る。安全な一括修正対象ではない。
- `Scritable` / `scritableObjects` はフォルダ構造と LifetimeScope コメントに定着している。

## Comment evidence

- `GameStatePresenter.cs` は長いシナリオの各操作直前に `// ノベル再生`、`// ナビの表示`、`// プレイヤー停止` のような短い日本語コメントを置く。
- `PlayerMovementModel.cs` と `NaviPresenter.cs` は計算段階を日本語で説明する。
- `GameStateModel.cs`, `ItemResetSignal.cs`, `INaviTarget.cs` は public contract の意図を XML summary にする。
- 古い source には文字化けコメントや大きな separator が残るが、新しい source では UTF-8 の日本語 summary と理由コメントが増えている。

## Test evidence

- `Assets/Editor/NaviModelTests.cs`: Pure C#、nested Stub、`Assert.That`。
- `Assets/Editor/NaviPresenterTests.cs`: View と Status の nested Stub。
- `Assets/Editor/PlayerMovementViewTests.cs`: `SerializedObject` で private SerializeField を設定し、`try/finally` で GameObject を破棄。
- `Assets/Editor/AsyncCancellationTests.cs`: `[UnityTest]`、Play Mode enter/exit、destroy 中 await。
- `Assets/Editor/DemoPlaySimulationTests.cs`: seed 決定性、統計、culture-independent output。

## Known variance

次は「作者の書き方に存在する」が、新規コードへ積極的に増やす規則にはしない。

- RaruLib の singleton と public mutable field。
- 古い non-underscored private field。
- `MAX` sentinel のある enum とない enum。
- PascalCase 定数と `UPPER_SNAKE_CASE` 定数の混在。
- XML summary のある新しい public API と、行コメントだけの古い public API。
- CRLF、LF、mixed line endings の混在。
- 文字化けした既存 Tooltip/comment。

AI は最寄りのファイルの局所形式を優先しつつ、新規ゲーム機能では `coding-style.md` の標準形を採用する。
