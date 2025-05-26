using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Detailed_View : MonoBehaviour
{
    // -----
    // -----SerializeField
    [SerializeField] private Image _itemImage;
    [SerializeField] private Image _blockSizeImage;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _unitNameText;
    [SerializeField] private TextMeshProUGUI _atkText; // 例: 攻撃力
    [SerializeField] private TextMeshProUGUI _rapidText; // 例: 攻撃速度
    [SerializeField] private TextMeshProUGUI _priceText; // 例: 価格

    // -----Public
    /// <summary>
    /// モジュール情報をUIに設定します。
    /// </summary>
    /// <param name="masterData">モジュールのマスターデータ。</param>
    /// <param name="runtimeData">モジュールのランタイムデータ。</param>
    public void SetInfo(ModuleData masterData, RuntimeModuleData runtimeData)
    {
        if (masterData == null || runtimeData == null)
        {
            Debug.LogError("MasterData or RuntimeData is null in Detailed_View.SetInfo.");
            return;
        }

        // アイテム画像代入 (ModuleDataにSpriteなどの画像情報があれば)
        // if (_itemImage != null) _itemImage.sprite = masterData.IconSprite;

        // ブロックサイズ画像代入 (もしあれば)
        // if (_blockSizeImage != null) _blockSizeImage.sprite = masterData.BlockSprite;

        // 各テキストコンポーネントに値を代入
        if (_levelText != null) _levelText.text = $"Lv: {runtimeData.CurrentLevel}";
        if (_unitNameText != null) _unitNameText.text = masterData.ViewName; // マスターデータの表示名
        //if (_atkText != null) _atkText.text = $"ATK: {masterData.AttackPower + (runtimeData.CurrentLevel * 5)}"; // 例: 攻撃力はマスターデータとレベルから計算
        //if (_rapidText != null) _rapidText.text = $"SPD: {masterData.Speed}"; // 例: 速度はマスターデータから
        if (_priceText != null) _priceText.text = $"Price: {masterData.BasePrice}";
    }
}