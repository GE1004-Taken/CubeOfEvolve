using System.Collections.Generic;
using UnityEngine;

public class LayerSearch : MonoBehaviour
{
    // ---------------------------- Field
    private float _range;                 // 探索範囲
    private LayerMask _layerMask;       // 検出対象のレイヤー名

    private GameObject _nearestTargetObj; // 最も近い対象オブジェクト
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

        // 指定レイヤー内で、一定範囲内にあるコライダーを取得
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            _range,
            _layerMask);

        foreach (var hit in hits)
        {
            if (hit == null) continue;

            GameObject enemyRoot = hit.transform.root.gameObject;

            // リストに未追加なら追加
            if (!_nearestTargetList.Contains(enemyRoot))
            {
                _nearestTargetList.Add(enemyRoot);
            }

            // 最も近い敵を更新
            float distance = Vector3.Distance(transform.position, hit.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                _nearestTargetObj = hit.gameObject.transform.root.gameObject;
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
