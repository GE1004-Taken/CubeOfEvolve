using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;

public class BubbleToggleAnimator : MonoBehaviour
{
    [Header(" 吹き出しUI (RectTransform)")]
    [SerializeField] private RectTransform bubbleRect;

    [Header(" 切り替えボタン (Button)")]
    [SerializeField] private Button toggleButton;

    [Header(" 表示位置（B）")]
    [SerializeField] private Vector2 showPosition;

    [Header(" 非表示位置（A）")]
    [SerializeField] private Vector2 hidePosition;

    [Header(" アニメーション時間（秒）")]
    [SerializeField] private float moveDuration = 0.3f;

    [Header(" ボタンの見た目反転 (Z軸180度)")]
    [SerializeField] private RectTransform buttonIconRect; // ここが回転する部分

    private bool isShown = false;
    private CancellationTokenSource cancelTokenSource;

    private void Awake()
    {
        if (bubbleRect == null) Debug.LogError(" bubbleRect が未設定です");
        if (toggleButton == null) Debug.LogError(" toggleButton が未設定です");
        if (buttonIconRect == null) Debug.LogWarning(" buttonIconRect が未設定です（Z回転は無効になります）");
    }

    private void Start()
    {
        if (bubbleRect == null || toggleButton == null) return;

        // 最初に非表示位置に配置
        bubbleRect.anchoredPosition = hidePosition;
        isShown = false;
        UpdateButtonVisual();

        toggleButton.onClick.AddListener(() => ToggleAsync().Forget());
    }

    private async UniTaskVoid ToggleAsync()
    {
        cancelTokenSource?.Cancel();
        cancelTokenSource = new CancellationTokenSource();

        isShown = !isShown;
        Vector2 targetPos = isShown ? showPosition : hidePosition;

        Debug.Log($"[Moving to] {targetPos} (TimeScale={Time.timeScale})");

        await bubbleRect.DOAnchorPos(targetPos, moveDuration)
            .SetEase(Ease.InOutQuad)
            .SetUpdate(true) // ← これが重要！
            .WithCancellation(cancelTokenSource.Token);

        UpdateButtonVisual();
    }





    private void UpdateButtonVisual()
    {
        if (buttonIconRect != null)
        {
            float z = isShown ? 180f : 0f;
            buttonIconRect.localRotation = Quaternion.Euler(0f, 0f, z);
        }
    }

    private void OnDestroy()
    {
        cancelTokenSource?.Cancel();
    }
}
