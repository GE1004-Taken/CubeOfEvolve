using UnityEngine;

public class LayerSearch : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField] private float _range;
    [SerializeField] private LayerMask _mask;

    // ---------------------------- Field
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
            _mask))
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
}
