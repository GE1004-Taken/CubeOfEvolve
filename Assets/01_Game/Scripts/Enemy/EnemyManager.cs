using R3;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    // シングルトン
    public static EnemyManager Instance;

    // ---------------------------- SerializeField
    [Header("プレイヤー")]
    [SerializeField, Tooltip("プレイヤー")] private GameObject _playerObj;


    // ---------------------------- Field
    private List<EnemyStatus> _enemyList = new();


    // ---------------------------- Property
    public GameObject PlayerObj { get { return _playerObj; } }


    // ---------------------------- UnityMessage
    private void Awake()
    {
        // シングルトン
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // 子オブジェクトを保存
        for (int i = 0; i < transform.childCount; i++)
        {
            var enemyObj = transform.GetChild(i).gameObject;
            _enemyList.Add(enemyObj.GetComponent<EnemyStatus>());
        }
    }
    private void Start()
    {
        // 死の判定
        foreach (var enemy in _enemyList)
        {
            enemy.Hp
                .Where(value => value <= 0)
                .Subscribe(value =>
                {
                    Destroy(enemy.gameObject);

                    _enemyList.Remove(enemy);
                })
                .AddTo(enemy.gameObject);
        }
    }
}
