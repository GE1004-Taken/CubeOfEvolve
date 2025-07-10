using Assets.IGC2025.Scripts.GameManagers;
using UnityEngine;

public class UIReferenceRelay : MonoBehaviour
{
    [EnumAction(typeof(GameState))]
    public void OnButtonChangeGameState(int state)
    {
        GameManager.Instance.ChangeGameState(state);
    }

    /// <summary>
    /// シーン上ボタンからの「やり直し」呼び出し
    /// </summary>
    public void OnRetryButtonPressed()
    {
        GameManager.Instance.RequestRetry();
    }

    /// <summary>
    /// シーン上ボタンからの「タイトルへ戻る」呼び出し
    /// </summary>
    public void OnButtonReloadScene()
    {
        GameManager.Instance.SceneLoader.ReloadScene();
    }
}
