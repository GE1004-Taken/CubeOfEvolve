using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemDrop : MonoBehaviour
{
    [Serializable]
    private class DropItem
    {
        public ItemData data;
        public int value;
    }

    [Serializable]
    private class RareDrop
    {
        public ItemData data;
        public int value;
        public float rate;
    }
    // ---------------------------- SerializeField
    [Header("ドロップするもの")]
    [SerializeField, Tooltip("")] private DropItem[] _dropItemList;

    [Header("ドロップするもの")]
    [SerializeField, Tooltip("")] private RareDrop[] _rareDropList;

    [Header("吹き飛ぶ力")]
    [SerializeField, Tooltip("上")] private float _forceHeightPower;
    [SerializeField, Tooltip("横")] private float _forceHorizontalPower;

    // ---------------------------- PublicMethod
    /// <summary>
    /// アイテムを落とす処理
    /// </summary>
    public void DropItemProcess()
    {
        Debug.Log("アイテムを落とす処理");
        // 通常のアイテム
        foreach (var dropItem in _dropItemList)
        {
            for (int i = 0; i < dropItem.value; i++)
            {
                DropAnimation(dropItem.data.Item);
            }
        }

        // レアドロップ
        foreach (var dropItem in _rareDropList)
        {
            var random = Random.Range(0f, 1f) * 100;

            if (random <= dropItem.rate)
            {
                Debug.Log("レアドロップ");

                for (int i = 0; i < dropItem.value; i++)
                {
                    DropAnimation(dropItem.data.Item);
                }
            }
        }
    }

    // ---------------------------- PrivateMethod
    /// <summary>
    /// アイテムを生成して飛ばす処理
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="dropObj"></param>
    private void DropAnimation(ItemBase dropObj)
    {
        var obj = Instantiate(dropObj);
        obj.transform.position = transform.position;

        if (obj.GetComponent<Rigidbody>() == null)
            obj.gameObject.AddComponent<Rigidbody>();

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
