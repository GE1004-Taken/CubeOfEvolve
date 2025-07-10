using EasyTransition;
using UnityEngine;
using R3;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // ---------- SerializeField
    [SerializeField] private TransitionManager _transitionManager;
    [SerializeField] private TransitionSettings _transitionSettings;

    // ---------- Property
    public bool IsRunning => _transitionManager.IsRunning;

    // ---------- R3
    public Observable<Unit> OnTransitionStarted => _transitionManager.OnTransitionStarted;
    public Observable<Unit> OnTransitionHalf => _transitionManager.OnTransitionHalf;
    public Observable<Unit> OnTransitionCompleted => _transitionManager.OnTransitionCompleted;

    // ---------- Method
    /// <summary>
    /// 現在のシーンをロード
    /// </summary>
    public void ReloadScene()
    {
        if (IsRunning) return;

        _transitionManager.Transition(
            SceneManager.GetActiveScene().name,
            _transitionSettings);
        Time.timeScale = 1;
    }

    public void ReloadScene(float startDelay)
    {
        if (IsRunning) return;

        _transitionManager.Transition(
            SceneManager.GetActiveScene().name,
            _transitionSettings,
            startDelay);
    }

    public void LoadScene(
       string sceneName)
    {
        if (IsRunning) return;

        _transitionManager.Transition(sceneName, _transitionSettings);
    }

    public void LoadScene(
        int sceneIndex)
    {
        if (IsRunning) return;

        _transitionManager.Transition(sceneIndex, _transitionSettings);
    }

    public void LoadScene(
        string sceneName,
        float startDelay)
    {
        if (IsRunning) return;

        _transitionManager.Transition(sceneName, _transitionSettings, startDelay);
    }

    public void LoadScene(
        int sceneIndex,
        float startDelay)
    {
        if (IsRunning) return;

        _transitionManager.Transition(sceneIndex, _transitionSettings, startDelay);
    }
}
