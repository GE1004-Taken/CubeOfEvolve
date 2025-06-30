using UnityEngine;
using R3;
using System.Collections;
using R3.Triggers;
using Assets.IGC2025.Scripts.GameManagers;

public class TimeManager : MonoBehaviour
{
    // ---------- RP
    [SerializeField] private SerializableReactiveProperty<float> _currentTimeSecond;
    public ReadOnlyReactiveProperty<float> CurrentTimeSecond => _currentTimeSecond;

    // ---------- Event
    public void ResetTimer()
    {
        _currentTimeSecond.Value = 0f;
    }

    // ---------- Method
    private void Start()
    {
        this.UpdateAsObservable()
            .Where(_ => GameManager.Instance.CurrentGameState.CurrentValue == GameState.BATTLE)
            .Subscribe(_ =>
            {
                _currentTimeSecond.Value += Time.deltaTime;
            });
    }
}
