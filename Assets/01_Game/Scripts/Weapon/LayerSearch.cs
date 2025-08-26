using System.Collections.Generic;
using UnityEngine;

public class LayerSearch : MonoBehaviour
{
    // ---------------------------- SerializeField
    [Header("障害物を貫通するサーチを行うか設定")]
    [SerializeField] private bool _canPenetrate; // 障害物を貫通するサーチを行うかどうか

    // ---------------------------- Field
    private float _range;                        // 探索範囲
    private LayerMask _layerMask;                // 検出対象のレイヤー
    private GameObject _nearestTargetObj;        // 最も近い対象オブジェクト
    private readonly List<GameObject> _nearestTargetList = new(); // 範囲内の敵オブジェクト一覧

    // ---------------------------- Property
    /// <summary>
    /// 最も近い対象オブジェクト
    /// </summary>
    public GameObject NearestTargetObj
    {
        get
        {
            SearchEnemiesInRange();
            return _nearestTargetObj;
        }
    }

    /// <summary>
    /// 範囲内の対象オブジェクト一覧
    /// </summary>
    public List<GameObject> NearestTargetList
    {
        get
        {
            SearchEnemiesInRange();
            return _nearestTargetList;
        }
    }

    // ---------------------------- PrivateMethod
    /// <summary>
    /// 範囲内の敵を検出し、最も近い敵を特定する。
    /// </summary>
    private void SearchEnemiesInRange()
    {
        float nearestDistance = float.MaxValue;
        _nearestTargetObj = null;
        _nearestTargetList.Clear();

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            _range,
            _layerMask);

        foreach (var hit in hits)
        {
            if (hit == null) continue;

            GameObject enemyRoot = hit.transform.root.gameObject;

            // 貫通サーチが無効なら Raycast で遮蔽物チェック
            if (!_canPenetrate)
            {
                Vector3 dir = (hit.transform.position - transform.position).normalized;
                float distance = Vector3.Distance(transform.position, hit.transform.position);

                if (Physics.Raycast(transform.position, dir, out RaycastHit rayHit, distance))
                {
                    // Rayが敵に当たらなければブロックされている
                    if (rayHit.transform.root.gameObject != enemyRoot)
                    {
                        continue;
                    }
                }
            }

            // リストに未追加なら追加
            if (!_nearestTargetList.Contains(enemyRoot))
            {
                _nearestTargetList.Add(enemyRoot);
            }

            // 最も近い敵を更新
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < nearestDistance)
            {
                nearestDistance = dist;
                _nearestTargetObj = enemyRoot;
            }
        }
    }


    // ---------------------------- PublicMethod
    /// <summary>
    /// 探索設定を初期化。
    /// </summary>
    /// <param name="range">探索範囲</param>
    /// <param name="layerName">対象レイヤー名</param>
    public void Initialize(float range, LayerMask layerMask)
    {
        _range = range;
        _layerMask = layerMask;
    }
}
