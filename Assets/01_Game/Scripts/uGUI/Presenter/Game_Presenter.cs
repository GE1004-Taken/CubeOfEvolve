using AT.EVENT;
using MVRP.AT.View;
using R3;
using UnityEngine;

namespace MVRP.AT.Presenter
{
    /// <summary>
    /// Player‚Ì‘Ì—Í‚ðView‚É”½‰f‚·‚éPresenter
    /// </summary>
    public sealed class Game_Presenter : MonoBehaviour
    {
        // Model
        [SerializeField] private PlayerCore _models;

        // View
        [SerializeField] private SliderAnimation _hpSliderAnimation;
        [SerializeField] private SliderAnimation _expSliderAnimation;
        [SerializeField]private  Event_Levelup _levelup;
        [SerializeField] private TextScaleAnimation _cubeCountTextScaleAnimation;
        [SerializeField] private TextScaleAnimation _maxCubeCountTextScaleAnimation;
        [SerializeField] private TextScaleAnimation _moneyTextScaleAnimation;


        private void Start()
        {
            // Player‚ÌHealth‚ðŠÄŽ‹
            _models.Hp
                .Subscribe(x =>
                {
                    // View‚É”½‰f
                    _hpSliderAnimation.SliderAnime(x);
                }).AddTo(this);

            // Player‚ÌŒoŒ±’l‚ðŠÄŽ‹
            _models.Exp
                .Subscribe(x =>
                {
                    // View‚É”½‰f
                    _expSliderAnimation.SliderAnime((float)x);
                }).AddTo(this);

            // Player‚ÌŒoŒ±’l‚ðŠÄŽ‹
            _models.Level
                .Subscribe(_ =>
                {
                    // EventŽÀs
                    _levelup.event_Levelup();
                }).AddTo(this);

            // Player‚ÌŠŽƒLƒ…[ƒu”‚ðŠÄŽ‹
            _models.CubeCount
                .Subscribe(x =>
                {
                    // View‚É”½‰f
                    _cubeCountTextScaleAnimation.AnimateFloatAndText(x,1f);
                }).AddTo(this);

            // Player‚ÌŠŽƒLƒ…[ƒu”‚ðŠÄŽ‹
            _models.MaxCubeCount
                .Subscribe(x =>
                {
                    // View‚É”½‰f
                    _maxCubeCountTextScaleAnimation.AnimateFloatAndText(x, 1f);
                }).AddTo(this);

            // Player‚ÌŠŽ‹à‚ðŠÄŽ‹
            _models.Money
                .Subscribe(x =>
                {
                    // View‚É”½‰f
                    _moneyTextScaleAnimation.AnimateFloatAndText(x, 1f);
                }).AddTo(this);
        }
    }
}
