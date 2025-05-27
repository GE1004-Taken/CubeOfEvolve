// RuntimeModuleData.cs
using System;
using App.BaseSystem.DataStores.ScriptableObjects.Modules; // ModuleData の名前空間

/// <summary>
/// プレイヤーが所有する単一のモジュールのランタイムデータ。ゲーム中に状態が変化する。
/// </summary>
[Serializable] // このクラスをJSONなどでシリアライズ可能にする
public class RuntimeModuleData
{
    // ------------------ マスターデータから取得される不変な情報 (最小限に留める)
    public int Id { get; private set; } // モジュールのマスターデータID
    public string Name { get; set; } // モジュールの名前 (表示用、マスターデータから取得)

    // ------------------ ゲーム中に変化する情報 (RuntimeModuleData のみが保持する)
    public int CurrentLevel { get; set; } = 0; // モジュールの現在のレベル
    public int Quantity { get; set; } = 0; // 所持数 (プレイヤー特有の概念)

    // ------------------ コンストラクタ
    /// <summary>
    /// ModuleData（マスターデータ）を基に新しいランタイムモジュールを初期化します。
    /// </summary>
    /// <param name="masterData">このランタイムモジュールの元となるModuleData。</param>
    public RuntimeModuleData(ModuleData masterData)
    {
        // マスターデータからIDと名前をコピー（あるいはキャッシュ）
        Id = masterData.Id;
        Name = masterData.Name; // BaseData の Name プロパティ

        // ランタイムデータの初期値を設定
        // masterData に _initialLevel があればそれを利用
        // 今回はmasterDataにレベルがないので、初期レベルは0とします
        CurrentLevel = masterData.Level;
        Quantity = masterData.Quantity; // プレイヤーがモジュールを所有したら初期数は1とする例
    }

    /// <summary>
    /// セーブデータ（ModuleSaveState）を基にランタイムモジュールを復元します。
    /// </summary>
    /// <param name="state">ロードするモジュールのセーブデータ。</param>
    public RuntimeModuleData(ModuleSaveState state)
    {
        Id = state.id;
        CurrentLevel = state.level;
        Quantity = state.quantity;
        // Name はロード時に RuntimeModuleManager からマスターデータを参照して設定されるか、
        // 必要であればセーブデータに含めることも可能です。（IDがあれば通常は不要）
    }

    // ------------------ セーブデータ構造 (ネストされたクラスとして定義)
    // このクラスは RuntimeModuleData の状態を保存するために使われる
    [Serializable]
    public class ModuleSaveState
    {
        public int id;
        public int level;
        public int quantity;
    }
}