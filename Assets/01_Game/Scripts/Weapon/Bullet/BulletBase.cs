using UnityEngine;

public class BulletBase : MonoBehaviour
{
    // ---------------------------- Field
    protected string _targetTag;
    protected float _attack;

    protected float _destroySecond = 10f;

    // ---------------------------- Property
    public string TargetTag => _targetTag;
}
