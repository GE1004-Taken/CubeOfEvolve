using Assets.IGC2025.Scripts.Event;
using Assets.AT;
using R3;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using TMPro;
using UnityEngine.UI;

namespace Assets.IGC2025.Scripts.Presenter
{
    /// <summary>
    /// PlayerÇÃëÃóÕÇViewÇ…îΩâfÇ∑ÇÈPresenter
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

        private void Start()
        {
            _timeManager = GameManager.Instance.GetComponent<TimeManager>();
            _timeManager.CurrentTimeSecond
                .Subscribe(x =>
                {
                    _timeText.text = $"{(x).ToString("F1")}";
                })
                .AddTo(this);

            // PlayerÇÃHealthÇäƒéã
            _models.Hp
                .Subscribe(x =>
                {
                    // ViewÇ…îΩâf
                    _hpSliderAnimation.SliderAnime(x);
                }).AddTo(this);

            // PlayerÇÃåoå±ílÇäƒéã
            _models.Exp
                .Subscribe(x =>
                {
                    // ViewÇ…îΩâf
                    _expSliderAnimation.SliderAnime((float)x);
                }).AddTo(this);

            // PlayerÇÃåoå±ílÇäƒéã
            _models.Level
                .Subscribe(_ =>
                {
                    // Eventé¿çs
                    _levelUp.PlayLevelUpEvent();
                    _hpSliderAnimation.GetComponent<Slider>().maxValue = _models.MaxHp.CurrentValue;
                }).AddTo(this);

            // PlayerÇÃèäéùÉLÉÖÅ[ÉuêîÇäƒéã
            _models.CubeCount
                .Subscribe(x =>
                {
                    // ViewÇ…îΩâf
                    _cubeCountTextScaleAnimation.AnimateFloatAndText(x, 1f);
                }).AddTo(this);

            // PlayerÇÃèäéùÉLÉÖÅ[ÉuêîÇäƒéã
            _models.MaxCubeCount
                .Subscribe(x =>
                {
                    // ViewÇ…îΩâf
                    _maxCubeCountTextScaleAnimation.AnimateFloatAndText(x, 1f);
                }).AddTo(this);

            // PlayerÇÃèäéùã‡Çäƒéã
            _models.Money
                .Subscribe(x =>
                {
                    // ViewÇ…îΩâf
                    _moneyTextScaleAnimation.AnimateFloatAndText(x, 1f);
                }).AddTo(this);
        }
    }
}
