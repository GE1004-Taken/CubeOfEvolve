using R3;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class EnemySpawnBase : MonoBehaviour
{
    [Serializable]
    public class SpawnType
    {
        [Header("生成したい時間")]
        public float delaySecond;

        [Header("生成数")]
        public int spawnValue;

        [Header("生成したい敵の種類")]
        public List<GameObject> enemyList = new();
    }

    // ---------------------------- SerializeField
    [Header("敵"), SerializeField] protected SpawnType[] _spawnType;

    [Header("生成距離"), SerializeField] private float _playerDistance;


    // ---------------------------- Field
    protected SpawnType _currentSpawnType = new();
    private GameObject _targetObj;                  // 攻撃対象

    // ---------------------------- UnityMessage
    private void Start()
    {
        _targetObj = PlayerMonitoring.Instance.PlayerObj;

        GameManager.Instance.CurrentGameState
            .Where(value => value == Assets.IGC2025.Scripts.GameManagers.GameState.BATTLE)
            .Take(1)
            .Subscribe(_ => StartMethod())
            .AddTo(this);
    }

    // ---------------------------- AbstractMethod
    public abstract void StartMethod();

    // ---------------------------- ProtectedMethod
    /// <summary>
    /// 生成処理
    /// </summary>
    protected void Spawn(GameObject enemyObj)
    {
        // 360度から抽選
        float spawnAngle = Random.Range(0, 360);
        // ラジアン角に変更
        float radians = spawnAngle * Mathf.Deg2Rad;
        // 方向
        Vector3 direction = new Vector3(Mathf.Sin(radians), 0, Mathf.Cos(radians)).normalized;
        // ターゲットを視点に生成位置を決定
        Vector3 spawnPos = _targetObj.transform.position + direction * _playerDistance;

        // 敵を生成
        var obj = Instantiate(enemyObj);
        obj.transform.position = spawnPos;

        // 敵のステータスの初期設定処理
        obj.GetComponent<EnemyStatus>().EnemySpawn();
    }
}
