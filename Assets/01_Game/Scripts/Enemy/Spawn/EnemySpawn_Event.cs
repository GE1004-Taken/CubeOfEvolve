using R3;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn_Event : EnemySpawnBase
{
    // ---------------------------- Field
    private List<GameObject> _currentSpawnObjList = new();

    // ---------------------------- OverrideMethod
    public override void StartMethod()
    {
        foreach (var spawnType in _spawnType)
        {
            _timeManager.CurrentTimeSecond
                .Where(value => spawnType.delaySecond <= value)
                .Take(1)
                .Subscribe(value =>
                {
                    _currentSpawnObjList = spawnType.enemyList;

                    // “G‚ğ¶¬‚·‚éˆ—
                    int num = Random.Range(0, _currentSpawnObjList.Count);

                    Spawn(_currentSpawnObjList[num]);
                })
                .AddTo(this);
        }
    }
}
