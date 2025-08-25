using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.AT
{
    public class SliderAnimation : MonoBehaviour
    {
        private Slider _slider;

        private void Start()
        {
            _slider = GetComponent<Slider>();
        }

        public void SliderAnime(float value)
        {
            // アニメーションしながらSliderを動かす
            DOTween.To(() => _slider.value,
                n => _slider.value = n,
                value,
                duration: 1.0f)
                .SetUpdate(true);
        }
    }
}
