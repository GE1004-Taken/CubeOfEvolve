using R3;
using System.Collections.Generic;
using UnityEngine;

public class CreatePrediction : MonoBehaviour
{
    // ---------- SerializeField
    [SerializeField] private List<Cube> _cubes = new();
    [SerializeField] private List<Renderer> _renderer = new();

    [SerializeField] private Material _normalMaterial;
    [SerializeField] private Material _trueMaterial;
    [SerializeField] private Material _falseMaterial;

    [SerializeField] private SerializableReactiveProperty<bool> _isActived;

    public ReadOnlyReactiveProperty<bool> IsActived => _isActived;

    private ReactiveProperty<bool> _canCreated = new();
    public ReadOnlyReactiveProperty<bool> CanCreated => _canCreated;

    private Vector3[] _directions =
    {
        Vector3.up,
        -Vector3.up,
        Vector3.right,
        -Vector3.right,
        Vector3.forward,
        -Vector3.forward
    };

    // ---------- UnityMessage
    private void Start()
    {
        // 設置が出来るかで色を変える
        _canCreated
            .Where(_ => !_isActived.Value)
            .Subscribe(x =>
            {
                foreach (var renderer in _renderer)
                {
                    if (x)
                    {
                        renderer.material = _trueMaterial;
                    }
                    else
                    {
                        renderer.material = _falseMaterial;
                    }
                }
            })
            .AddTo(this);

        // 設置や削除処理
        _isActived
            .Skip(1)
            .Subscribe(x =>
            {
                if (x)
                {
                    foreach (var renderer in _renderer)
                    {
                        renderer.material = _normalMaterial;
                    }
                    foreach (var cube in _cubes)
                    {
                        cube.GetComponent<BoxCollider>().enabled = true;
                    }
                }
                else
                {
                    Destroy(gameObject);
                }
            })
            .AddTo(this);
    }

    // ---------- PrivateMethod
    /// <summary>
    /// 全てのキューブが隣接しているかチェック
    /// </summary>
    /// <returns></returns>
    public void CheckNeighboringAllCube()
    {
        foreach (var cube in _cubes)
        {
            if (CheckNeighboringCube(cube, 1f)) continue;

            _canCreated.Value = false;
            return;
        }

        _canCreated.Value = true;
    }

    /// <summary>
    /// キューブが隣接しているかチェック
    /// </summary>
    /// <param name="cube">対象のキューブ</param>
    /// <param name="cubeScale">キューブの一辺の長さ</param>
    /// <returns></returns>
    private bool CheckNeighboringCube(
    Cube cube,
    float cubeScale)
    {
        foreach (var direction in _directions)
        {
            if (Physics.Raycast(
            cube.transform.position,
            direction,
            out RaycastHit hit,
            cubeScale))
            {
                if (!hit.collider.CompareTag("Cube")) continue;

                // 0.49fは隣接しているキューブを除外する為
                var halfScale = cubeScale * 0.49f;

                // 対象のキューブにめり込んでいるコライダー数
                var cubeInsideColliders = Physics.OverlapBox(
                    cube.transform.position,
                    new Vector3(halfScale, halfScale, halfScale),
                    cube.transform.rotation);

                // その数が0より大きいなら設置済みとみなす
                if (cubeInsideColliders.Length > 0) return false;

                return true;
            };
        }
        return false;
    }

    // ---------- Event
    /// <summary>
    /// このスクリプトがアタッチされているオブジェクトを生成する
    /// </summary>
    public void CreateObject()
    {
        _isActived.Value = true;
    }

    /// <summary>
    /// このスクリプトがアタッチされているオブジェクトを消去する
    /// </summary>
    public void RemoveObject()
    {
        _isActived.Value = false;
    }
}
