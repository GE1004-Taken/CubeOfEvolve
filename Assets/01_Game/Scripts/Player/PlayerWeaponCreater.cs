using R3;
using R3.Triggers;
using UnityEngine;

public class PlayerWeaponCreater : BasePlayerComponent
{
    // ---------- SerializeField
    [SerializeField] private WeaponCreatePrediction _weaponPrefab;
    [SerializeField] private float _rayDist = 50f;

    // ---------- Field
    public WeaponCreatePrediction _predictWeapon = null;
    private Vector3 _createPos;

    // ---------- UnityMessage
    protected override void OnInitialize()
    {
        // 設置予測処理
        this.UpdateAsObservable()
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

                    // 設置予測キューブの多重生成防止
                    if (_predictWeapon == null)
                    {
                        _predictWeapon = Instantiate(_weaponPrefab, _createPos, transform.rotation);

                        _predictWeapon.transform.SetParent(transform);
                    }

                    // 設置予測キューブの位置を更新
                    _predictWeapon.transform.position = _createPos;

                    // 隣接するキューブがあるかチェック
                    _predictWeapon.CheckNeighboringAllCube();
                }
                // 設置予測キューブが生成されていたら削除
                else
                {
                    if (_predictWeapon == null) return;

                    Destroy(_predictWeapon.gameObject);
                }
            });

        // 回転処理
        InputEventProvider.Move
            .DistinctUntilChanged()
            .Subscribe(x =>
            {
                RotateWeapon(90f * (int)x.y, -90f * (int)x.x, 0f);
            });

        // 生成処理
        InputEventProvider.Create
            .Where(x => x)
            .Where(x => _predictWeapon.CanCreated.CurrentValue)
            .Subscribe(_ =>
            {
                _predictWeapon.ActiveWeapon();
                _predictWeapon = null;
            });
    }

    // ---------- Event
    public void RotateWeapon(float x, float y, float z)
    {
        if (_predictWeapon == null) return;

        _predictWeapon.transform.Rotate(new Vector3(x, y, z));
    }
}
