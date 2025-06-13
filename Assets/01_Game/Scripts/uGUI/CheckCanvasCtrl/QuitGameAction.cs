using UnityEngine;

[CreateAssetMenu(fileName = "QuitCheckAction", menuName = "UI/Check Action/Quit")]
public class QuitGameAction : ScriptableObject, ICheckAction
{
    /// <summary>
    /// 「はい」ボタンが押された際の処理：ゲームを終了します。
    /// </summary>
    public void OnYes()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // エディター上での実行を停止
#else
        Application.Quit(); // ビルド後のアプリケーションを終了
#endif
        Debug.Log("ゲーム終了処理");
    }

    /// <summary>
    /// 「いいえ」ボタンが押された際の処理：ゲーム終了をキャンセルします。
    /// </summary>
    public void OnNo()
    {
        Debug.Log("ゲーム終了をキャンセル");
    }
}
