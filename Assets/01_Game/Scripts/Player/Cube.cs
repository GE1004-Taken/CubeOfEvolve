using R3;
using UnityEngine;

public class Cube : MonoBehaviour
{
    // ---------- SerializeField
    [SerializeField] private Vector3 _index;
    [SerializeField] private SerializableReactiveProperty<bool> _isActived;
    [SerializeField] private BoxCollider _collider;

    // ---------- Property
    public Vector3 Index
    {
        get => _index;
        set => _index = value;
    }

    // ---------- UnityMesage
    private void Start()
    {
        if (_isActived.Value)
        {
            _collider.enabled = true;
        }
        else
        {
            _collider.enabled = false;
        }

        _isActived
            .Skip(1)
            .DistinctUntilChanged()
            .Subscribe(x =>
            {
                if (x)
                {
                    _collider.enabled = true;
                }
                else
                {
                    Destroy(gameObject);
                }
            });
    }

    // ---------- Event
    public void ActiveCube()
    {
        _isActived.Value = true;
    }

    public void InactiveCube()
    {
        _isActived.Value = false;
    }
}
