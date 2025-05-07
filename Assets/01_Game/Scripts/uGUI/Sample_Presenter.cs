using R3;
using MVRP.Sample.Models;
using MVRP.Sample.Views;
using UnityEngine;

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

        private void Start()
        {
            _views.MaxHealth = _models.MaxHealth;


            // Player‚ÌHealth‚ðŠÄŽ‹
            _models.Health
                .Subscribe(x =>
                {
                    // View‚É”½‰f
                    _views.SetValue(x);
                    Debug.Log(x);
                }).AddTo(this);
        }
    }
}
