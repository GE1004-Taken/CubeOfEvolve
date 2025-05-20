using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemDrop : MonoBehaviour
{
    [Serializable]
    private class DropItem
    {
        public int value;
        public GameObject obj;
    }
    // ---------------------------- SerializeField
    [Header("ドロップするもの")]
    [SerializeField, Tooltip("経験値")] private DropItem _expItem;
    [SerializeField, Tooltip("お金")] private DropItem _moneyItem;

    [Header("吹き飛ぶ力")]
    [SerializeField, Tooltip("上")] private float _forceHeightPower;
    [SerializeField, Tooltip("横")] private float _forceHorizontalPower;

    // ---------------------------- PublicMethod
    /// <summary>
    /// 経験値を落とす処理
    /// </summary>
    public void DropExp()
    {
        for (int i = 0; i < _expItem.value; i++)
        {
            DropAnimation(_expItem.obj);
        }
    }

    /// <summary>
    /// お金を落とす距離
    /// </summary>
    public void DropMoney()
    {
        for (int i = 0; i < _moneyItem.value; i++)
        {
            DropAnimation(_moneyItem.obj);
        }
    }

    // ---------------------------- PrivateMethod
    /// <summary>
    /// アイテムを生成して飛ばす処理
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="dropObj"></param>
    private void DropAnimation(GameObject dropObj)
    {
        GameObject obj = Instantiate(dropObj);
        obj.transform.position = transform.position;

        if (obj.GetComponent<Rigidbody>() == null)
            obj.AddComponent<Rigidbody>();

        Rigidbody rb = obj.GetComponent<Rigidbody>();

        // 360度から抽選
        float spawnAngle = Random.Range(0, 8) * 45;
        // ラジアン角に変更
        float radians = spawnAngle * Mathf.Deg2Rad;
        // 方向
        Vector3 direction = new Vector3(Mathf.Sin(radians), _forceHeightPower, Mathf.Cos(radians));

        // 飛ばす
        rb.AddForce(_forceHorizontalPower * direction, ForceMode.Impulse);
    }
}
