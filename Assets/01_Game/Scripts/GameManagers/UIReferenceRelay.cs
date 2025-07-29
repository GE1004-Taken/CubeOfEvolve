using Assets.AT;
using Assets.IGC2025.Scripts.GameManagers;
using UnityEngine;

public class UIReferenceRelay : MonoBehaviour
{
    /// <summary>
    /// シーン上ボタンからのGameStateの変更(直前のステートに戻る)
    /// </summary>
    /// <param name="state"></param>
    [EnumAction(typeof(GameState))]
    public void OnButtonChangeGameState()
    {
        GameManager.Instance.ChangeGameState(GameManager.Instance.PrevGameState);
    }

    /// <summary>
    /// シーン上ボタンからのGameStateの変更
    /// </summary>
    /// <param name="state"></param>
    [EnumAction(typeof(GameState))]
    public void OnButtonChangeGameState(int state)
    {
        GameManager.Instance.ChangeGameState(state);
    }

    /// <summary>
    /// シーン上ボタンからのカメラの変更
    /// </summary>
    /// <param name="targetCameraKey"></param>
    public void OnButtonChangeCamera(string targetCameraKey)
    {
        CameraCtrlManager.Instance.ChangeCamera(targetCameraKey);
    }

    /// <summary>
    /// シーン上ボタンからの「やり直し」呼び出し
    /// </summary>
    public void OnRetryButtonPressed()
    {
        var gameManager = GameManager.Instance;
        gameManager.RequestRetry();
        gameManager.SceneLoader.ReloadScene();
    }

    /// <summary>
    /// シーン上ボタンからの「タイトルへ戻る」呼び出し
    /// </summary>
    public void OnButtonReloadScene()
    {
        var gameManager = GameManager.Instance;
        gameManager.RequestReturnToTitle();
        gameManager.SceneLoader.ReloadScene();
    }


}
