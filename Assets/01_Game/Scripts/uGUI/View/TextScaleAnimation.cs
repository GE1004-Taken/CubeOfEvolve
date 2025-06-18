using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using R3;

namespace Assets.AT
{
    public class TextScaleAnimation : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI _text;

        private float _currentValue = 0f;
        private Vector3 _initScale;

        private void Start()
        {
            if (_text != null)
            {
                _initScale = _text.GetComponent<RectTransform>().localScale;
            }
            else
            {
                Debug.LogError("_text が nullでな！");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Value"></param>
        /// <param name="totalDuration"></param>
        public void AnimateFloatAndText(float Value, float totalDuration)
        {
            var sizeDuration = totalDuration * 0.5f;

            // 1. float型の変数の値をアニメーション
            DOTween.To(() => _currentValue,
                n => _currentValue = n,
                Value,
                duration: totalDuration
                ).OnUpdate(TextUpdate); ;

            // 2. テキストサイズを総アニメーション時間の半分秒で2倍に拡大
            Sequence sequence = DOTween.Sequence();
            sequence.Append(_text.rectTransform.DOScale(_initScale * 2, sizeDuration));

            // 3. 2の完了時、総アニメーション時間の半分秒で元のサイズに戻る
            sequence.Append(_text.rectTransform.DOScale(_initScale, sizeDuration));
        }

        private void TextUpdate()
        {
            _text.text = $"{_currentValue:F0}";
        }
    }
}
