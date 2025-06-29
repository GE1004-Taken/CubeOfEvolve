using R3;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField] private List<SpawnWaveData> _waves;
    [SerializeField] private float _playerDistance = 10f;

    // ---------------------------- Field
    private GameObject _target;

    // ---------------------------- UnityMessage
    private void Start()
    {
        _target = PlayerMonitoring.Instance.PlayerObj;

        GameManager.Instance.CurrentGameState
            .Where(value => value == Assets.IGC2025.Scripts.GameManagers.GameState.BATTLE)
            .Take(1)
            .Subscribe(_ =>
            {
                // delaySecond の昇順に並び替えている
                _waves.Sort((a, b) => a.delaySecond.CompareTo(b.delaySecond));
                StartCoroutine(SpawnWaveSequence());
            })
            .AddTo(this);
    }

    // ---------------------------- PrivateMethod
    /// <summary>
    /// 複数のSpawnWaveDataを順番に処理
    /// </summary>
    /// <returns></returns>
    private IEnumerator SpawnWaveSequence()
    {
        foreach (var wave in _waves)
        {
            // 現在の経過時間からWave開始までの待機時間を計算
            float waitTime = wave.delaySecond - GameManager.Instance.TimeManager.CurrentTimeSecond.CurrentValue;

            if (waitTime > 0)
            {
                // 指定時間だけ待機してWaveの開始を遅らせる
                yield return new WaitForSeconds(waitTime);
            }

            // イベント
            if (wave.patternType == SpawnWaveData.SpawnPatternType.Event)
            {
                for (int i = 0; i < wave.spawnCount; i++)
                {
                    Spawn(wave.enemyList[Random.Range(0, wave.enemyList.Count)]);
                }
            }
            // ループ
            else if (wave.patternType == SpawnWaveData.SpawnPatternType.Loop)
            {
                // Waveの開始時間
                float startTime = Time.time;
                // Waveの終了時間
                float endTime = startTime + wave.duration;
                // 次にスポーンを行う時間
                float nextSpawnTime = startTime;

                // Waveの終了時間までループ処理を継続
                while (Time.time < endTime)
                {
                    // 次のスポーン予定時間に達したかを判定
                    if (Time.time >= nextSpawnTime)
                    {
                        // 指定された数だけ敵を生成
                        for (int i = 0; i < wave.spawnCount; i++)
                        {
                            Spawn(wave.enemyList[Random.Range(0, wave.enemyList.Count)]);
                        }

                        // 次のスポーン時間を更新
                        nextSpawnTime += wave.interval;
                    }

                    // 毎フレーム1回ループを継続、負荷軽減のため WaitForSeconds は使わない
                    yield return null;
                }
            }
        }
    }


    /// <summary>
    /// 生成処理
    /// </summary>
    /// <param name="enemyPrefab">生成する敵のプレハブ</param>
    private void Spawn(GameObject enemyPrefab)
    {
        // 0～360度のランダムな角度を生成
        float spawnAngle = Random.Range(0, 360);

        // 角度をラジアンに変換
        float radians = spawnAngle * Mathf.Deg2Rad;

        // ラジアンからXZ平面の方向ベクトルを計算（Yは0に固定）
        Vector3 direction = new Vector3(Mathf.Sin(radians), 0, Mathf.Cos(radians)).normalized;

        // プレイヤー位置から指定距離の方向へオフセットして生成位置を決定
        Vector3 spawnPos = _target.transform.position + direction * _playerDistance;

        // 敵オブジェクトを生成し、位置と回転を設定
        var enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        // 生成後に敵の初期化メソッドを呼び出す（EnemyStatusコンポーネントがあれば）
        enemy.GetComponent<EnemyStatus>()?.EnemySpawn();
    }
}
