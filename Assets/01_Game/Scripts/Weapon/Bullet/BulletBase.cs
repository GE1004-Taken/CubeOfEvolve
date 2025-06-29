using UnityEngine;

public class BulletBase : MonoBehaviour
{
    // ---------------------------- Field
    protected string _targetLayerName;
    protected float _attack;

    protected float _destroySecond = 10f;

    // ---------------------------- Property
    public string TargetLayerName => _targetLayerName;
}
