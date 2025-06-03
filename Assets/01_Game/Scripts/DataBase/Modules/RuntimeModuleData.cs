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
        private ReactiveProperty<int> _currentLevel; // 現在のレベルを管理するReactiveProperty。
        public ReadOnlyReactiveProperty<int> Level => _currentLevel; // 外部公開用の読み取り専用レベルプロパティ。
        public int CurrentLevelValue => _currentLevel.Value; // 現在のレベルの直接値。

        private ReactiveProperty<int> _quantity;
        public ReadOnlyReactiveProperty<int> Quantity => _quantity;
        public int CurrentQuantityValue => _quantity.Value;

        private ReactiveProperty<float> _attack;
        public ReadOnlyReactiveProperty<float> Atk => _attack;
        public float CurrentAttackValue => _attack.Value;

        private ReactiveProperty<float> _bulletSpeed;
        public ReadOnlyReactiveProperty<float> Spd => _bulletSpeed;
        public float CurrentBulletSpeedValue => _bulletSpeed.Value;

        private ReactiveProperty<float> _interval;
        public ReadOnlyReactiveProperty<float> Interval => _interval;
        public float CurrentIntervalValue => _interval.Value;

        private ReactiveProperty<float> _searchRange;
        public ReadOnlyReactiveProperty<float> SearchRange => _searchRange;
        public float CurrentSearchRangeValue => _searchRange.Value;

        // ----- Constructor (コンストラクタ)
        /// <summary>
        /// ModuleDataマスターデータからRuntimeModuleDataのインスタンスを初期化します。
        /// </summary>
        /// <param name="masterData">モジュールのマスターデータ。</param>
        public RuntimeModuleData(ModuleData masterData)
        {
            Id = masterData.Id; // マスターデータからIDを設定。
            _currentLevel = new ReactiveProperty<int>(masterData.Level); // 初期レベルはマスターデータから代入。
            _quantity = new ReactiveProperty<int>(masterData.Quantity); // 初期数量はマスターデータから代入。
            _attack = new ReactiveProperty<float>(masterData.ModuleState.Attack);
            _bulletSpeed = new ReactiveProperty<float>(masterData.ModuleState.BulletSpeed);
            _interval = new ReactiveProperty<float>(masterData.ModuleState.Interval);
            _searchRange = new ReactiveProperty<float>(masterData.ModuleState.SearchRange);
        }

        // ----- Private

        private void LevelUpBonus()
        {

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
            LevelUpBonus();
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