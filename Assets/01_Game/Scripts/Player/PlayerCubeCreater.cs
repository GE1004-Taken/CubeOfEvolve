using R3;
using R3.Triggers;
using UnityEditor.Search;
using UnityEngine;

public class PlayerCubeCreater : BasePlayerComponent
{
    // ---------- SerializeField
    [SerializeField] private Cube _cubePrefab;
    [SerializeField] private float _rayDist = 50f;

    [SerializeField] private Material _normalMaterial;
    [SerializeField] private Material _trueMaterial;
    [SerializeField] private Material _falseMaterial;

    // ---------- RP
    private bool _canCreated = new();

    // ---------- Field
    public Cube _predictCube = null;
    private MeshRenderer _predictCubeMeshRenderer;
    private Vector3 _createPos;

    // ---------- UnityMessage
    protected override void OnInitialize()
    {
        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(
                    mouseRay.origin,
                    mouseRay.direction * _rayDist,
                    out RaycastHit hit))
                {
                    if (hit.collider.TryGetComponent<Cube>(out var cube))
                    {
                        _createPos = cube.transform.position + hit.normal;

                        // 設置予測キューブを生成
                        if (_predictCube == null)
                        {
                            _predictCube = Instantiate(
                                    _cubePrefab,
                                    _createPos,
                                    transform.rotation);

                            _predictCube.transform.SetParent(transform);

                            _predictCubeMeshRenderer = _predictCube.GetComponent<MeshRenderer>();

                            _predictCubeMeshRenderer.material = _trueMaterial;
                        }
                        // 設置予測キューブの位置を更新
                        else
                        {
                            _predictCube.transform.position = _createPos;
                        }

                        _canCreated = true;
                    }
                    else
                    {
                        _canCreated = false;

                        if (_predictCube != null)
                        {
                            _predictCubeMeshRenderer.material = _falseMaterial;

                            Destroy(_predictCube.gameObject);
                        }
                    }
                }
            });

        InputEventProvider.Create
            .Where(x => x)
            .Where(x => _canCreated)
            .Subscribe(_ =>
            {
                _predictCube.ActiveCube();
                _predictCubeMeshRenderer.material = _normalMaterial;
                _predictCube = null;
            });
    }
}
