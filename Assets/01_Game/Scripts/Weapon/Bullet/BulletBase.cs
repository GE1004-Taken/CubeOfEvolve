using UnityEngine;

public class BulletBase : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField] protected string _hitSEName;

    // ---------------------------- Field
    protected string _targetLayerName;
    protected float _attack;

    protected float _destroySecond = 10f;

    // ---------------------------- Property
    public string TargetLayerName => _targetLayerName;
}
