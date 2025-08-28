using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using Assets.IGC2025.Scripts.GameManagers;
using System.Collections.Generic;
using R3;
using R3.Triggers;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Assets.AT;

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
    // 選択されているモジュールのデータ
    private ModuleData _currentModuleData;
    // 削除モード中か
    private bool _isRemoving;
    // 1個前の消す対象だったオブジェクト
    private GameObject _prevRemoveObject;
    // 現在消す対象のオブジェクト
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
    // 既に生成されているオブジェクト達
    private List<GameObject> _createdObjects = new();
    // プレイヤーと繋がっているオブジェクト達
    private List<GameObject> _connectCheckedObjects = new();
    // プレイヤーと繋がって居なかったオブジェクト達
    private List<CreatePrediction> _disconnectObjects = new();
    // プレイヤーコアと繋がっているか確認している関数の現在の実行数
    private int _runningConnectCheckCount;

    // ---------- Property
    public List<GameObject> CreatedObjects
    {
        get => _createdObjects;
        set => _createdObjects = value;
    }

    public bool GetIsRemoving => _isRemoving;

    // ---------- R3
    private Subject<ModuleData> _selectModuleData = new();
    public Observable<ModuleData> OnSelectModuleData => _selectModuleData;

    private Subject<Unit> _createSubject = new();
    public Observable<Unit> OnCreate => _createSubject;

    private Subject<Unit> _removeSubject = new();

    public Subject<Unit> OnRemove => _removeSubject;

    // ---------- UnityMessage
    /// <summary>
    /// UnityMessageのStart()と同義
    /// </summary>
    protected override void OnInitialize()
    {
        _createSubject.AddTo(this);

        var currentState =
            GameManager.Instance.CurrentGameState;

        // 選択されたモジュールをIDから取得する
        _selectModuleData
            .Subscribe(moduleData =>
            {
                // 既に生成予測キューブが生成されていたら破壊
                if (_predictCube != null)
                {
                    Destroy(_predictCube.gameObject);
                }

                // 削除モード中に設置したいものが選択されたら削除モードを終える
                if (_isRemoving)
                {
                    _isRemoving = false;
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
                _isRemoving = false;
                _targetCreatePrediction = null;
                _currentModuleData = null;
                Destroy(_predictCube.gameObject);
            })
            .AddTo(this);


        // 生成・削除予測処理
        this.UpdateAsObservable()
            .Where(_ => currentState.CurrentValue == GameState.BUILD || currentState.CurrentValue == GameState.TUTORIAL)
            .Subscribe(_ =>
            {
                var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                // レイに何も当たらなかったら処理しない
                if (!Physics.Raycast(
                    mouseRay.origin,
                    mouseRay.direction * _rayDist,
                    out RaycastHit hit)) return;

                // 生成予測処理
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

                        // キューブの設置上限を超えていないかモジュールを選択していたら
                        if (Core.CubeCount.CurrentValue < Core.MaxCubeCount.CurrentValue
                        || _currentModuleData != null)
                        {
                            // 隣接するキューブがあるかチェック
                            _predictCube?.CheckCanCreate();
                        }
                        else
                        {
                            _predictCube.ResistCreate();
                        }

                    }
                    // レイがキューブに当たらなくなったら処理
                    else
                    {
                        // レイが土台に当たっていない時は生成できないようにする
                        _predictCube.ResistCreate();

                        _predictCube.transform.position = hit.point + _createPosOffset;
                    }
                }
                // 削除予測処理
                else
                {
                    // レイに当たった物がプレイヤーの物でなかったら
                    if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Player"))
                    {
                        // 繋がっていない物リストをリセット
                        ResetDisconnectObjects();

                        // 今レイに当たっている物変数をNullにする
                        _curRemoveObject = null;

                        return;
                    }

                    // レイに当たった物がプレイヤーコアなら処理しない
                    if (hit.collider.gameObject == this.gameObject) return;

                    // レイに当たっているものが変わっていないなら処理しない
                    if (_curRemoveObject
                    == hit.collider.GetComponentInParent<CreatePrediction>().gameObject) return;

                    // 現在の削除対象を更新
                    _curRemoveObject =
                        hit
                        .collider
                        .GetComponentInParent<CreatePrediction>()
                        .gameObject;

                    // 繋がっていない物リストをリセット
                    ResetDisconnectObjects();

                    // 生成済みの物が削除後も繋がっているか確認
                    ConnectCheck(this.gameObject, 1f);
                }
            });

        // 回転処理
        InputEventProvider.Move
            .Where(_ => currentState.CurrentValue == GameState.BUILD || currentState.CurrentValue == GameState.TUTORIAL)
            .DistinctUntilChanged()
            .Subscribe(x =>
            {
                RotatePredictCube(90f * (int)x.y, -90f * (int)x.x, 0f);
            })
            .AddTo(this);

        // 生成・削除処理
        InputEventProvider.Create
            .Where(x => x)
            .Where(_ => currentState.CurrentValue == GameState.BUILD || currentState.CurrentValue == GameState.TUTORIAL)
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

                    _createdObjects.Add(_predictCube.gameObject);

                    // 生成予測キューブをヌルに
                    _predictCube = null;

                    // 生成したことを通知する
                    _createSubject.OnNext(Unit.Default);

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

                        var curRuntimeModuleData =
                            RuntimeModuleManager
                            .Instance
                            .GetRuntimeModuleData(_currentModuleData.Id);

                        // 選択しているモジュールの所持数が0以下なら選択解除する
                        if (curRuntimeModuleData.Quantity.CurrentValue <= 0)
                        {
                            _targetCreatePrediction = null;
                            _currentModuleData = null;
                        }
                    }
                    // 生成するものがキューブの時
                    else
                    {
                        // 現在設置しているキューブ数が上限以上なら処理しない
                        if (Core.CubeCount.CurrentValue > Core.MaxCubeCount.CurrentValue) return;
                        // キューブの設置してる数を増やす
                        Core.AddCube();
                    }
                }
                // 削除モード
                else
                {
                    // 削除対象が存在しないなら処理しない
                    if (_curRemoveObject == null) return;

                    // 繋がっていないオブジェクト達を削除
                    RemoveDisconnectObjects();
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

        // 削除モードに移行時生成する対象をリセットする
        if (_isRemoving )
        {
            _targetCreatePrediction = null;
            _currentModuleData = null;
            Destroy(_predictCube.gameObject);
        }
    }

    /// <summary>
    /// プレイヤーと繋がっているかを検証する
    /// </summary>
    /// <param name="cube"></param>
    /// <param name="cubeScale"></param>
    private void ConnectCheck(
       GameObject cube,
       float cubeScale)
    {
        // 現在実行されている関数の数を加算
        _runningConnectCheckCount++;

        // プレイヤーと繋がっているかチェック済みリストに追加
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
                var prediction = hit.collider.gameObject.GetComponentInParent<CreatePrediction>();

                // 無いなら処理しない
                if (prediction == null) continue;

                // 対象を削除した時に繋がっているか確認したいので最初の削除対象は処理しない
                if (prediction.gameObject == _curRemoveObject) continue;

                // 既にチェック済みのものなら処理しない
                if (_connectCheckedObjects.Contains(prediction.gameObject)) continue;

                // すべてのキューブに隣接していないなら消す
                if (!prediction.CheckNeighboringAllCube())
                {
                    Destroy(prediction.gameObject);
                    continue;
                }

                // 対象のオブジェクトでまたこのスクリプトを実行
                ConnectCheck(prediction.gameObject, cubeScale);
            }
        }

        // 現在実行されている関数の数を減算
        _runningConnectCheckCount--;

        // 現在実行されている関数が無くなったら処理
        if (_runningConnectCheckCount <= 0)
        {
            SearchDisconnectedObjects();
        }
    }

    /// <summary>
    /// プレイヤーと繋がっていないモジュール/キューブを洗い出す
    /// </summary>
    private void SearchDisconnectedObjects()
    {
        foreach (var createdObject in _createdObjects)
        {
            // Nullなら処理しない
            if (createdObject == null) continue;
            // プレイヤーと繋がっていたら処理しない
            if (_connectCheckedObjects.Contains(createdObject)) continue;
            // 現在の対象のCreatePredictionを取得
            var curCreatePrediction = createdObject.GetComponent<CreatePrediction>();
            // 繋がっていないリストに追加
            _disconnectObjects.Add(curCreatePrediction);
            // 消える色に変更
            curCreatePrediction.ChangeFalseMaterial();
        }

        // プレイヤーとの接続確認用のリストを初期化
        _connectCheckedObjects.Clear();
    }

    /// <summary>
    /// プレイヤーコアと繋がっていないオブジェクトを削除
    /// </summary>
    private void RemoveDisconnectObjects()
    {
        foreach(var removeTarget in _disconnectObjects)
        {
            // 削除対象を設置済みオブジェクトリストから消しておく(Null対策)
            _createdObjects.Remove(removeTarget.gameObject);

            // 対象を削除
            RemoveObject(removeTarget.gameObject);
        }

        // 削除イベントを通知
        OnRemove.OnNext(Unit.Default);

        // 繋がっていないオブジェクトリストを空に
        ResetDisconnectObjects();
    }

    /// <summary>
    /// 繋がっていないオブジェクトリストをリセット
    /// </summary>
    private void ResetDisconnectObjects()
    {
        // 消える色になっていた物を元の色に戻す
        foreach( var disconnectObject in _disconnectObjects)
        {
            // 中身が無かったら処理しない
            if (disconnectObject == null) continue;

            disconnectObject.ChangeNormalMaterial();
        }

        // 繋がっていないオブジェクトリストをリセット
        _disconnectObjects.Clear();
    }

    /// <summary>
    /// 設置済みのモジュール・キューブを消す
    /// </summary>
    /// <param name="gameObject"></param>
    private void RemoveObject(GameObject gameObject)
    {
        // WeaponBaseを継承していたらモジュールと見なす
        if (gameObject.TryGetComponent<IModuleID>(out var module))
        {
            RuntimeModuleManager.Instance.ChangeModuleQuantity(module.Id, 1);

            if (gameObject.TryGetComponent<OptionBase>(out var option))
            {
                option.ProcessingWhenRemoved();
            }
        }
        else
        {
            Core.RemoveCube();
        }

        Destroy(gameObject);
    }

    public void RemoveAllObjects()
    {
        foreach (var obj in CreatedObjects)
        {
            RemoveObject(obj);
        }

        CreatedObjects.Clear();
    }

}
