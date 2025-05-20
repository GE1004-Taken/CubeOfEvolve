using UnityEngine;

public class PlayerMonitoring : MonoBehaviour
{
    // シングルトン
    public static PlayerMonitoring Instance;

    // ---------------------------- SerializeField
    [Header("プレイヤー")]
    [SerializeField, Tooltip("プレイヤー")] private GameObject _playerObj;

    // ---------------------------- Property
    public GameObject PlayerObj => _playerObj;


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
    }
}
