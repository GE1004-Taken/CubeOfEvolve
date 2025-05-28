using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using R3;
using System;
using UnityEngine;

namespace App.GameSystem.Modules
{
    /// <summary>
    /// ゲーム中に動的に変化するモジュールデータを管理するクラス。
    /// マスターデータ (ModuleData) を基に初期化され、レベルや数量などの状態を保持します。
    /// </summary>
    [Serializable]
    public class RuntimeModuleData
    {
        // ----- Property (公開プロパティ)
        public int Id { get; private set; } // モジュールの一意なID。

        // ----- ReactiveProperty (リアクティブプロパティ)
        [SerializeField]
        private ReactiveProperty<int> _currentLevel; // 現在のレベルを管理するReactiveProperty。
        public ReadOnlyReactiveProperty<int> Level => _currentLevel; // 外部公開用の読み取り専用レベルプロパティ。
        public int CurrentLevelValue => _currentLevel.Value; // 現在のレベルの直接値。

        [SerializeField]
        private ReactiveProperty<int> _quantity; // 現在の数量を管理するReactiveProperty。
        public ReadOnlyReactiveProperty<int> Quantity => _quantity; // 外部公開用の読み取り専用数量プロパティ。
        public int CurrentQuantityValue => _quantity.Value; // 現在の数量の直接値。

        // ----- Constructor (コンストラクタ)
        /// <summary>
        /// ModuleDataマスターデータからRuntimeModuleDataのインスタンスを初期化します。
        /// </summary>
        /// <param name="masterData">モジュールのマスターデータ。</param>
        public RuntimeModuleData(ModuleData masterData)
        {
            Id = masterData.Id; // マスターデータからIDを設定。
            _currentLevel = new ReactiveProperty<int>(0); // 初期レベルは0で設定。
            _quantity = new ReactiveProperty<int>(0); // 初期数量は0で設定。
        }

        // ----- Public Methods (公開メソッド)
        /// <summary>
        /// モジュールのレベルを更新します。
        /// </summary>
        /// <param name="newLevel">設定する新しいレベル。</param>
        public void SetLevel(int newLevel)
        {
            if (newLevel < 0) newLevel = 0; // レベルが負の値にならないように制限。
            _currentLevel.Value = newLevel; // ReactivePropertyの値を更新。
        }

        /// <summary>
        /// モジュールの数量を更新します。
        /// </summary>
        /// <param name="newQuantity">設定する新しい数量。</param>
        public void SetQuantity(int newQuantity)
        {
            if (newQuantity < 0) newQuantity = 0; // 数量が負の値にならないように制限。
            _quantity.Value = newQuantity; // ReactivePropertyの値を更新。
        }

        /// <summary>
        /// モジュールのレベルを1上げます。
        /// </summary>
        public void LevelUp() => SetLevel(_currentLevel.Value + 1);

        /// <summary>
        /// モジュールの数量を指定された量だけ変更します。
        /// </summary>
        /// <param name="amount">数量の増減量。</param>
        public void ChangeQuantity(int amount) => SetQuantity(_quantity.Value + amount);
    }
}