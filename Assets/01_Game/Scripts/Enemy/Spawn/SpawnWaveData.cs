using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnWave", menuName = "ScriptableObjects/Data/SpawnWave")]
public class SpawnWaveData : ScriptableObject
{
    public enum SpawnPatternType
    {
        [InspectorName("一度だけ生成")]
        Event,
        [InspectorName("ループ")]
        Loop,
    }

    public float delaySecond;               // 開始時間
    public float duration;                  // 持続時間
    public int spawnCount;                  // 生成数
    public List<GameObject> enemyList;      // 敵の種類
    public SpawnPatternType patternType;    // 生成方法
    public float interval;                  // ループ用(生成間隔)
}
