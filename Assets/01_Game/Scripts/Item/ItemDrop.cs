using UnityEngine;

public class ItemDrop : MonoBehaviour
{
    // シングルトン
    public static ItemDrop Instance;

    // ---------------------------- SerializeField
    [Header("プレイヤー")]
    [SerializeField, Tooltip("プレイヤー")] private GameObject _playerObj;

    [Header("ドロップするもの")]
    [SerializeField, Tooltip("経験値")] private GameObject _expObj;
    [SerializeField, Tooltip("お金")] private GameObject _money;

    [Header("吹き飛ぶ力")]
    [SerializeField, Tooltip("上")] private float _forceHeightPower;
    [SerializeField, Tooltip("横")] private float _forceHorizontalPower;

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

    // ---------------------------- PublicMethod
    public void DropExp(Vector3 pos, int value)
    {
        for (int i = 0; i < value; i++)
        {
            DropAnimation(pos, _expObj);
        }
    }
    public void DropMoney(Vector3 pos, int value)
    {
        for (int i = 0; i < value; i++)
        {
            DropAnimation(pos, _money);
        }
    }

    // ---------------------------- PrivateMethod
    private void DropAnimation(Vector3 pos, GameObject dropObj)
    {
        GameObject obj = Instantiate(dropObj);
        obj.transform.position = pos;

        if (obj.GetComponent<Rigidbody>() == null)
            obj.AddComponent<Rigidbody>();

        Rigidbody rb = obj.GetComponent<Rigidbody>();

        // 360度から抽選
        float spawnAngle = Random.Range(0, 360);
        // ラジアン角に変更
        float radians = spawnAngle * Mathf.Deg2Rad;
        // 方向
        Vector3 direction = new Vector3(Mathf.Sin(radians), _forceHeightPower, Mathf.Cos(radians));

        // 飛ばす
        rb.AddForce(_forceHorizontalPower * direction, ForceMode.Impulse);
    }
}
