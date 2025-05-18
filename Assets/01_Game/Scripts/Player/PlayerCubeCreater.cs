using R3;
using R3.Triggers;
using UnityEditor.Search;
using UnityEngine;

public class PlayerCubeCreater : BasePlayerComponent
{
    // ---------- SerializeField
    [SerializeField] private Cube _cubePrefab;
    [SerializeField] private float _rayDist = 50f;

    // ---------- Field
    private Vector3 _createPos;
    private bool _canCreated;

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
                        _canCreated = true;

                        _createPos = cube.transform.position + hit.normal;
                    }
                    else
                    {
                        _canCreated = false;
                    }
                }
            });

        InputEventProvider.Create
            .Where(x => x)
            .Where(x => _canCreated)
            .Subscribe(_ =>
            {
                var createCube = Instantiate(
                            _cubePrefab,
                            _createPos,
                            transform.rotation);

                createCube.transform.SetParent(transform);
            });
    }
}
