using Unity.Cinemachine;
using System.Collections.Generic;
using UnityEngine;
using R3; // R3 (UniRx.Async) を使用するため
using System.Linq; // LINQを使用するため


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AT.CameraSystem // 適切なNamespaceに変更
{
    /// <summary>
    /// シーン上のCinemachineCameraインスタンスを管理し、切り替えや初期化を行うためのコンポーネント。
    /// Singletonパターンにより、どこからでもアクセス可能です。
    /// </summary>
    public class CameraCtrl : MonoBehaviour
    {
        // --- 定数
        private const int BASE_PRIORITY = 10; // カメラの基本プライオリティ
        private const int ACTIVE_CAMERA_PRIORITY_OFFSET = 100; // アクティブカメラに加算するプライオリティ

        // --- Inspector Control
        [System.Serializable]
        private class CameraEntry
        {
            public string key;
            public CinemachineCamera Camera; // Unity.CinemachineではなくCinemachineCamera

            public CameraEntry(CinemachineCamera cam)
            {
                key = cam.name; // カメラのGameObject名をキーとする
                this.Camera = cam;
            }
        }

        // --- Field
        [SerializeField]
        private List<CameraEntry> _cameraEntries = new List<CameraEntry>(); // カメラエントリのリスト

        [Header("初期設定")]
        [SerializeField]
        private string _initialActiveCameraKey = ""; // 初期表示するカメラのキー

        private CinemachineBrain _cinemachineBrain;
        private float _cameraBlendTime; // CinemachineBrainから取得したブレンド時間
        private string _currentActiveCameraKey = null; // 現在アクティブなカメラのキー

        // --- Singleton Pattern
        public static CameraCtrl Instance { get; private set; }

        // カメラ切り替えの待機時間（ブレンド時間）を外部から取得するためのプロパティ
        public float CameraBlendTime => _cameraBlendTime;

        // --- UnityMessage
        private void Awake()
        {
            // Singletonのインスタンス設定
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("CameraCtrl: 既に別のインスタンスが存在します。このオブジェクトは破棄されます。", this);
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // CinemachineBrainの取得とエラーハンドリング
            _cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
            if (_cinemachineBrain == null)
            {
                Debug.LogError("メインカメラにCinemachineBrainが見つかりません。カメラ制御が正しく機能しない可能性があります。", this);
                return;
            }

            // CinemachineBrainからブレンド時間を取得
            _cameraBlendTime = _cinemachineBrain.DefaultBlend.BlendTime;

            // Optional: Inspectorで設定されていない場合、エディタで自動的にSetupを走らせる
            if (_cameraEntries == null || _cameraEntries.Count == 0)
            {
                SetupCameras();
            }
            else
            {
                // ランタイムチェック：nullになっているエントリをフィルタリング
                _cameraEntries = _cameraEntries.Where(entry => entry.Camera != null).ToList();
            }
        }

        private void Start()
        {
            InitializeCameras();
        }

        // --- Private Methods

        /// <summary>
        /// シーン上のCinemachineCameraコンポーネントを検出し、管理リストに登録します。
        /// 既存のリストエントリを更新し、無効な参照を削除します。
        /// 主にエディタの「Setup Cameras」ボタンから呼び出されます。
        /// </summary>
        private void SetupCameras()
        {
            // CinemachineCamera を取得
            // Unity.Cinemachine.CinemachineCamera は CinemachineCamera の基底クラスだが、
            // より具体的な CinemachineCamera を直接扱う方が一般的
            CinemachineCamera[] sceneVirtualCameras = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);

            if (_cameraEntries == null)
            {
                _cameraEntries = new List<CameraEntry>();
            }

            // 新しいVirtualCameraを追加
            foreach (CinemachineCamera cam in sceneVirtualCameras)
            {
                // 既にリストに存在しないVirtualCameraのみを追加
                if (_cameraEntries.FindIndex(x => x.Camera == cam) < 0)
                {
                    _cameraEntries.Add(new CameraEntry(cam));
                }
            }

            // シーンから無くなっていたVirtualCameraをリストから削除
            for (int i = _cameraEntries.Count - 1; i >= 0; i--)
            {
                if (_cameraEntries[i].Camera == null)
                {
                    _cameraEntries.RemoveAt(i);
                }
                // キーをオブジェクト名に更新する（名前変更に対応するため）
                else
                {
                    _cameraEntries[i].key = _cameraEntries[i].Camera.name;
                }
            }

            // キーによるアクセスを容易にするため、キーでソートする（任意）
            _cameraEntries = _cameraEntries.OrderBy(entry => entry.key).ToList();

            Debug.Log($"CameraCtrl: Setup完了。管理対象のCinemachineCamera数: {_cameraEntries.Count}");
#if UNITY_EDITOR
            EditorUtility.SetDirty(this); // エディタ上で変更を保存
#endif
        }

        /// <summary>
        /// カメラの初期設定を行います。
        /// 各仮想カメラのプライオリティを設定し、初期カメラをアクティブにします。
        /// </summary>
        private void InitializeCameras()
        {
            // 仮想カメラリストのバリデーション
            if (_cameraEntries == null || _cameraEntries.Count == 0)
            {
                Debug.LogWarning("仮想カメラが設定されていません。CameraCtrlは機能しません。", this);
                return;
            }

            // 全てのカメラを非アクティブな状態（低いプライオリティ）にする
            foreach (var entry in _cameraEntries)
            {
                if (entry.Camera != null)
                {
                    // 各カメラの基本プライオリティを設定。後から変更できるようBASE_PRIORITYにオフセットを加算しない
                    entry.Camera.Priority = BASE_PRIORITY;
                }
                else
                {
                    Debug.LogWarning($"_cameraEntriesリストにnullの要素があります。", this);
                }
            }

            // 初期カメラをアクティブにする
            if (!string.IsNullOrEmpty(_initialActiveCameraKey))
            {
                SetActiveCamera(_initialActiveCameraKey);
            }
            else
            {
                Debug.LogWarning("初期アクティブカメラのキーが設定されていません。最初のカメラをデフォルトでアクティブにします。", this);
                // キーが設定されていない場合は、リストの最初の有効なカメラをアクティブにする
                if (_cameraEntries.Count > 0 && _cameraEntries[0].Camera != null)
                {
                    SetActiveCamera(_cameraEntries[0].key);
                }
                else
                {
                    Debug.LogError("アクティブにできるカメラがありません。", this);
                }
            }
        }

        /// <summary>
        /// 指定されたキーのカメラをアクティブにし、_currentActiveCameraKeyを更新します。
        /// </summary>
        /// <param name="key">アクティブにするカメラのキー。</param>
        private void SetActiveCamera(string key)
        {
            CinemachineCamera targetCam = GetCamera(key);

            if (targetCam != null)
            {
                // 現在アクティブなカメラがあればプライオリティを元に戻す
                if (!string.IsNullOrEmpty(_currentActiveCameraKey))
                {
                    CinemachineCamera prevCam = GetCamera(_currentActiveCameraKey);
                    if (prevCam != null)
                    {
                        prevCam.Priority = BASE_PRIORITY;
                    }
                }

                // 新しいカメラをアクティブにする
                targetCam.Priority = BASE_PRIORITY + ACTIVE_CAMERA_PRIORITY_OFFSET;
                _currentActiveCameraKey = key;
                Debug.Log($"カメラ '{key}' がアクティブになりました。");
            }
            else
            {
                Debug.LogError($"無効なカメラキーが指定されました: '{key}'。またはカメラが見つかりません。", this);
            }
        }

        /// <summary>
        /// キーに基づいてCinemachineCameraを取得します。
        /// </summary>
        /// <param name="key">取得するCinemachineCameraのキー。</param>
        /// <returns>対応するCinemachineCameraインスタンス、または見つからない場合はnull。</returns>
        public CinemachineCamera GetCamera(string key)
        {
            if (_cameraEntries == null)
            {
                Debug.LogError("CameraCtrl: _cameraEntriesがnullです。SetupCamerasが実行されていませんか？", this);
                return null;
            }

            CameraEntry entry = _cameraEntries.Find(x => x.key == key);
            if (entry == null || entry.Camera == null)
            {
                Debug.LogWarning($"CameraCtrl: キー '{key}' に対応するCinemachineCameraが見つからないか、参照が切れています。", this);
                return null;
            }
            return entry.Camera;
        }

        // --- Public Methods

        /// <summary>
        /// 指定されたキーのカメラに切り替えます。
        /// R3 (UniRx.Async) を使用して、カメラ切り替えと待機を非同期に処理します。
        /// </summary>
        /// <param name="targetCameraKey">切り替え対象のカメラキー。</param>
        public void ChangeCamera(string targetCameraKey)
        {
            // 無効なキーまたは現在のカメラと同じ場合は何もしない
            if (targetCameraKey == _currentActiveCameraKey || string.IsNullOrEmpty(targetCameraKey))
            {
                return;
            }

            CinemachineCamera targetCam = GetCamera(targetCameraKey);
            if (targetCam == null)
            {
                Debug.LogWarning($"CameraCtrl: 切り替え対象のカメラ '{targetCameraKey}' が見つからないため、切り替えを中止します。", this);
                return;
            }

            // 新しいカメラをアクティブにする
            SetActiveCamera(targetCameraKey);

            // カメラのブレンド時間だけ待機
            Observable.Timer(System.TimeSpan.FromSeconds(_cameraBlendTime))
                .Subscribe(_ =>
                {
                    // カメラ切り替え完了後の処理が必要であればここに記述
                    // 例: Debug.Log($"カメラがキー: '{targetCameraKey}' に切り替わりました。");
                })
                .AddTo(this); // GameObjectが破棄されたときに購読を解除
        }

        /// <summary>
        /// 現在アクティブなカメラのキーを取得します。
        /// </summary>
        public string GetCurrentActiveCameraKey()
        {
            return _currentActiveCameraKey;
        }


        // --- Editor Integration
#if UNITY_EDITOR
        [CustomEditor(typeof(CameraCtrl))]
        public class CameraCtrlEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                // デフォルトのInspectorを表示
                DrawDefaultInspector();

                CameraCtrl cameraCtrl = (CameraCtrl)target;

                EditorGUILayout.Space();

                if (GUILayout.Button("Setup Cameras (Find All Scene Virtual Cameras)"))
                {
                    cameraCtrl.SetupCameras();
                }

                EditorGUILayout.HelpBox(
                    "「Setup Cameras」ボタンを押すと、シーン上の全てのCinemachineCameraを自動検出してリストに追加します。\n" +
                    "キーは自動的にGameObject名が設定されます。GameObject名が変更された場合も自動で更新されます。\n" +
                    "参照が切れたカメラは自動的にリストから削除されます。",
                    MessageType.Info
                );
            }
        }
#endif
    }
}