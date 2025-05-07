using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MVRP.Sample.Views
{
    public sealed class Sample_View : MonoBehaviour
    {
        /// <summary>
        /// uGUIのSliderをアニメーションさせるコンポーネント（View）
        /// </summary>
        [SerializeField] private Slider _slider;

        [SerializeField] private TextMeshProUGUI _text;
        public int MaxHealth = 200;

        public void SetValue(float value)
        {
            // アニメーションしながらSliderを動かす
            DOTween.To(() => _slider.value,
                n => _slider.value = n,
                value,
                duration: 1.0f);
            _text.text = ($"{value} / {MaxHealth}");
        }
    }
}
