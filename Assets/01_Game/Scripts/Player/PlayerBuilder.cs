using Assets.IGC2025.Scripts.GameManagers;
using R3;
using R3.Triggers;
using TreeEditor;
using UnityEngine;

public class PlayerBuilder : BasePlayerComponent
{
    // ---------- SerializeField
    [SerializeField] private SerializableReactiveProperty<GameObject> _selectedWeapon;
    [SerializeField] private Cube _cubePrefab;
    [SerializeField] private float _rayDist = 50f;

    // ---------- Field
    private CreatePrediction _predictObject = null;
    private Vector3 _createPos;

    // ---------- UnityMessage
    protected override void OnInitialize()
    {
        var currentState =
            GameManager.Instance.CurrentGameState;

        // 戦闘に戻ったらリセット
        currentState
            .Where(_ => _predictObject != null)
            .Where(_ => currentState.CurrentValue == GameState.BATTLE)
            .Subscribe(_ =>
            {
                Destroy(_predictObject.gameObject);
            });

        // 設置予測処理
        this.UpdateAsObservable()
            .Where(_ => currentState.CurrentValue == GameState.BUILD)
            .Subscribe(_ =>
            {
                var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (!Physics.Raycast(
                    mouseRay.origin,
                    mouseRay.direction * _rayDist,
                    out RaycastHit hit)) return;

                if (hit.collider.TryGetComponent<Cube>(out var cube))
                {
                    _createPos = cube.transform.position + hit.normal;

                    // 生成対象の生成予測スクリプト
                    CreatePrediction targetCreatePrediction;

                    // 武器が選択されていたら
                    if(_selectedWeapon.Value != null)
                    {
                        targetCreatePrediction =
                        _selectedWeapon.Value.GetComponent<CreatePrediction>();
                    }
                    else
                    {
                        targetCreatePrediction =
                        _cubePrefab.GetComponent<CreatePrediction>();
                    }

                    // 設置予測キューブの多重生成防止
                    if (_predictObject == null)
                    {
                        _predictObject = Instantiate(
                            targetCreatePrediction,
                            _createPos,
                            transform.rotation);

                        _predictObject.transform.SetParent(transform);
                    }

                    // 設置予測キューブの位置を更新
                    _predictObject.transform.position = _createPos;

                    // 隣接するキューブがあるかチェック
                    _predictObject?.CheckNeighboringAllCube();
                }
                // 設置予測キューブが生成されていたら削除
                else
                {
                    if (_predictObject == null) return;

                    Destroy(_predictObject.gameObject);
                }
            });

        // 武器が変わったら予測キューブ更新
        _selectedWeapon
            .Where(_ => currentState.CurrentValue == GameState.BUILD)
            .Where(_ => _predictObject != null)
            .Subscribe(_ =>
            {
                Destroy(_predictObject.gameObject);
            });

        // 回転処理
        InputEventProvider.Move
            .Where(_ => currentState.CurrentValue == GameState.BUILD)
            .DistinctUntilChanged()
            .Subscribe(x =>
            {
                RotateWeapon(90f * (int)x.y, -90f * (int)x.x, 0f);
            });

        // 生成処理
        InputEventProvider.Create
            .Where(x => x)
            .Where(_ => currentState.CurrentValue == GameState.BUILD)
            .Where(_ => _predictObject != null)
            .Where(_ => _predictObject.CanCreated.CurrentValue)
            .Subscribe(_ =>
            {
                _predictObject.ActiveWeapon();
                _predictObject = null;
            });
    }

    // ---------- Event
    public void RotateWeapon(float x, float y, float z)
    {
        if (_predictObject == null) return;

        _predictObject.transform.Rotate(new Vector3(x, y, z));
    }
}
