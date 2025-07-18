# Ohirun Bot

お昼が決められない人のための Discord Bot です。登録した店舗と食べ物からランダムに組み合わせを提案してくれます。

## 🍽️ 特徴

- **ランダム提案**: 登録した店舗と食べ物からランダムに組み合わせを提案
- **重複回避**: 前日の提案と同じ組み合わせを避けて選択
- **ユーザー別履歴**: 各ユーザーの提案履歴を個別に管理
- **データ管理**: 店舗、食べ物、関連付けの登録・照会が可能
- **価格情報**: 店舗ごとに異なる価格情報を保存可能

## 🚀 クイックスタート

### 1. 必要な環境
- .NET 9.0
- SQLite
- Discord Bot Token

### 2. 環境設定

#### appsettings.json
```json
{
  "Discord": {
    "Token": "YOUR_DISCORD_BOT_TOKEN_HERE"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=ohirun.db"
  }
}
```

#### Bot権限
Discord Developer Portalで以下の権限を設定：
- `applications.commands` スコープ
- `Send Messages` 権限
- `Use Slash Commands` 権限

### 3. 実行方法

```bash
# 依存関係のインストール
dotnet restore

# データベースの作成・更新
dotnet ef database update

# アプリケーションの実行
dotnet run
```

## 📱 使用方法

### 基本的な使い方

1. **初期データの登録**
```bash
# 店舗を追加
/add store セブンイレブン コンビニ
/add store 吉野家 牛丼

# 食べ物を追加
/add meal おにぎり 1 ツナマヨ
/add meal 弁当 1 のり弁

# 関連付け（店舗で何が買えるか）
/link 1 1 120  # セブンイレブンでおにぎり（120円）
/link 2 2 500  # 吉野家で弁当（500円）
```

2. **お昼の提案**
```bash
/ohiru
```

3. **データの確認**
```bash
/list stores  # 店舗一覧
/list meals   # 食べ物一覧
/list links   # 関連付け一覧
```

### 詳細な使い方
詳しい使い方は [USAGE.md](USAGE.md) を参照してください。

## 🏗️ アーキテクチャ

### 技術スタック
- **フレームワーク**: .NET 9.0
- **Discord API**: Discord.Net
- **データベース**: SQLite + Entity Framework Core
- **ログ**: NLog
- **DI**: Microsoft.Extensions.DependencyInjection
- **ホスティング**: Microsoft.Extensions.Hosting

### プロジェクト構造
```
Ohirun/
├── Commands/           # スラッシュコマンド実装
│   ├── OhiruCommand.cs      # お昼提案コマンド
│   ├── AddCommand.cs        # データ追加コマンド
│   ├── LinkCommand.cs       # 関連付けコマンド
│   └── ListCommand.cs       # データ照会コマンド
├── Configuration/      # 設定関連
│   └── ApplicationConfig.cs
├── Data/              # データベース関連
│   └── ApplicationDbContext.cs
├── Models/            # データモデル
│   ├── Store.cs            # 店舗モデル
│   ├── Meal.cs             # 食べ物モデル
│   ├── FoodType.cs         # 食べ物種類モデル
│   ├── StoreMeal.cs        # 関連付けモデル
│   ├── LunchHistory.cs     # 履歴モデル
│   └── LunchDecision.cs    # 提案結果モデル
├── Services/          # ビジネスロジック
│   ├── BotService.cs           # Discord Bot メインサービス
│   ├── SlashCommandService.cs  # コマンド処理サービス
│   ├── LunchDecisionService.cs # 昼食決定サービス
│   └── DataManagementService.cs # データ管理サービス
├── Migrations/        # データベースマイグレーション
├── Program.cs         # エントリーポイント
├── appsettings.json   # 設定ファイル
└── nlog.config        # ログ設定
```

### データベース設計
```
Store (店舗)
├── Id: int (PK)
├── Name: string (店舗名)
├── Genre: string (ジャンル)
└── IsActive: bool (有効フラグ)

FoodType (食べ物種類)
├── Id: int (PK)
├── Name: string (種類名: コメ/麺/パン)
└── Description: string (説明)

Meal (食べ物)
├── Id: int (PK)
├── Name: string (食べ物名)
├── FoodTypeId: int (FK)
└── Description: string (説明)

StoreMeal (関連付け)
├── StoreId: int (FK, PK)
├── MealId: int (FK, PK)
├── Price: decimal? (価格)
└── IsAvailable: bool (利用可能フラグ)

LunchHistory (履歴)
├── Id: int (PK)
├── StoreId: int (FK)
├── MealId: int (FK)
├── UserId: string (Discord ユーザーID)
├── Username: string (ユーザー名)
└── SuggestedAt: DateTime (提案日時)
```

## 🔧 開発

### 開発環境のセットアップ

1. **リポジトリのクローン**
```bash
git clone <repository-url>
cd Ohirun
```

2. **依存関係のインストール**
```bash
dotnet restore
```

3. **データベースの作成**
```bash
dotnet ef database update
```

4. **設定ファイルの作成**
`appsettings.json` を作成し、Discord Bot Token を設定

### マイグレーションの作成
```bash
# 新しいマイグレーションを作成
dotnet ef migrations add <MigrationName>

# データベースに適用
dotnet ef database update
```

### コマンドの追加
1. `Commands/` フォルダーに新しいコマンドクラスを作成
2. `ISlashCommand` インターフェースを実装
3. `SlashCommandRegistry.cs` にコマンドを登録
4. `Program.cs` の DI コンテナーに追加

## 📋 コマンド一覧

| コマンド | 説明 |
|---------|------|
| `/ohiru` | お昼の提案をランダムに表示 |
| `/add store <name> <genre>` | 店舗を追加 |
| `/add meal <name> <type> [description]` | 食べ物を追加 |
| `/link <store-id> <meal-id> [price]` | 店舗と食べ物を関連付け |
| `/list stores` | 店舗一覧を表示 |
| `/list meals` | 食べ物一覧を表示 |
| `/list links` | 関連付け一覧を表示 |

## 🐛 トラブルシューティング

### よくある問題

**「利用可能な店舗と食べ物の組み合わせがありません」**
- 店舗、食べ物、関連付けが正しく登録されているか確認
- `/list` コマンドでデータを確認

**コマンドが表示されない**
- Bot権限の確認
- `applications.commands` スコープでの招待確認
- ギルドコマンドの反映待ち

**データベースエラー**
- SQLiteファイルの権限確認
- マイグレーションの実行確認

## 🤝 コントリビューション

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## 📄 ライセンス

このプロジェクトはMITライセンスの下で公開されています。

## 🙏 謝辞

- [Discord.Net](https://github.com/discord-net/Discord.Net) - Discord API wrapper
- [Entity Framework Core](https://github.com/dotnet/efcore) - データベースORM
- [NLog](https://github.com/NLog/NLog) - ログライブラリ

## 📞 サポート

問題や質問がある場合は、GitHubのIssuesで報告してください。