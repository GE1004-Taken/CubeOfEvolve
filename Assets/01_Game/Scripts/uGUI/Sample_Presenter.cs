using R3;
using MVRP.Sample.Models;
using MVRP.Sample.Views;
using UnityEngine;
using TMPro;

namespace MVRP.Sample.Presenters
{
    /// <summary>
    /// Player‚Ì‘Ì—Í‚ðView‚É”½‰f‚·‚éPresenter
    /// </summary>
    public sealed class Sample_Presenter : MonoBehaviour
    {
        // Model
        [SerializeField] private Sample_Model _models;

        // View
        [SerializeField] private Sample_View _views;
        [SerializeField] private TextMeshProUGUI _text;

        private void Start()
        {
            // Player‚ÌHealth‚ðŠÄŽ‹
            _models.Health
                .Subscribe(x =>
                {
                    // View‚É”½‰f
                    _views.SetValue(x);
                    _text.text = $"{x} / {_models.MaxHealth}";
                    Debug.Log(x);
                }).AddTo(this);
        }
    }
}
