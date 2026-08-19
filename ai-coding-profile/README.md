# NuiIndicator AI Coding Profile

このフォルダは、AI が NuiIndicator の既存コードへ変更を加えるときに、作者の書き方へ寄せるための参照データである。

この snapshot で採用した規則は、単なる提案ではなく「作者の意図として真」として扱う。新しい反証がコード、Asset、テストから得られない限り、AI は一般的なベストプラクティスへ置き換えず、このプロファイルを再現する。

ただし、一ゲーム実装後のレビューで追加された **Prefab-first / instance-scoped MVP / short comments / no magic numbers / error-log policy** は project-wide correction であり、旧コード evidence より優先する。これらに反する旧実装は新規コードの exemplar として模倣しない。

## 読み順

1. [`coding-style.md`](coding-style.md) — 判断規則と実装テンプレート。
2. [`style-profile.json`](style-profile.json) — 機械可読な規則、語彙、根拠。
3. [`exemplars.json`](exemplars.json) — 課題別に最初に読む既存実装。
4. [`evidence.md`](evidence.md) — 規則を抽出した既存ファイルと観測値。

リポジトリ全体の安全・検証契約はルートの `AGENTS.md` が上位である。このプロファイルは、その契約を変更せず、コードの形・分離方法・語彙を補う。

## 適用範囲

- 主対象: `Assets/!lumirr/scripts`
- 共有ライブラリ: `Assets/!lumirr/RaruLib/src`
- テスト: `Assets/Editor`
- 設定 Asset: `Assets/!lumirr/object/scritableObjects`
- DI authoring: `Assets/!lumirr/scripts/VContainer` と対応 Prefab
- runtime object/UI Prefab: `Assets/!MyAssets/Object/Prefab` の既存 grouping/naming

新規ゲームコードでは `scripts` の規則を優先する。`RaruLib` の既存ファイルを局所修正するときだけ、そのファイルの古い局所スタイルを優先する。

## このデータの読み方

- `strength: required`: 新規コードで必ず再現する。
- `strength: preferred`: 衝突がなければ再現する。
- `strength: preserve_only`: 既存 API・Serialize 互換のため保持するが、新しい識別子には増やさない。
- `confidence: high`: 複数領域で反復している。
- `confidence: medium`: 有力だが例外がある。
- `confidence: low`: 局所的な癖または互換事項。

## 更新方法

主要な設計パターンが変わったときは、憶測で追記せず、`scripts`、テスト、LifetimeScope、ScriptableObject の実例を再調査する。`style-profile.json` の `snapshot` と `evidence.md` の観測値も同時に更新する。
