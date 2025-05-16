using Assets.IGC2025.Scripts.GameManagers;
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
        Observable.EveryUpdate()
         .Select(_ => transform.childCount)
         .DistinctUntilChanged()
         .Subscribe(value =>
         {
             _enemyList.Clear();

             for (int i = 0; i < transform.childCount; i++)
             {
                 var enemyObj = transform.GetChild(i).gameObject;
                 _enemyList.Add(enemyObj.GetComponent<EnemyStatus>());
             }
         })
         .AddTo(this);
    }
    private void Start()
    {
        GameManager.Instance.ChangeGameState(GameState.GAME);
    }
}
