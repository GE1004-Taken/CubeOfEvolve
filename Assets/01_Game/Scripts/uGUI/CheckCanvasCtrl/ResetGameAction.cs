using UnityEngine;

[CreateAssetMenu(fileName = "ResetCheckAction", menuName = "UI/Check Action/Reset")]
public class ResetGameAction : ScriptableObject, ICheckAction
{
    /// <summary>
    /// 「はい」ボタンが押された際の処理：ゲームデータをリセットします。
    /// </summary>
    public void OnYes()
    {
        // ここにゲームデータのリセット処理を記述します。
        // 例：PlayerPrefs.DeleteAll(); や、セーブデータのファイルを削除するなど。
    }

    /// <summary>
    /// 「いいえ」ボタンが押された際の処理：ゲームデータのリセットをキャンセルします。
    /// </summary>
    public void OnNo()
    {
        Debug.Log("ゲームデータリセットをキャンセル");
    }
}