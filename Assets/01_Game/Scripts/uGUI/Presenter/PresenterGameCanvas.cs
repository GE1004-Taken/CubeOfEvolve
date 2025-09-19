using Assets.AT;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.IGC2025.Scripts.Presenter
{
    /// <summary>
    /// Playerの体力をViewに反映するPresenter
    /// </summary>
    public sealed class PresenterGameCanvas : MonoBehaviour, IPresenter
    {
        // -----SerializeField
        [Header("Models")]
        [SerializeField] private PlayerCore _models;

        [Header("Views")]
        [SerializeField] private TextMeshProUGUI _timeText;
        [SerializeField] private SliderAnimation _hpSliderAnimation;
        [SerializeField] private SliderAnimation _expSliderAnimation;
        [SerializeField] private TextScaleAnimation _cubeCountTextScaleAnimation;
        [SerializeField] private TextScaleAnimation _maxCubeCountTextScaleAnimation;
        [SerializeField] private TextScaleAnimation _moneyTextScaleAnimation;

        // -----Field
        private Slider _hpSlider;
        private TimeManager _timeManager;
        private const float BOSS_CREATE_TIME = 300;

        public bool IsInitialized { get; private set; } = false;

        // -----UnityMessage

        // -----PublicMethod
        public void Initialize()
        {
            if (IsInitialized) return;

            _hpSlider = _hpSliderAnimation.GetComponent<Slider>();
            _timeManager = GameManager.Instance.GetComponent<TimeManager>();

            _timeManager.CurrentTimeSecond
                .Subscribe(x =>
                {
                    var time = (BOSS_CREATE_TIME - x);
                    if (time >= 0) _timeText.text = $"ボス出現まで残り……\r\n{time.ToString("F1")}\r\nカウント！";
                    else _timeText.text = $"ボス出現中！";
                })
                .AddTo(this);

            _models.MaxHp
                .Subscribe(max => { _hpSlider.maxValue = max; })
                .AddTo(this);

            _models.Hp
                .Subscribe(x => _hpSliderAnimation.SliderAnime(x))
                .AddTo(this);

            _models.RequireExp
                .Subscribe(x => _expSliderAnimation.GetComponent<Slider>().maxValue = x)
                .AddTo(this);

            _models.Exp
                .Subscribe(x => _expSliderAnimation.SliderAnime((float)x))
                .AddTo(this);

            _models.CubeCount
                .Subscribe(x => _cubeCountTextScaleAnimation.AnimateFloatAndText(x, 1f))
                .AddTo(this);

            _models.MaxCubeCount
                .Subscribe(x => _maxCubeCountTextScaleAnimation.AnimateFloatAndText(x, 1f))
                .AddTo(this);

            _models.Money
                .Subscribe(x => _moneyTextScaleAnimation.AnimateFloatAndText(x, 1f))
                .AddTo(this);

            IsInitialized = true;
        }
    }
}
