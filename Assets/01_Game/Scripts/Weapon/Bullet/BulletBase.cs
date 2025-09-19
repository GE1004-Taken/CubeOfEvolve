using UnityEngine;

public class BulletBase : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField] protected string _hitSEName;
    [SerializeField] protected float _destroySecond = 10f;

    // ---------------------------- Field
    protected LayerMask _targetLayerMask;
    protected float _attack;
    protected float _currentDestroySecond = 0f;

    // ---------------------------- Property
    public string TargetLayerMask => LayerMask.LayerToName(_targetLayerMask);

    // ---------------------------- ProtectedMethod
    /// <summary>
    /// ©‘RÁ–Åˆ—
    /// </summary>
    protected void DestroySecondCount()
    {
        _currentDestroySecond += Time.deltaTime;

        if (_currentDestroySecond >= _destroySecond)
        {
            Destroy(gameObject);
        }
    }

    // ---------------------------- PublicMethod
    public void Initialize(
        LayerMask layerMask,
        float attack)
    {
        _targetLayerMask = layerMask;
        _attack = attack;
    }
}
