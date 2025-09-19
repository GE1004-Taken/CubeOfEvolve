using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using R3.Triggers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Collections.AllocatorManager;

namespace Assets.IGC2025.Scripts.View
{
    /// <summary>
    /// モジュール選択画面の View。
    /// - 表示候補の生成、初期見た目の設定
    /// - 画面の開閉アニメーション
    /// - クリック/ホバーの通知イベント
    /// Presenter からは Display → Prepare → PlayOpen の順で呼ばれる想定。
    /// </summary>
    public sealed class ViewDropCanvas : MonoBehaviour
    {
        // -----SerializeField
        [Header("参照")]
        [SerializeField] private GameObject _moduleItemPrefab;
        [SerializeField] private Transform _contentParent;

        [Header("表示設定")]
        [SerializeField, Tooltip("表示する選択肢の上限")] private int _maxOptions = 3;

        [Header("アニメ/キャンバス & カード")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField, Range(0.05f, 1f)] private float _openDuration = 0.25f;
        [SerializeField, Range(0.00f, 0.30f)] private float _stagger = 0.06f;
        [SerializeField, Range(0.05f, 0.60f)] private float _closeDuration = 0.18f;
        [SerializeField, Tooltip("カード初期拡大率")] private Vector3 _spawnScale = new(0.9f, 0.9f, 0.9f);

        [Header("アニメ/背景")]
        [SerializeField, Tooltip("Yスケールを 0→Overshoot→1 にする背景")] private RectTransform _background;
        [SerializeField, Tooltip("背景 0→Overshoot の時間")] private float _bgOpenDuration = 0.25f;
        [SerializeField, Tooltip("背景 Overshoot→1 の時間(0でスキップ)")] private float _bgSettleDuration = 0.12f;
        [SerializeField, Tooltip("背景のオーバーシュート量(Y)")] private float _bgOvershootY = 1.5f;

        [Header("アニメ/バナー")]
        [SerializeField, Tooltip("左からスライドインする帯")] private RectTransform _banner;
        [SerializeField, Tooltip("画面外へ出す余白(px)")] private float _bannerExtraMargin = 640f;

        // -----Events
        public Subject<int> OnModuleSelected { get; private set; } = new();
        public Subject<int> OnModuleHovered { get; private set; } = new();

        // -----Runtime
        private readonly List<GameObject> _instantiatedItems = new();
        private readonly Dictionary<int, Button> _selectionButtons = new();
        private readonly List<int> _currentDisplayedModuleIds = new();
        private readonly CompositeDisposable _disposables = new();

        private static readonly ReactiveProperty<bool> isDropChoseEnabled = new(true);
        public ReadOnlyReactiveProperty<bool> DropChoseEnabled => isDropChoseEnabled;

        // -----Field
        // Tween handles
        private Sequence _openSeq;
        private Tween _closeTween;

        // Banner cache
        private Vector2 _bannerInitialPos;
        private float _bannerOffscreenX;
        private bool _bannerCached;

        // -----UnityMessage
        private void OnDestroy()
        {
            OnModuleSelected?.Dispose();
            OnModuleHovered?.Dispose();
            _disposables.Dispose();
            _openSeq?.Kill();
            _closeTween?.Kill();
        }

        // -----publicMethod

        public void ToggleDropChoseEnabled()
        {
            isDropChoseEnabled.Value = !isDropChoseEnabled.Value;
        }

        /// <summary>指定ID/ランダム補完でカードを生成して並べる。</summary>
        public void DisplayModulesByIdOrRandom(
    List<int> moduleIds,
    List<RuntimeModuleData> candidatePool,
    ModuleDataStore dataStore)
        {
            ClearItems();

            // 自動
            if (!isDropChoseEnabled.Value)
            {
                var randomPool = candidatePool.ToList();
                ShuffleInPlace(randomPool);

                int randomIndex = 0;
                int count = Mathf.Min(_maxOptions, moduleIds.Count);

                for (int i = 0; i < count; i++)
                {
                    int reqId = moduleIds[i];
                    if (TryResolveModule(reqId, randomPool, ref randomIndex, candidatePool, dataStore,
                                         out int id, out ModuleData master, out RuntimeModuleData runtime))
                    {
                        OnModuleSelected.OnNext(id);
                        return;
                    }
                }

                Debug.LogWarning($"{nameof(ViewDropCanvas)}: 自動選択できる候補が見つかりません。");
                return;
            }

            var randomPoolNormal = candidatePool.ToList();
            ShuffleInPlace(randomPoolNormal);

            int randomIndexNormal = 0;
            int countNormal = Mathf.Min(_maxOptions, moduleIds.Count);

            for (int i = 0; i < countNormal; i++)
            {
                int reqId = moduleIds[i];
                if (TryResolveModule(reqId, randomPoolNormal, ref randomIndexNormal, candidatePool, dataStore,
                                     out int id, out ModuleData master, out RuntimeModuleData runtime))
                {
                    CreateItem(id, master, runtime);
                }
            }
        }


        /// <summary>開アニメ前の状態へ初期化（Canvas/Banner/Card）。</summary>
        public void PrepareInitialStatesForOpen()
        {
            if (!isDropChoseEnabled.Value) return;
            KillTweens();
            InitCanvasClosed();

            if (_banner && !_bannerCached)
            {
                _bannerInitialPos = _banner.anchoredPosition;
                _bannerCached = true;
            }
            _bannerOffscreenX = _banner ? CalcBannerOffscreenX(_banner, _bannerExtraMargin) : 0f;

            InitBackgroundAndBannerClosed();

            foreach (var go in _instantiatedItems)
            {
                var rt = go.transform as RectTransform;
                if (rt) rt.localScale = _spawnScale;
                EnsureCanvasGroup(go).alpha = 0f;
            }
        }

        /// <summary>画面を開く（Canvas → 背景/バナー → カード）。</summary>
        public async UniTask PlayOpenAsync(CancellationToken ct = default)
        {
            if (!isDropChoseEnabled.Value) return; // 追加：自動モード時は開かない

            KillTweens();
            if (!_canvasGroup) return;
            InitCanvasClosed();

            _openSeq = BuildOpenSequence();
            await _openSeq.AsyncWaitForCompletion();

            if (ct.IsCancellationRequested) return;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        /// <summary>画面を閉じる（Canvas フェード + バナー退場）。</summary>
        public async UniTask PlayCloseAsync()
        {
            if (!isDropChoseEnabled.Value) return; // 追加：自動モード時は閉じも不要

            _openSeq?.Kill();
            if (!_canvasGroup) return;

            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            _closeTween?.Kill();
            _closeTween = BuildCloseSequence();

            await _closeTween.AsyncWaitForCompletion();
        }

        // -----PrivateMethod
        // Build Timeline

        /// <summary>開アニメの全体タイムラインを構築。</summary>
        private Sequence BuildOpenSequence()
        {
            var seq = DOTween.Sequence();

            // 1) Canvas
            seq.Append(_canvasGroup.DOFade(1f, _openDuration).SetEase(Ease.OutSine));

            // 2) 背景&バナー（同時ブロック）
            var block = DOTween.Sequence();
            if (_background)
            {
                block.Join(_background.DOScaleY(_bgOvershootY, _bgOpenDuration).SetEase(Ease.OutBack));
                //if (_bgSettleDuration > 0f)
                //    block.Append(_background.DOScaleY(1.5f, _bgSettleDuration).SetEase(Ease.OutCubic));
            }
            if (_banner)
            {
                block.Join(_banner.DOAnchorPosX(_bannerInitialPos.x, _bgOpenDuration)
                                  .SetEase(Ease.OutCubic)
                                  .SetLink(_banner.gameObject));
            }
            seq.Append(block);

            // 3) カード（等間隔で順に）
            seq.Append(BuildCardsSequence());

            return seq;
        }

        /// <summary>閉アニメのタイムラインを構築。</summary>
        private Sequence BuildCloseSequence()
        {
            var close = DOTween.Sequence();
            close.Join(_canvasGroup.DOFade(0f, _closeDuration).SetEase(Ease.InSine));

            if (_background)
            {
                close.Join(_background.DOScaleY(0f, _closeDuration).SetEase(Ease.OutBack));
                //if (_bgSettleDuration > 0f)
                //    block.Append(_background.DOScaleY(1.5f, _bgSettleDuration).SetEase(Ease.OutCubic));
            }

            if (_banner)
            {
                close.Join(_banner.DOAnchorPosX(_bannerOffscreenX, _closeDuration)
                                  .SetEase(Ease.InCubic)
                                  .SetLink(_banner.gameObject));
            }
            return close;
        }

        /// <summary>カード入場（1枚=1サブSequenceを積む方式で等間隔を保証）。</summary>
        private Sequence BuildCardsSequence()
        {
            var cards = DOTween.Sequence();

            for (int i = 0; i < _instantiatedItems.Count; i++)
            {
                var go = _instantiatedItems[i];
                var rt = go.transform as RectTransform;
                if (!rt) continue;

                var cg = EnsureCanvasGroup(go);

                var one = DOTween.Sequence(); // 1枚分
                one.Join(rt.DOScale(1f, _openDuration).SetEase(Ease.OutBack).SetLink(go));
                one.Join(cg.DOFade(1f, _openDuration).SetLink(go));

                cards.Append(one);
                if (i < _instantiatedItems.Count - 1) cards.AppendInterval(_stagger);
            }

            return cards;
        }

        // -----PrivateMethod
        // Items

        /// <summary>候補解決（指定ID or ランダム補完）。</summary>
        private static bool TryResolveModule(
            int requestId,
            IList<RuntimeModuleData> randomPool,
            ref int randomIndex,
            List<RuntimeModuleData> candidatePool,
            ModuleDataStore dataStore,
            out int moduleId,
            out ModuleData master,
            out RuntimeModuleData runtime)
        {
            moduleId = -1; master = null; runtime = null;

            if (requestId == -1)
            {
                // 既出IDを避けつつランダム補完
                while (randomIndex < randomPool.Count)
                {
                    var r = randomPool[randomIndex++];
                    runtime = r;
                    master = dataStore.FindWithId(r.Id);
                    moduleId = r.Id;
                    if (master != null) return true;
                }
                Debug.LogWarning($"{nameof(ViewDropCanvas)}: ランダム候補が不足しています。");
                return false;
            }

            runtime = candidatePool.FirstOrDefault(c => c.Id == requestId);
            master = dataStore.FindWithId(requestId);
            moduleId = requestId;

            if (runtime == null || master == null)
            {
                Debug.LogWarning($"{nameof(ViewDropCanvas)}: 指定ID {requestId} のデータが見つかりません。スキップ。");
                return false;
            }

            return true;
        }

        /// <summary>カード1枚を生成し、見た目とイベントをセット。</summary>
        private void CreateItem(int moduleId, ModuleData master, RuntimeModuleData runtime)
        {
            var item = Instantiate(_moduleItemPrefab, _contentParent);
            _instantiatedItems.Add(item);

            var view = item.GetComponent<ViewInfo>();
            var button = item.GetComponentInChildren<Button>();

            if (!view || !button)
            {
                Debug.LogError($"{nameof(ViewDropCanvas)}: Prefabに ViewInfo / Button がありません。ID:{moduleId}");
                return;
            }

            view.SetInfo(master, runtime);
            _selectionButtons[moduleId] = button;

            var indicator = item.transform.Find("LevelZeroIndicator");
            if (indicator) indicator.gameObject.SetActive(runtime.CurrentLevelValue == 0);

            InitItemVisual(item);
            BindItemEvents(moduleId, button);

            _currentDisplayedModuleIds.Add(moduleId);
        }

        /// <summary>アイテムの初期見た目（アニメ前）。</summary>
        private void InitItemVisual(GameObject item)
        {
            if (item.transform is RectTransform rt) rt.localScale = _spawnScale;
            EnsureCanvasGroup(item).alpha = 0f;
        }

        /// <summary>クリック/ホバーのイベント購読。</summary>
        private void BindItemEvents(int moduleId, Button button)
        {
            button.OnClickAsObservable()
                  .Subscribe(_ => OnModuleSelected.OnNext(moduleId))
                  .AddTo(_disposables);

            button.OnPointerEnterAsObservable()
                  .Subscribe(_ => OnModuleHovered.OnNext(moduleId))
                  .AddTo(_disposables);
        }

        // Utilities

        private void InitBackgroundAndBannerClosed()
        {
            if (_background) _background.localScale = new Vector3(1.5f, 0f, 1.5f);
            if (_banner) _banner.anchoredPosition = new Vector2(_bannerOffscreenX, _bannerInitialPos.y);
        }

        private void ClearItems()
        {
            foreach (var obj in _instantiatedItems) Destroy(obj);
            _instantiatedItems.Clear();
            _selectionButtons.Clear();
            _currentDisplayedModuleIds.Clear();
            _disposables.Clear();
        }

        private void InitCanvasClosed()
        {
            if (!_canvasGroup) return;
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        private void KillTweens()
        {
            _openSeq?.Kill(); _openSeq = null;
            _closeTween?.Kill(); _closeTween = null;
        }

        private static void ShuffleInPlace<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        private float CalcBannerOffscreenX(RectTransform banner, float extraMargin)
        {
            var parentRT = banner.parent as RectTransform;
            float parentW = parentRT ? parentRT.rect.width : Screen.width;
            float bannerW = banner.rect.width;
            return _bannerInitialPos.x - (parentW + bannerW) * 0.5f - extraMargin;
        }

        private static CanvasGroup EnsureCanvasGroup(GameObject go)
            => go.TryGetComponent(out CanvasGroup cg) ? cg : go.AddComponent<CanvasGroup>();
    }
}
