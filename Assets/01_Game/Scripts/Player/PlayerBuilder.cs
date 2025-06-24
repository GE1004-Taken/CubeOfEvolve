using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using Assets.IGC2025.Scripts.GameManagers;
using R3;
using R3.Triggers;
using UnityEngine;

public class PlayerBuilder : BasePlayerComponent
{
    // ---------- SerializeField
    [SerializeField] private ModuleDataStore _moduleDataStore;
    [SerializeField] private Cube _cubePrefab;
    [SerializeField] private float _rayDist = 50f;

    // ---------- Field
    // 対象の生成予測スクリプト
    private CreatePrediction _targetCreatePrediction;

    // 対象の生成予測キューブ
    private CreatePrediction _predictCube = null;
    private Vector3 _createPos;
    // 仮
    private ModuleData _currentModuleData;

    // ---------- R3
    private Subject<ModuleData> _selectModuleData = new();
    public Observable<ModuleData> OnSelectModuleData => _selectModuleData;

    // ---------- UnityMessage
    /// <summary>
    /// UnityMessageのStart()と同義
    /// </summary>
    protected override void OnInitialize()
    {
        var currentState =
            GameManager.Instance.CurrentGameState;

        // 選択されたモジュールをIDから取得する
        _selectModuleData
            .Subscribe(moduleData =>
            {
                // 既に生成予測キューブが生成されていたら破壊
                if (_predictCube != null)
                {
                    Destroy(_predictCube);
                }

                // 武器が選択されていたら
                if (moduleData != null)
                {
                    // その武器の生成予測スクリプト取得
                    _targetCreatePrediction =
                        moduleData
                        .Model
                        .GetComponent<CreatePrediction>();

                    _currentModuleData = moduleData;
                }
                else
                {
                    // キューブの生成予測スクリプト取得
                    _targetCreatePrediction =
                        _cubePrefab
                        .GetComponent<CreatePrediction>();
                }
            })
            .AddTo(this);

        // 戦闘に戻ったらリセット
        currentState
            .Where(_ => _predictCube != null)
            .Where(_ => currentState.CurrentValue == GameState.BATTLE)
            .Subscribe(_ =>
            {
                Destroy(_predictCube.gameObject);
            })
            .AddTo(this);

        // 設置予測処理
        this.UpdateAsObservable()
            .Where(_ => currentState.CurrentValue == GameState.BUILD)
            .Where(_ => _targetCreatePrediction != null)
            .Subscribe(_ =>
            {
                var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                // レイに何も当たらなかったら処理しない
                if (!Physics.Raycast(
                    mouseRay.origin,
                    mouseRay.direction * _rayDist,
                    out RaycastHit hit)) return;

                // レイがキューブが当たったら処理
                if (hit.collider.TryGetComponent<Cube>(out var cube))
                {
                    // 生成位置取得
                    _createPos = cube.transform.position + hit.normal;

                    // 設置予測キューブの多重生成防止
                    if (_predictCube == null)
                    {
                        _predictCube = Instantiate(
                            _targetCreatePrediction,
                            _createPos,
                            transform.rotation);

                        _predictCube.transform.SetParent(transform);
                    }

                    // 設置予測キューブの位置を更新
                    _predictCube.transform.position = _createPos;

                    // 隣接するキューブがあるかチェック
                    _predictCube?.CheckNeighboringAllCube();
                }
                // レイがキューブに当たらなくなったら処理
                else
                {
                    if (_predictCube == null) return;

                    Destroy(_predictCube.gameObject);
                }
            });

        // 回転処理
        InputEventProvider.Move
            .Where(_ => currentState.CurrentValue == GameState.BUILD)
            .DistinctUntilChanged()
            .Subscribe(x =>
            {
                RotatePredictCube(90f * (int)x.y, -90f * (int)x.x, 0f);
            })
            .AddTo(this);

        // 生成処理
        InputEventProvider.Create
            .Where(x => x)
            .Where(_ => currentState.CurrentValue == GameState.BUILD)
            .Where(_ => _predictCube != null)
            .Where(_ => _predictCube.CanCreated.CurrentValue)
            .Subscribe(_ =>
            {
                _predictCube.CreateObject();
                _predictCube = null;

                // オプションの時
                if (_currentModuleData != null
                && _currentModuleData.ModuleType == ModuleData.MODULE_TYPE.Options)
                {
                    _currentModuleData.Model.GetComponent<OptionBase>().WhenEquipped();
                }
            })
            .AddTo(this);
    }

    // ---------- PublicMethod
    /// <summary>
    /// 生成予測キューブを回転させる
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    public void RotatePredictCube(float x, float y, float z)
    {
        if (_predictCube == null) return;

        _predictCube.transform.Rotate(new Vector3(x, y, z));
    }

    /// <summary>
    /// 現在選択しているモジュールを変更する
    /// </summary>
    /// <param name="moduleData"></param>
    public void SetModuleData(ModuleData moduleData)
    {
        _selectModuleData.OnNext(moduleData);
    }

    /// <summary>
    /// 生成するものをキューブに変更する
    /// </summary>
    public void SetCube()
    {
        _selectModuleData.OnNext(null);
    }
}
