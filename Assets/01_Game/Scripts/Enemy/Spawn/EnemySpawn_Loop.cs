using R3;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawn_Loop : EnemySpawnBase
{
    // ---------------------------- SerializeField
    [Header("生成間隔"), SerializeField] private float _spawnInterval;

    [Header("間隔加速度"), SerializeField] private float _intervalAccelerate;

    // ---------------------------- OverrideMethod
    public override void StartMethod()
    {
        foreach (var spawnType in _spawnType)
        {
            GameManager.Instance.TimeManager.CurrentTimeSecond
                .Where(value => spawnType.delaySecond <= value)
                .Take(1)
                .Subscribe(value =>
                {
                    _currentSpawnType = spawnType;
                })
                .AddTo(this);
        }

        // ウェーブを進める処理
        StartCoroutine(SpawnCoroutine());
    }

    // ---------------------------- PrivateMethod
    /// <summary>
    /// 生成コルーチン
    /// </summary>
    /// <returns></returns>
    private IEnumerator SpawnCoroutine()
    {
        while (true)
        {
            // 生成間隔 × 割合(経過時間 )
            yield return new WaitForSeconds(_spawnInterval);

            // 敵を生成する処理
            for (int i = 0; i < _currentSpawnType.spawnValue; i++)
            {
                int num = Random.Range(0, _currentSpawnType.enemyList.Count);
                Spawn(_currentSpawnType.enemyList[num]);
            }
        }
    }
}
