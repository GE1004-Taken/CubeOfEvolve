using Assets.AT;
using Assets.IGC2025.Scripts.Event;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.IGC2025.Scripts.Presenter
{
    /// <summary>
    /// Playerの体力をViewに反映するPresenter
    /// </summary>
    public sealed class PresenterGameCanvas : MonoBehaviour
    {
        [Header("Models")]
        [SerializeField] private PlayerCore _models;

        [Header("Views")]
        [SerializeField] private TextMeshProUGUI _timeText;
        [SerializeField] private SliderAnimation _hpSliderAnimation;
        [SerializeField] private SliderAnimation _expSliderAnimation;
        [SerializeField] private EventLevelUp _levelUp;
        [SerializeField] private TextScaleAnimation _cubeCountTextScaleAnimation;
        [SerializeField] private TextScaleAnimation _maxCubeCountTextScaleAnimation;
        [SerializeField] private TextScaleAnimation _moneyTextScaleAnimation;

        private TimeManager _timeManager;

        private const float BOSS_CREATE_TIME = 60;

        private void Start()
        {
            _timeManager = GameManager.Instance.GetComponent<TimeManager>();
            _timeManager.CurrentTimeSecond
                .Subscribe(x =>
                {
                    var time = (BOSS_CREATE_TIME - x);
                    if (time >= 0) _timeText.text = $"ボス出現まで残り……\r\n{time.ToString("F1")}\r\nカウント！";
                    else _timeText.text = $"ボス出現中！";
                })
                .AddTo(this);

            // PlayerのHealthを監視
            _models.Hp
                .Subscribe(x =>
                {
                    // Viewに反映
                    _hpSliderAnimation.SliderAnime(x);
                }).AddTo(this);

            // Playerの経験値を監視
            _models.Exp
                .Subscribe(x =>
                {
                    // Viewに反映
                    _expSliderAnimation.SliderAnime((float)x);
                }).AddTo(this);

            // Playerの経験値を監視
            _models.Level
                .Subscribe(_ =>
                {
                    // Event実行
                    _levelUp.PlayLevelUpEvent();
                    _hpSliderAnimation.GetComponent<Slider>().maxValue = _models.MaxHp.CurrentValue;
                }).AddTo(this);

            // Playerの所持キューブ数を監視
            _models.CubeCount
                .Subscribe(x =>
                {
                    // Viewに反映
                    _cubeCountTextScaleAnimation.AnimateFloatAndText(x, 1f);
                }).AddTo(this);

            // Playerの所持キューブ数を監視
            _models.MaxCubeCount
                .Subscribe(x =>
                {
                    // Viewに反映
                    _maxCubeCountTextScaleAnimation.AnimateFloatAndText(x, 1f);
                }).AddTo(this);

            // Playerの所持金を監視
            _models.Money
                .Subscribe(x =>
                {
                    // Viewに反映
                    _moneyTextScaleAnimation.AnimateFloatAndText(x, 1f);
                }).AddTo(this);
        }
    }
}
