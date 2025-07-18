using UnityEngine;

public class BulletBase : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField] protected string _hitSEName;

    // ---------------------------- Field
    protected LayerMask _targetLayerMask;
    protected float _attack;

    protected float _destroySecond = 10f;

    // ---------------------------- Property
    public string TargetLayerMask => LayerMask.LayerToName(_targetLayerMask);
}
