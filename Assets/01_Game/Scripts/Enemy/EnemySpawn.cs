using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    [Serializable]
    private class Wave
    {
        public float delaySecond;
        public List<GameObject> enemyList = new();
    }

    // ---------------------------- SerializeField
    [Header("ウェーブ"), SerializeField] private List<Wave> _waveList = new();

    [Header("初期の高さ"), SerializeField] private float _startHeight;
    [Header("生成するb高さ"), SerializeField] private float _spawnHeight;


    // ---------------------------- Field
    //private List<EnemyStatus> _enemyStatusList = new();

    private int _nextWave;

    private Coroutine _coroutine;
    private float _currentDelay;

    // ---------------------------- Property
    private void Awake()
    {
        // 登録された敵のステータスを保存
        foreach (var wave in _waveList)
        {
            foreach (var enemy in wave.enemyList)
            {
                enemy.transform.position
                = new Vector3(enemy.transform.position.x, _startHeight, enemy.transform.position.z);
                //_enemyStatusList.Add(enemy.GetComponent<EnemyStatus>());
            }
        }

        _nextWave = 0;

        // 次のウェーブの待ち時間
        _currentDelay = _waveList[_nextWave].delaySecond;

        // ウェーブを進める処理
        _coroutine = StartCoroutine(SpawnCoroutine());
    }

    // ---------------------------- UnityMessage


    // ---------------------------- PublicMethod


    // ---------------------------- PrivateMethod
    /// <summary>
    /// 敵を生成する処理
    /// </summary>
    private void EnemySpawnProcess()
    {
        foreach (var enemy in _waveList[_nextWave].enemyList)
        {
            enemy.transform.position
                = new Vector3(enemy.transform.position.x, _spawnHeight, enemy.transform.position.z);

            enemy.GetComponent<EnemyStatus>().EnemySpawn();
        }

        _nextWave++;

        if (_nextWave > _waveList.Count) return;

        // 次のウェーブの待ち時間
        _currentDelay = _waveList[_nextWave].delaySecond;
    }

    private IEnumerator SpawnCoroutine()
    {
        while (_nextWave < _waveList.Count)
        {
            yield return new WaitForSeconds(_currentDelay);

            // 敵を生成する処理
            EnemySpawnProcess();
        }

        yield break;
    }
}
