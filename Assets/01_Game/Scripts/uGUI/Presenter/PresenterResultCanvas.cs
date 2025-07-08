using Assets.IGC2025.Scripts.GameManagers;
using Assets.IGC2025.Scripts.View;
using R3;
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
        [SerializeField] private Button[] _endGameButton;
        [SerializeField] private TextMeshProUGUI _finishTimeTextUGUI;

        // ----- UnityMessage
        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CurrentGameState
                    .Where(x => x == GameState.GAMEOVER || x == GameState.GAMECLEAR)
                    .Subscribe(x =>
                    {
                        _view.ShowCanvas(x);
                        _finishTimeTextUGUI.text = $"{GameManager.Instance.GetComponent<TimeManager>().CurrentTimeSecond.CurrentValue.ToString("F1")}ƒJƒEƒ“ƒg";
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