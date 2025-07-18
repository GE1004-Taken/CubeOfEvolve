using R3;
using UnityEngine;

public class StopEffect : MonoBehaviour
{
    // ---------------------------- Field
    ParticleSystem _particleSystem;

    // ---------------------------- UnityMessage
    private void Start()
    {
        _particleSystem = GetComponent<ParticleSystem>();

        GameManager.Instance.CurrentGameState
            .Subscribe(value =>
            {
                if (value == Assets.IGC2025.Scripts.GameManagers.GameState.BATTLE)
                {
                    _particleSystem.Play();
                }
                else
                {
                    _particleSystem.Pause();
                }
            })
            .AddTo(_particleSystem);
    }
}
