using App.GameSystem.Modules;
using Assets.IGC2025.Scripts.GameManagers;
using Game.Utils;
using ObservableCollections;
using R3;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField] private List<SpawnWaveData> _waves;
    [SerializeField] private float _playerDistance = 10f;
    [SerializeField] private GameObject _spawnEffect;

    // ---------------------------- Field
    private GameObject _target;
    private float _currentSpawnRate = 1f;

    // ---------------------------- UnityMessage

    private void Start()
    {
        _target = PlayerMonitoring.Instance.PlayerObj;

        _waves.Sort((a, b) => a.delaySecond.CompareTo(b.delaySecond));

        ObserveStatusEffects();

        GameManager.Instance.CurrentGameState
            .Where(value => value == GameState.BATTLE)
            .Take(1)
            .Subscribe(_ =>
            {
                // 状態がBATTLEのときのみWave開始
                foreach (var wave in _waves)
                {
                    StartCoroutine(StartWave(wave));
                }
            })
            .AddTo(this);
    }

    // ---------------------------- PrivateMethod
    /// <summary>
    /// オプションを監視
    /// </summary>
    private void ObserveStatusEffects()
    {
        var addStream = RuntimeModuleManager.Instance.CurrentCurrentStatusEffectList
        .ObserveAdd(destroyCancellationToken)
        .Select(_ => Unit.Default);

        var removeStream = RuntimeModuleManager.Instance.CurrentCurrentStatusEffectList
            .ObserveRemove(destroyCancellationToken)
            .Select(_ => Unit.Default);

        // どちらかのイベントが発生した時を監視
        addStream.Merge(removeStream)
            .Subscribe(_ =>
            {
                UpdateSpawnRate();
            })
            .AddTo(this);
    }

    /// <summary>
    /// 湧き率更新
    /// </summary>
    private void UpdateSpawnRate()
    {
        var spawnRate = 0f;

        foreach (var effect in RuntimeModuleManager.Instance.CurrentCurrentStatusEffectList)
        {
            spawnRate += effect.SpawnRate;
        }

        _currentSpawnRate = 1f + (spawnRate / 100f);
    }

    /// <summary>
    /// ウェーブを開始する
    /// </summary>
    private IEnumerator StartWave(SpawnWaveData wave)
    {
        // バトル中のみカウントする遅延処理
        float elapsed = 0f;
        while (elapsed < wave.delaySecond)
        {
            if (GameManager.Instance.CurrentGameState.CurrentValue == GameState.BATTLE)
            {
                elapsed += Time.deltaTime;
            }
            yield return null;
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
            float loopElapsed = 0f;
            float nextSpawnTime = wave.interval;
            bool isInfinite = wave.duration < 0f;

            while (isInfinite || loopElapsed < wave.duration)
            {
                if (GameManager.Instance.CurrentGameState.CurrentValue == GameState.BATTLE)
                {
                    // 出現タイミングに達したか
                    if (loopElapsed >= nextSpawnTime)
                    {
                        for (int i = 0; i < wave.spawnCount * _currentSpawnRate; i++)
                        {
                            Spawn(wave.enemyList[Random.Range(0, wave.enemyList.Count)]);
                        }

                        nextSpawnTime += wave.interval;
                    }

                    loopElapsed += Time.deltaTime;
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

        if (_spawnEffect != null)
        {
            var effect = Instantiate(_spawnEffect, enemy.transform.position, Quaternion.identity);
            effect.AddComponent<StopEffect>();
        }
    }
}
