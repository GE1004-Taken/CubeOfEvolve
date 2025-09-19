using UnityEngine;
using R3;

public class CursorCtrl : MonoBehaviour
{
    private void Start()
    {
        GameManager.Instance.CurrentGameState.
        Subscribe(value =>
        {
            if (value == Assets.IGC2025.Scripts.GameManagers.GameState.BATTLE)
            {
                // カーソルを消す
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;

            }
            else
            {
                // カーソルを出す
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        })
        .AddTo(this);
    }
}
