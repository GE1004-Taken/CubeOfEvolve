using System.Collections.Generic;
using UnityEngine;

public class EnemyCollective : MonoBehaviour
{
    // ---------------------------- Field
    private List<EnemyStatus> _enemyList = new();

    // ---------------------------- UnityMessage
    private void Start()
    {
        var targetObj = PlayerMonitoring.Instance.PlayerObj;

        // 敵から対象へのベクトルを取得
        var moveForward = targetObj.transform.position - transform.position;

        // 高さは追わない
        moveForward.y = 0;

        // キャラクターの向きを進行方向に向ける
        if (moveForward != Vector3.zero)
        {
            // 方向ベクトルを取得
            Vector3 direction = targetObj.transform.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

            // Y軸の回転のみ取得
            transform.rotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
        }


        // 自身の子オブジェクトをスキャンしてリストに追加
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            var status = child.GetComponent<EnemyStatus>();

            // 有効な EnemyStatus をリストに追加
            if (status != null && !_enemyList.Contains(status))
            {
                _enemyList.Add(status);
            }
        }

        // 各敵を出現・親子関係を解除
        foreach (EnemyStatus status in _enemyList)
        {
            // 敵の初期化処理
            status.EnemySpawn();

            // 敵をこのオブジェクトから外す
            status.transform.SetParent(null);
        }
    }
}
