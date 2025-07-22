using Assets.IGC2025.Scripts.GameManagers;
using R3;
using R3.Triggers;
using UnityEngine;

public class DrillRotate : MonoBehaviour
{
    private const float _rotationSpeed = 720f;

    private void Start()
    {
        // ゲームステートがBATTLEの間だけ回転
        this.UpdateAsObservable()
            .Where(_ => GameManager.Instance.CurrentGameState.CurrentValue == GameState.BATTLE)
            .Subscribe(_ =>
            {
                float y = transform.localEulerAngles.y + _rotationSpeed * Time.deltaTime;
                transform.localEulerAngles = new Vector3(0, y % 360f, 0);
            })
            .AddTo(this);
    }
}
