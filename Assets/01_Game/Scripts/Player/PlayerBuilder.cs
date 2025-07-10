using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using Assets.IGC2025.Scripts.GameManagers;
using System.Collections.Generic;
using R3;
using R3.Triggers;
using UnityEngine;

public class PlayerBuilder : BasePlayerComponent
{
    // ---------- SerializeField
    [SerializeField] private Cube _cubePrefab;
    [SerializeField] private float _rayDist = 50f;

    // ---------- Field
    // 対象の生成予測スクリプト
    private CreatePrediction _targetCreatePrediction;

    // 対象の生成予測キューブ
    private CreatePrediction _predictCube = null;
    private Vector3 _createPos;
    private Vector3 _createPosOffset = new Vector3(0f, 0.5f, 0f);
    // 仮
    private ModuleData _currentModuleData;

    public bool _isRemoving;

    private GameObject _prevRemoveObject;
    private GameObject _curRemoveObject;

    // 各方向を格納
    private Vector3[] _directions =
    {
        Vector3.up,
        -Vector3.up,
        Vector3.right,
        -Vector3.right,
        Vector3.forward,
        -Vector3.forward
    };

    // プレイヤーと繋がっているオブジェクト達
    private List<GameObject> _connectCheckedObjects = new();

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

                    _currentModuleData = null;
                }
            })
            .AddTo(this);

        // 戦闘に戻ったらリセット
        currentState
            .Where(_ => _predictCube != null)
            .Where(_ => currentState.CurrentValue == GameState.BATTLE)
            .Subscribe(_ =>
            {
                _targetCreatePrediction = null;
                _currentModuleData = null;
                Destroy(_predictCube.gameObject);
            })
            .AddTo(this);


        // 設置予測処理
        this.UpdateAsObservable()
            .Where(_ => currentState.CurrentValue == GameState.BUILD)
            .Subscribe(_ =>
            {
                var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                // レイに何も当たらなかったら処理しない
                if (!Physics.Raycast(
                    mouseRay.origin,
                    mouseRay.direction * _rayDist,
                    out RaycastHit hit)) return;

                if (!_isRemoving)
                {
                    if (_targetCreatePrediction == null) return;

                    if (_predictCube == null)
                    {
                        _predictCube = Instantiate(
                            _targetCreatePrediction,
                            hit.point,
                            transform.rotation);

                        _predictCube.transform.SetParent(this.transform);
                    }

                    // レイがキューブが当たったら処理
                    if (hit.collider.TryGetComponent<Cube>(out var cube))
                    {
                        // 生成位置取得
                        _createPos = cube.transform.position + hit.normal;

                        // 設置予測キューブの位置を更新
                        _predictCube.transform.position = _createPos;

                        // 隣接するキューブがあるかチェック
                        _predictCube?.CheckNeighboringAllCube();
                    }
                    // レイがキューブに当たらなくなったら処理
                    else
                    {
                        // レイが土台に当たっていない時は生成できないようにする
                        _predictCube.ResistCreate();

                        _predictCube.transform.position = hit.point + _createPosOffset;
                    }
                }
                else
                {
                    // レイに当たった物がプレイヤーの物でなかったら
                    if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Player"))
                    {
                        // 直前にプレイヤーの物に当たっていないなら処理しない
                        if (_curRemoveObject == null) return;

                        // 削除対象にレイが当たらなくなったら元の色に戻す
                        _curRemoveObject
                        .GetComponent<CreatePrediction>()
                        .ChangeNormalColor();

                        // 今レイに当たっている物変数をNullにする
                        _curRemoveObject = null;

                        return;
                    }

                    // レイに当たっているものが変わっていないなら処理しない
                    if (_curRemoveObject == hit.collider.GetComponentInParent<CreatePrediction>().gameObject) return;

                    // 直前にプレイヤーの物に当たっていないなら処理しない
                    if (_curRemoveObject != null)
                    {
                        _curRemoveObject
                        .GetComponent<CreatePrediction>()
                        .ChangeNormalColor();
                    }

                    //
                    _curRemoveObject =
                        hit
                        .collider
                        .GetComponentInParent<CreatePrediction>()
                        .gameObject;

                    _curRemoveObject.GetComponent<CreatePrediction>()
                    .ChangeFalseColor();
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

        // 生成・削除処理
        InputEventProvider.Create
            .Where(x => x)
            .Where(_ => currentState.CurrentValue == GameState.BUILD)
            .Subscribe(_ =>
            {
                // 生成モード
                if (!_isRemoving)
                {
                    // 予測キューブが生成されていないなら処理しない
                    if (_predictCube == null) return;

                    // 生成条件を満たしていないなら処理しない
                    if (!_predictCube.CanCreated.CurrentValue) return;

                    // 武器・キューブの設置
                    _predictCube.CreateObject();

                    // 生成予測キューブをヌルに
                    _predictCube = null;

                    // 生成するものがモジュールの時
                    if (_currentModuleData != null)
                    {
                        // オプションの時
                        if (_currentModuleData.ModuleType == ModuleData.MODULE_TYPE.Options)
                        {
                            _currentModuleData.Model.GetComponent<OptionBase>().WhenEquipped();
                        }

                        // モジュールの所持数を減らす
                        RuntimeModuleManager.Instance.ChangeModuleQuantity(
                            _currentModuleData.Id,
                            -1);
                    }
                    // 生成するものがキューブの時
                    else
                    {
                        // キューブの設置してる数を増やす
                        Core.AddCube();
                    }
                }
                // 削除モード
                else
                {
                    // 削除対象が存在しないなら処理しない
                    if (_curRemoveObject == null) return;

                    // WeaponBaseを継承していたらモジュールと見なす
                    if (_curRemoveObject.TryGetComponent<WeaponBase>(out var weapon))
                    {
                        // 削除対象のモジュールの所持数を増やす
                        RuntimeModuleManager.Instance.ChangeModuleQuantity(weapon.Id, 1);
                    }
                    // そうでないならキューブ(土台)と見なす
                    else
                    {
                        // キューブの設置数を減らす
                        Core.RemoveCube();
                    }

                    // 対象のオブジェクトを削除する
                    Destroy(_curRemoveObject.gameObject);
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

    /// <summary>
    /// 生成モードと削除モードを切り替える
    /// </summary>
    public void ChangeBuildMode()
    {
        // 削除中フラグを反転
        _isRemoving = !_isRemoving;
    }

    /// <summary>
    /// プレイヤーと繋がっているかを検証する
    /// </summary>
    /// <param name="cube"></param>
    /// <param name="cubeScale"></param>
    public void ConnectCheck(
       GameObject cube,
       float cubeScale)
    {
        _connectCheckedObjects.Add(cube);

        foreach (var direction in _directions)
        {
            if (Physics.Raycast(
            cube.transform.position,
            direction,
            out RaycastHit hit,
            cubeScale,
            LayerMask.GetMask("Player")))
            {
                // 対象のオブジェクトの生成予測スクリプトを取得
                var prediction = GetComponentInParent<CreatePrediction>();

                // 無いなら処理しない
                if (prediction == null) continue;

                // キューブスクリプトを取得
                var targetCube = prediction.gameObject.GetComponent<Cube>();

                // 対象のオブジェクトでまたこのスクリプトを実行
                //prediction.DestroyCheck(
                //    targetCube,
                //    cubeScale);
            }
        }
    }
}
