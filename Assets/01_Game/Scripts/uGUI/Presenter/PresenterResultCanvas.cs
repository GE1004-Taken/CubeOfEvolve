using Assets.IGC2025.Scripts.GameManagers;
using Assets.IGC2025.Scripts.View;
using R3;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.IGC2025.Scripts.Presenter
{
    public class PresenterResultCanvas : MonoBehaviour
    {
        // ----- SerializedField
        [Header("Models")]

        [Header("Views")]
        [SerializeField] private ViewResultCanvas _view;
        [SerializeField] private GameEndController _gameEndController;
        [SerializeField] private Button[] _endGameButton;
        [SerializeField] private TextMeshProUGUI _finishTimeTextUGUI;

        [SerializeField] private Canvas _canvas;

        // ----- UnityMessage
        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CurrentGameState
                    .Where(x => x == GameState.GAMEOVER || x == GameState.GAMECLEAR)
                    .Subscribe(x =>
                    {
                        _canvas.enabled = true;
                        _gameEndController.PlayGameEndSequence(x).Forget();
                        _finishTimeTextUGUI.text = $"{GameManager.Instance.GetComponent<TimeManager>().CurrentTimeSecond.CurrentValue.ToString("F1")}カウント";
                    })
                    .AddTo(this);
            }

            if (GameManager.Instance.SceneLoader != null)
            {
                if (_endGameButton.Length == 0) return;
                for (int i = 0; i < _endGameButton.Length; i++)
                {
                    _endGameButton[i].onClick.AddListener(() => GameManager.Instance.SceneLoader.ReloadScene());

                }
            }
        }

        // -----Private



        #region ModelToView



        #endregion


        #region ViewToModel



        #endregion

    }
}