using Assets.IGC2025.Scripts.GameManagers;
using UnityEngine;

public class UIReferenceRelay : MonoBehaviour
{
    [EnumAction(typeof(GameState))]
    public void OnButtonChangeGameState(int state)
    {
        GameManager.Instance.ChangeGameState(state);
    }
}
