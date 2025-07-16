using Assets.AT;
using AT.uGUI;
using Cysharp.Threading.Tasks;
using R3;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class GuideManager : MonoBehaviour
{
    public static GuideManager Instance { get; private set; }

    // public static bool GuideEnabled { get; private set; } = true;

    private static readonly ReactiveProperty<bool> isGuideEnabled = new(true);
    public ReadOnlyReactiveProperty<bool> GuideEnabled => isGuideEnabled;

    private static HashSet<string> shownGuides = new();
    private CanvasCtrlManager canvasManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    private void Start()
    {
        canvasManager = CanvasCtrlManager.Instance;
    }

    public void ToggleGuideEnabled()
    {
        isGuideEnabled.Value = !isGuideEnabled.Value;
    }

    public void TryShowGuide(string guideKey)
    {
        if (!isGuideEnabled.Value || HasShown(guideKey)) return;

        var guide = canvasManager.GetCanvas(guideKey);
        if (guide != null)
        {
            guide.OnOpenCanvas();
            shownGuides.Add(guideKey);
        }
    }

    public async UniTask ShowGuideAndWaitAsync(string guideKey, CancellationToken token)
    {
        if (!isGuideEnabled.Value || HasShown(guideKey)) return;

        var guide = canvasManager.GetCanvas(guideKey);
        if (guide != null)
        {
            guide.OnOpenCanvas();
            shownGuides.Add(guideKey);

            await UniTask.WaitUntil(() => !guide.GetComponent<Canvas>().enabled); // ”ñ”Ä—p
        }
    }

    public async UniTask DoBuildModeAndWaitAsync(CancellationToken token)
    {
        if (!isGuideEnabled.Value) return;

        CanvasCtrlManager canvasCtrlManager = CanvasCtrlManager.Instance;

        CameraCtrlManager.Instance.ChangeCamera("Build Camera");
        canvasCtrlManager.ShowOnlyCanvas("BuildView");

        await UniTask.WaitUntil(() => !canvasCtrlManager.GetCanvas("BuildView").GetComponent<Canvas>().enabled && !canvasCtrlManager.GetCanvas("ShopView").GetComponent<Canvas>().enabled); // ”ñ”Ä—p
    }

    public void ShowGuideAlways(string guideKey)
    {
        var guide = canvasManager.GetCanvas(guideKey);
        if (guide != null)
        {
            guide.OnOpenCanvas();
        }
    }

    public bool HasShown(string guideKey) => shownGuides.Contains(guideKey);
}
