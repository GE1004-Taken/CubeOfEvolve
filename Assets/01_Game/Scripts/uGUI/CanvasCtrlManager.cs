// 作成日：250616
// 作成者：AT
// CanvasCtrlが多くなったため管理用に作成。
// 初期化等、一括操作に使用。

using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Assets.IGC2025.Scripts.GameManagers;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AT.uGUI
{
    /// <summary>
    /// シーン上のCanvasCtrlインスタンスを管理し、一括操作や初期化を行うためのリストコンポーネント。
    /// Singletonパターンにより、どこからでもアクセス可能です。
    /// </summary>
    public class CanvasCtrlManager : MonoBehaviour
    {
        // --- Inspector Control
        [System.Serializable]
        private class CanvasEntry
        {
            public string key;
            public CanvasCtrl canvasCtrl;

            public CanvasEntry(CanvasCtrl canvasCtrl)
            {
                key = $"{canvasCtrl.name}";
                this.canvasCtrl = canvasCtrl;
            }
        }

        // --- Field
        [SerializeField]
        private List<CanvasEntry> _canvasEntries;

        // --- Singleton Pattern
        public static CanvasCtrlManager Instance { get; private set; }

        // --- UnityMessage
        private void Awake()
        {
            // すでに別のインスタンスが存在する場合、それを破棄
            if (Instance != null && Instance != this)
            {
                Debug.Log("[CanvasCtrlManager] 古いインスタンスを破棄し、最新のインスタンスに差し替えます", this);
                Destroy(Instance.gameObject);
            }

            // このインスタンスを最新として登録
            Instance = this;
            Debug.Log("[CanvasCtrlManager] 新しいインスタンスが設定されました", this);

            // Setup 実行
            if (_canvasEntries == null || _canvasEntries.Count == 0)
            {
                Setup();
            }
            else
            {
                _canvasEntries = _canvasEntries.Where(entry => entry.canvasCtrl != null).ToList();
            }

            Initialize();
        }

        private void Start()
        {
            ShowOnlyCanvas("TitleView");
            GameManager.Instance.ChangeGameState(GameState.TITLE);
        }

        // --- Private Methods

        /// <summary>
        /// シーン上のCanvasCtrlコンポーネントを検出し、管理リストに登録します。
        /// 既存のリストエントリを更新し、無効な参照を削除します。
        /// 主にエディタの「Setup CanvasCtrls」ボタンから呼び出されます。
        /// </summary>
        private void Setup()
        {
            CanvasCtrl[] sceneCanvasCtrls = FindObjectsOfType<CanvasCtrl>();

            if (_canvasEntries == null)
            {
                _canvasEntries = new List<CanvasEntry>();
            }

            // 新しいCanvasCtrlを追加
            foreach (CanvasCtrl ctrl in sceneCanvasCtrls)
            {
                // 既にリストに存在しないCanvasCtrlのみを追加
                if (_canvasEntries.FindIndex(x => x.canvasCtrl == ctrl) < 0)
                {
                    _canvasEntries.Add(new CanvasEntry(ctrl));
                }
            }

            // シーンから無くなっていたCanvasCtrlをリストから削除
            for (int i = _canvasEntries.Count - 1; i >= 0; i--)
            {
                if (_canvasEntries[i].canvasCtrl == null)
                {
                    _canvasEntries.RemoveAt(i);
                }
            }

            //debug.log($"CanvasCtrlList: Setup完了。管理対象のCanvasCtrl数: {_canvasEntries.Count}");
#if UNITY_EDITOR
            EditorUtility.SetDirty(this); // エディタ上で変更を保存
#endif
        }

        /// <summary>
        /// 依存関連の参照確認。全てのCanvasの非表示。
        /// </summary>
        private void Initialize()
        {
            if (_canvasEntries == null)
            {
                Debug.LogError("CanvasCtrlList: _canvasEntriesがnullです。Setupが実行されていませんか？", this);
                return;
            }

            // 全てのキャンバスを非表示にする
            foreach (CanvasEntry entry in _canvasEntries)
            {
                // CanvasCtrlがnullでないことを確認
                if (entry.canvasCtrl != null)
                {
                    entry.canvasCtrl.GetComponent<Canvas>().enabled = false;
                    //entry.canvasCtrl.OnCloseCanvas();
                }
            }
        }

        // --- Public Methods

        /// <summary>
        /// キーに基づいてCanvasCtrlを取得します。
        /// </summary>
        /// <param name="key">取得するCanvasCtrlのキー。</param>
        /// <returns>対応するCanvasCtrlインスタンス、または見つからない場合はnull。</returns>
        public CanvasCtrl GetCanvas(string key)
        {
            if (_canvasEntries == null)
            {
                Debug.LogError("CanvasCtrlList: _canvasEntriesがnullです。Setupが実行されていませんか？", this);
                return null;
            }

            CanvasEntry entry = _canvasEntries.Find(x => x.key == key);
            if (entry == null || entry.canvasCtrl == null)
            {
                // 見つからない、または参照がnullになっている場合
                Debug.LogWarning($"CanvasCtrlList: キー '{key}' に対応するCanvasCtrlが見つからないか、参照が切れています。", this);
                return null;
            }
            return entry.canvasCtrl;
        }

        /// <summary>
        /// 全てのCanvasCtrlを非表示にします。
        /// </summary>
        public void HideAllCanvases()
        {
            if (_canvasEntries == null) return;

            foreach (var entry in _canvasEntries)
            {
                if (entry.canvasCtrl != null)
                {
                    //entry.canvasCtrl.GetComponent<Canvas>().enabled = false;
                    entry.canvasCtrl.OnCloseCanvas();
                }
            }
        }

        /// <summary>
        /// 指定されたキーのCanvasCtrlを表示し、それ以外の全てのCanvasCtrlを非表示にします。
        /// </summary>
        /// <param name="keyToShow">表示するCanvasCtrlのキー。</param>
        public void ShowOnlyCanvas(string keyToShow)
        {
            if (_canvasEntries == null)
            {
                Debug.LogError("CanvasCtrlList: _canvasEntriesがnullです。Setupが実行されていませんか？", this);
                return;
            }

            CanvasCtrl canvasToActivate = null;

            foreach (var entry in _canvasEntries)
            {
                if (entry.canvasCtrl != null)
                {
                    if (entry.key == keyToShow)
                    {
                        canvasToActivate = entry.canvasCtrl;
                    }
                    else
                    {
                        //entry.canvasCtrl.OnCloseCanvas(); // 他のキャンバスを非表示
                        entry.canvasCtrl.GetComponent<Canvas>().enabled = false;
                    }
                }
            }

            if (canvasToActivate != null)
            {
                canvasToActivate.OnOpenCanvas(); // 目的のキャンバスを表示
                //canvasToActivate.GetComponent<Canvas>().enabled = true;
            }
            else
            {
                Debug.LogWarning($"CanvasCtrlList: キー '{keyToShow}' に対応するCanvasCtrlが見つかりませんでした。表示できませんでした。", this);
            }
        }


        // --- Editor Integration
#if UNITY_EDITOR
        [CustomEditor(typeof(CanvasCtrlManager))]
        public class CanvasCtrlListEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                CanvasCtrlManager canvasList = target as CanvasCtrlManager;
                if (GUILayout.Button("Setup CanvasCtrls (Find All Scene CanvasCtrls)"))
                {
                    canvasList.Setup();
                }

                EditorGUILayout.HelpBox(
                    "「Setup CanvasCtrls」ボタンを押すと、シーン上の全てのCanvasCtrlを自動検出してリストに追加します。\n" +
                    "キーは手動で設定してください。参照が切れたCanvasCtrlは自動的に削除されます。",
                    MessageType.Info
                );

                base.OnInspectorGUI();
            }
        }
#endif
    }
}