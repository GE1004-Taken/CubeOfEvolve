using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace MVRP.Sample
{
    public sealed class Sample_View : MonoBehaviour
    {
        /// <summary>
        /// uGUIのSliderをアニメーションさせるコンポーネント（View）
        /// </summary>
        [SerializeField] private Slider _slider;

        public void SetValue(float value)
        {
            // アニメーションしながらSliderを動かす
            DOTween.To(() => _slider.value,
                n => _slider.value = n,
                value,
                duration: 1.0f);
        }
    }
}
