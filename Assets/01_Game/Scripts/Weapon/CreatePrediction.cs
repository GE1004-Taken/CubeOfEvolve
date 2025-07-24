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
    /// 生成可能フラグを変えながら隣接チェック
    /// </summary>
    /// <returns></returns>
    public void CheckCanCreate()
    {
        _canCreated.Value = CheckNeighboringAllCube();
    }

    /// <summary>
    /// 全てのキューブが隣接しているかチェック
    /// </summary>
    /// <returns></returns>
    public bool CheckNeighboringAllCube()
    {
        foreach (var cube in _cubes)
        {
            if (CheckNeighboringCube(cube, 1f)) continue;

            return false;
        }

        return true;
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

                // 0.45fは隣接しているキューブを除外する為
                var halfScale = cubeScale * 0.45f;

                // 対象のキューブにめり込んでいるコライダー数
                var cubeInsideColliders = Physics.OverlapBox(
                    cube.transform.position,
                    new Vector3(halfScale, halfScale, halfScale),
                    cube.transform.rotation,
                    LayerMask.GetMask("Player"));

                // その数が0より大きいなら重なっている
                if (cubeInsideColliders.Length > 0)
                {
                    // 自分自身が重なっている判定になるのを防止
                    if (cubeInsideColliders[0] != cube.GetComponent<Collider>())
                    {
                        return false;
                    }
                }

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

    /// <summary>
    /// 予期しない時に生成出来てしまうのを防ぐ関数
    /// </summary>
    public void ResistCreate()
    {
        // 生成できないようにする
        _canCreated.Value = false;
    }

    /// <summary>
    /// このスクリプトがアタッチされているオブジェクトの色を戻す
    /// </summary>
    public void ChangeNormalColor()
    {
        foreach (var renderer in _renderer)
        {
            renderer.material = _normalMaterial;
        }
    }

    /// <summary>
    /// このスクリプトがアタッチされているオブジェクトを色を不可能を示す色に変える
    /// </summary>
    public void ChangeFalseColor()
    {
        foreach (var renderer in _renderer)
        {
            renderer.material = _falseMaterial;
        }
    }

    public void DestroyCheck()
    {

    }
}
