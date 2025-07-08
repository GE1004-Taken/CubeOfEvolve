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

                // 全Waveを開始
                foreach (var wave in _waves)
                {
                    StartCoroutine(StartWave(wave));
                }
            })
            .AddTo(this);
    }

    // ---------------------------- PrivateMethod
    /// <summary>
    /// ウェーブを開始する
    /// </summary>
    /// <param name="wave"></param>
    /// <returns></returns>
    private IEnumerator StartWave(SpawnWaveData wave)
    {
        float waitTime = wave.delaySecond - GameManager.Instance.TimeManager.CurrentTimeSecond.CurrentValue;

        if (waitTime > 0)
        {
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
            // Waveの開始時刻
            float startTime = Time.time;
            // 開始時点
            float nextSpawnTime = startTime;
            // wave.duration が -1 のときは無限ループ、それ以外は制限時間付きループ
            bool isInfinite = wave.duration < 0;
            float endTime = isInfinite ? float.MaxValue : startTime + wave.duration;

            // 現在時刻が終了時刻を過ぎるまでループ（または無限）
            while (Time.time < endTime)
            {
                // 現在時刻が次のスポーンタイミングになったか
                if (Time.time >= nextSpawnTime)
                {
                    for (int i = 0; i < wave.spawnCount; i++)
                    {
                        Spawn(wave.enemyList[Random.Range(0, wave.enemyList.Count)]);
                    }

                    // 次回のスポーン時間を interval 秒後に設定
                    nextSpawnTime += wave.interval;
                }

                yield return null;
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
