using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using Game.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.IGC2025.Scripts.View
{
    public class ViewInfo : MonoBehaviour
    {
        // ----- SerializedField
        [SerializeField] private Image _itemImage; // モジュールのアイコン画像。
        [SerializeField] private Image _blockSizeImage; // モジュールのブロックサイズを示す画像。
        [SerializeField] private TextMeshProUGUI _levelText; // モジュールのレベルを表示するテキスト。
        [SerializeField] private TextMeshProUGUI _quantityText; // モジュールのレベルを表示するテキスト。
        [SerializeField] private TextMeshProUGUI _unitNameText; // モジュールの名前を表示するテキスト。
        [SerializeField] private TextMeshProUGUI _priceText; // モジュールの価格を表示するテキスト

        // ----- Public
        /// <summary>
        /// モジュール情報をUIに設定します。
        /// </summary>
        /// <param name="masterData">モジュールのマスターデータ。</param>
        /// <param name="runtimeData">モジュールのランタイムデータ。</param>
        public void SetInfo(ModuleData masterData, RuntimeModuleData runtimeData)
        {
            if (masterData == null || runtimeData == null)
            {
                Debug.LogError("Detailed_View.SetInfoでMasterDataまたはRuntimeDataがnullです。");
                return;
            }

            // スケーリング後の値を計算
            int level = runtimeData.CurrentLevelValue;

            float scaledPrice = StateValueCalculator.CalcStateValue(
                baseValue: masterData.BasePrice,
                currentLevel: level,
                maxLevel: 5,
                maxRate: 0.5f
            );

            // 各テキストコンポーネントに値を代入
            if (_levelText != null) _levelText.text = $"{level}";
            if (_quantityText != null) _quantityText.text = $"{runtimeData.CurrentQuantityValue}";
            if (_unitNameText != null) _unitNameText.text = masterData.ViewName; // マスターデータの表示名。
            if (_priceText != null) _priceText.text = $"{(int)scaledPrice}";

            // アイテム画像とブロックサイズ画像は、`ModuleData` に `Sprite` などの画像情報が含まれている場合に設定します。
            if (_itemImage != null) _itemImage.sprite = masterData.MainSprite;
            if (_blockSizeImage != null) _blockSizeImage.sprite = masterData.BlockSprite;

        }
    }
}