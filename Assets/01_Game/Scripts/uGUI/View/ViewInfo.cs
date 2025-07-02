using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
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
        [SerializeField] private TextMeshProUGUI _atkText; // モジュールの攻撃力を表示するテキスト（例）。
        [SerializeField] private TextMeshProUGUI _rapidText; // モジュールの攻撃速度を表示するテキスト（例）。
        [SerializeField] private TextMeshProUGUI _priceText; // モジュールの価格を表示するテキスト（例）。

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

            // 各テキストコンポーネントに値を代入
            if (_levelText != null) _levelText.text = $"{runtimeData.CurrentLevelValue}";
            if (_quantityText != null) _quantityText.text = $"{runtimeData.CurrentQuantityValue}";
            if (_unitNameText != null) _unitNameText.text = masterData.ViewName; // マスターデータの表示名。
            // ATKの計算例: 攻撃力はマスターデータとレベルから計算されます。
            // if (_atkText != null) _atkText.text = $"ATK: {masterData.AttackPower + (runtimeData.CurrentLevelValue * 5)}";
            // SPDの例: 速度はマスターデータから取得されます。
            // if (_rapidText != null) _rapidText.text = $"SPD: {masterData.Speed}";
            if (_priceText != null) _priceText.text = $"{masterData.BasePrice}";

            // アイテム画像とブロックサイズ画像は、`ModuleData` に `Sprite` などの画像情報が含まれている場合に設定します。
            if (_itemImage != null) _itemImage.sprite = masterData.MainSprite;
            // if (_blockSizeImage != null) _blockSizeImage.sprite = masterData.BlockSprite;
        }
    }
}