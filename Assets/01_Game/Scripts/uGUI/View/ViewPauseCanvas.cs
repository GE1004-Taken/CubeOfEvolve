using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public sealed class ViewPauseCanvas : MonoBehaviour
{

    // -----SerializeField
    [Header("参照")]
    [SerializeField] RectTransform _window;
    [SerializeField] GameObject _panel;
    [SerializeField] CanvasGroup _canvasGroup;

    [Header("アニメ")]
    [SerializeField, Range(0.1f, 1f)] float _slideDuration = 0.3f;
    [SerializeField] Ease _openEase = Ease.OutCubic;
    [SerializeField] Ease _closeEase = Ease.InCubic;

    // -----Field
    Vector2 _initialPos;
    Vector2 _hiddenPos;
    bool _cachedInitial;
    Tween _tween;
    bool _isPlayingAnim = false;

    // -----UnityMessage
    void Awake()
    {
        if (!_window) Debug.LogError("Window未設定", this);
        if (!_canvasGroup) Debug.LogError("CanvasGroup未設定", this);
    }

    // -----PublicMethod
    // 画面サイズが変わった時は、隠し位置だけ再計算する
    void OnRectTransformDimensionsChange()
    {
        if (_cachedInitial) RecalcHiddenPos(); // initialは固定、hiddenだけ更新
    }

    /// <summary>初期化：一度だけ初期位置をキャッシュ→毎回「隠し位置」だけ更新</summary>
    public void PrepareInitialStatesForOpen()
    {
        KillTween();
        CacheInitialPosIfNeeded();
        RecalcHiddenPos();

        // 状態を「隠れている場所」にセット
        _window.anchoredPosition = _hiddenPos;

        // 背景は非表示、入力は無効
        if (_panel) _panel.SetActive(false);
        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    public async UniTask PlayOpenAsync()
    {
        if (_isPlayingAnim) return;
        _isPlayingAnim = true;

        KillTween();
        if (_panel) _panel.SetActive(true);

        _tween = _window.DOAnchorPos(_initialPos, _slideDuration).SetEase(_openEase);
        await _tween.AsyncWaitForCompletion();

        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
        _isPlayingAnim = false;
    }

    public async UniTask PlayCloseAsync()
    {
        KillTween();

        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        // 初期位置から左へ戻す
        // （hiddenPosは毎回幅から計算されているのでズレない）
        _tween = _window.DOAnchorPos(_hiddenPos, _slideDuration).SetEase(_closeEase);
        await _tween.AsyncWaitForCompletion();

        if (_panel) _panel.SetActive(false);
    }

    // -----PrivateMethod
    private void CacheInitialPosIfNeeded()
    {
        if (_cachedInitial) return;

        _initialPos = _window.anchoredPosition;
        _cachedInitial = true;
    }

    private void RecalcHiddenPos()
    {
        // Width は RectTransform の現在幅を使用（アンカー/解像度が変わっても反映）
        float width = _window.rect.width;
        _hiddenPos = _initialPos + Vector2.left * width;
    }

    private void KillTween()
    {
        _tween?.Kill();
        _tween = null;
    }
}
