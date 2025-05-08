using UnityEngine;

[CreateAssetMenu(fileName = "QuitGameAction", menuName = "UI/Check Actions/Quit Game")]
public class QuitGameActionSO : ScriptableObject, ICheckAction
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

[CreateAssetMenu(fileName = "ResetGameAction", menuName = "UI/Check Actions/Reset Game")]
public class ResetGameActionSO : ScriptableObject, ICheckAction
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