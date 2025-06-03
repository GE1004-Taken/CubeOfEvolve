using UnityEngine;
using UnityEngine.UI;

public class LayerSearch : MonoBehaviour
{
    // ---------------------------- Field
    private float _range;
    private string _maskName;

    private GameObject _nearestEnemyObj;

    // ---------------------------- Property
    public GameObject NearestEnemyObj => _nearestEnemyObj;

    // ---------------------------- UnityMassage
    private void Update()
    {
        float nearestEnemyDis = 0;
        _nearestEnemyObj = null;

        // ˆê”Ô‹ß‚¢“G‚ðŽæ“¾
        foreach (RaycastHit hit in Physics.SphereCastAll(
            transform.position,
            _range,
            Vector3.down,
            0,
            LayerMask.GetMask(_maskName)))
        {

            if (hit.transform == null) continue;

            var dis = Vector3.Distance(
                transform.position,
                hit.transform.position);

            if (nearestEnemyDis == 0f || dis < nearestEnemyDis)
            {
                nearestEnemyDis = dis;
                _nearestEnemyObj = hit.transform.gameObject;
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _range);
    }

    // ---------------------------- PublicMethod
    public void Initialize(float range, string layerName)
    {
        _range = range;
        _maskName = layerName;
    }
}
