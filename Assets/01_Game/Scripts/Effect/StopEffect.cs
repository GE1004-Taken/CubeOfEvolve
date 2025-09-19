using R3;
using UnityEngine;
using Assets.IGC2025.Scripts.GameManagers;

public class StopEffect : MonoBehaviour
{
    // ---------------------------- Field
    private ParticleSystem _particleSystem;

    // ---------------------------- UnityMessage
    private void Start()
    {
        _particleSystem = GetComponent<ParticleSystem>();

        GameManager.Instance.CurrentGameState
            .Subscribe(value =>
            {
                if (value == GameState.BATTLE || value == GameState.GAMECLEAR)
                {
                    _particleSystem.Play();
                    gameObject.SetActive(true);
                }
                else
                {
                    _particleSystem.Pause();
                    gameObject.SetActive(false);
                }
            })
            .AddTo(_particleSystem);
    }
}
