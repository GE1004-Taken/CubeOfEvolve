using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 画面上のアチーブメントの表示を制御します。
/// </summary>
public class AchievenmentStack : MonoBehaviour
{
    public RectTransform[] StackPanels;
    public List<UIAchievement> BackLog = new List<UIAchievement>();

    public GameObject AchievementTemplate;
    private AchievementManager AM;

    private void Start()
    {
        AM = AchievementManager.instance;
    }

    /// <summary>
    /// 画面に収まる場合はアチーブメントを画面に追加し、そうでない場合はバックログリストに追加します。
    /// </summary>
    /// <param name="Index">追加するアチーブメントのインデックス。</param>
    public void ScheduleAchievementDisplay(int Index)
    {
        var Spawned = Instantiate(AchievementTemplate).GetComponent<UIAchievement>();
        Spawned.AS = this;
        Spawned.Set(AM.AchievementList[Index], AM.States[Index]);

        // 画面に空きがある場合
        if (GetCurrentStack().childCount < AM.NumberOnScreen)
        {
            Spawned.transform.SetParent(GetCurrentStack(), false);
            Spawned.StartDeathTimer();
        }
        else
        {
            Spawned.gameObject.SetActive(false);
            BackLog.Add(Spawned);
        }
    }

    /// <summary>
    /// アチーブメントを生成する場所のボックスを検索します。
    /// </summary>
    public Transform GetCurrentStack() => StackPanels[(int)AM.StackLocation].transform;

    /// <summary>
    /// バックログから1つのアチーブメントを画面に追加します。
    /// </summary>
    public void CheckBackLog()
    {
        if (BackLog.Count > 0)
        {
            BackLog[0].transform.SetParent(GetCurrentStack(), false);
            BackLog[0].gameObject.SetActive(true);
            BackLog[0].StartDeathTimer();
            BackLog.RemoveAt(0);
        }
    }
}