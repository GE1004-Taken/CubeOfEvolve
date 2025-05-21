using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

/// <summary>
/// アチーブメントシステムとのインタラクションを制御します。
/// </summary>
[System.Serializable]
public class AchievementManager : MonoBehaviour
{
    [Tooltip("アチーブメントがアンロックされた後、または進行状況が更新された後、画面に表示される秒数。")]
    public float DisplayTime = 3;
    [Tooltip("一度に画面に表示できるアチーブメントの最大数。")]
    public int NumberOnScreen = 3;
    [Tooltip("trueの場合、進行状況通知に正確な進捗が表示されます。falseの場合、最も近い段階が表示されます。")]
    public bool ShowExactProgress = false;
    [Tooltip("trueの場合、アチーブメントのアンロック/進捗更新通知がプレイヤーの画面に表示されます。")]
    public bool DisplayAchievements;
    [Tooltip("アチーブメント通知を表示する画面上の場所。")]
    public AchievementStackLocation StackLocation;
    [Tooltip("trueの場合、手動保存機能の呼び出しなしに、すべてのアチーブメントの状態が保存されます（推奨 = true）。")]
    public bool AutoSave;
    [Tooltip("アチーブメントがスポイラーとしてマークされている場合にUIに表示されるメッセージ。")]
    public string SpoilerAchievementMessage = "非表示";
    [Tooltip("アチーブメントがアンロックされたときにユーザーに表示されるサウンド。サウンドは「アチーブメントを表示」がtrueの場合にのみ再生されます。")]
    public AudioClip AchievedSound;
    [Tooltip("進捗更新がユーザーに表示されたときに再生されるサウンド。サウンドは「アチーブメントを表示」がtrueの場合にのみ再生されます。")]
    public AudioClip ProgressMadeSound;

    private AudioSource AudioSource;

    [SerializeField] public List<AchievementState> States = new List<AchievementState>(); // アチーブメントの状態のリスト（達成済み、進行状況、最後の通知）。
    [SerializeField] public List<AchievementInfromation> AchievementList = new List<AchievementInfromation>(); // 利用可能なすべてのアチーブメントのリスト。

    [Tooltip("trueの場合、他のすべてが完了すると、1つのアチーブメントが自動的にアンロックされます。")]
    public bool UseFinalAchievement = false;
    [Tooltip("最終アチーブメントのキー。")]
    public string FinalAchievementKey;

    public static AchievementManager instance = null; // シングルトンインスタンス。
    public AchievenmentStack Stack;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
        AudioSource = gameObject.GetComponent<AudioSource>();
        Stack = GetComponentInChildren<AchievenmentStack>();
        LoadAchievementState();
    }

    private void PlaySound(AudioClip Sound)
    {
        if (AudioSource != null)
        {
            AudioSource.clip = Sound;
            AudioSource.Play();
        }
    }

    #region その他

    /// <summary>
    /// アチーブメントがリストに存在するかどうか。
    /// </summary>
    /// <param name="Key">テストするアチーブメントのキー。</param>
    /// <returns>true：存在する場合。false：存在しない場合。</returns>
    public bool AchievementExists(string Key)
    {
        return AchievementExists(AchievementList.FindIndex(x => x.Key.Equals(Key)));
    }

    /// <summary>
    /// アチーブメントがリストに存在するかどうか。
    /// </summary>
    /// <param name="Index">テストするアチーブメントのインデックス。</param>
    /// <returns>true：存在する場合。false：存在しない場合。</returns>
    public bool AchievementExists(int Index)
    {
        return Index <= AchievementList.Count && Index >= 0;
    }

    /// <summary>
    /// アンロックされたアチーブメントの総数を返します。
    /// </summary>
    public int GetAchievedCount()
    {
        int Count = (from AchievementState i in States
                     where i.Achieved == true
                     select i).Count();
        return Count;
    }

    /// <summary>
    /// アンロックされたアチーブメントの現在の割合を返します。
    /// </summary>
    public float GetAchievedPercentage()
    {
        if (States.Count == 0)
        {
            return 0;
        }
        return (float)GetAchievedCount() / States.Count * 100;
    }

    #endregion

    #region アンロックと進捗

    /// <summary>
    /// プログレッションまたはゴールアチーブメントを完全にアンロックします。
    /// </summary>
    /// <param name="Key">アンロックするアチーブメントのキー。</param>
    public void Unlock(string Key)
    {
        Unlock(FindAchievementIndex(Key));
    }

    /// <summary>
    /// プログレッションまたはゴールアチーブメントを完全にアンロックします。
    /// </summary>
    /// <param name="Index">アンロックするアチーブメントのインデックス。</param>
    public void Unlock(int Index)
    {
        if (!States[Index].Achieved)
        {
            States[Index].Progress = AchievementList[Index].ProgressGoal;
            States[Index].Achieved = true;
            DisplayUnlock(Index);
            AutoSaveStates();

            if (UseFinalAchievement)
            {
                int Find = States.FindIndex(x => !x.Achieved);
                bool CompletedAll = (Find == -1 || AchievementList[Find].Key.Equals(FinalAchievementKey));
                if (CompletedAll)
                {
                    Unlock(FinalAchievementKey);
                }
            }
        }
    }

    /// <summary>
    /// アチーブメントの進捗を特定の値に設定します。
    /// </summary>
    /// <param name="Key">アチーブメントのキー。</param>
    /// <param name="Progress">この値に進捗を設定します。</param>
    public void SetAchievementProgress(string Key, float Progress)
    {
        SetAchievementProgress(FindAchievementIndex(Key), Progress);
    }

    /// <summary>
    /// アチーブメントの進捗を特定の値に設定します。
    /// </summary>
    /// <param name="Index">アチーブメントのインデックス。</param>
    /// <param name="Progress">この値に進捗を設定します。</param>
    public void SetAchievementProgress(int Index, float Progress)
    {
        if (AchievementList[Index].Progression)
        {
            if (States[Index].Progress >= AchievementList[Index].ProgressGoal)
            {
                Unlock(Index);
            }
            else
            {
                States[Index].Progress = Progress;
                DisplayUnlock(Index);
                AutoSaveStates();
            }
        }
    }

    /// <summary>
    /// 入力された進捗量をアチーブメントに追加します。アチーブメントの進捗は最大値にクランプされます。
    /// </summary>
    /// <param name="Key">アチーブメントのキー。</param>
    /// <param name="Progress">この数値を進捗に追加します。</param>
    public void AddAchievementProgress(string Key, float Progress)
    {
        AddAchievementProgress(FindAchievementIndex(Key), Progress);
    }

    /// <summary>
    /// 入力された進捗量をアチーブメントに追加します。アチーブメントの進捗は最大値にクランプされます。
    /// </summary>
    /// <param name="Index">アチーブメントのインデックス。</param>
    /// <param name="Progress">この数値を進捗に追加します。</param>
    public void AddAchievementProgress(int Index, float Progress)
    {
        if (AchievementList[Index].Progression)
        {
            if (States[Index].Progress + Progress >= AchievementList[Index].ProgressGoal)
            {
                Unlock(Index);
            }
            else
            {
                States[Index].Progress += Progress;
                DisplayUnlock(Index);
                AutoSaveStates();
            }
        }
    }

    #endregion

    #region 保存と読み込み

    /// <summary>
    /// 進捗と達成状態をPlayerPrefsに保存します。ゲームのロード間でデータをリロードするために使用されます。この関数は、自動保存設定がtrueに設定されている場合に自動的に呼び出されます。
    /// </summary>
    public void SaveAchievementState()
    {
        for (int i = 0; i < States.Count; i++)
        {
            PlayerPrefs.SetString("AchievementState_" + i, JsonUtility.ToJson(States[i]));
        }
        PlayerPrefs.Save();
    }

    /// <summary>
    /// すべての進捗とアチーブメントの状態をPlayerPrefsから読み込みます。この関数は、自動読み込み設定がtrueに設定されている場合に自動的に呼び出されます。
    /// </summary>
    public void LoadAchievementState()
    {
        AchievementState NewState;
        States.Clear();

        for (int i = 0; i < AchievementList.Count; i++)
        {
            // 新しいプロジェクトがデフォルト値を取得するようにします。
            if (PlayerPrefs.HasKey("AchievementState_" + i))
            {
                NewState = JsonUtility.FromJson<AchievementState>(PlayerPrefs.GetString("AchievementState_" + i));
                States.Add(NewState);
            }
            else
            {
                States.Add(new AchievementState());
            }
        }
    }

    /// <summary>
    /// 保存されたすべての進捗と達成状態をクリアします。
    /// </summary>
    public void ResetAchievementState()
    {
        States.Clear();
        for (int i = 0; i < AchievementList.Count; i++)
        {
            PlayerPrefs.DeleteKey("AchievementState_" + i);
            States.Add(new AchievementState());
        }
        SaveAchievementState();
    }

    #endregion

    /// <summary>
    /// 特定のキーを持つアチーブメントのインデックスを検索します。
    /// </summary>
    /// <param name="Key">アチーブメントのキー。</param>
    private int FindAchievementIndex(string Key)
    {
        return AchievementList.FindIndex(x => x.Key.Equals(Key));
    }

    /// <summary>
    /// AutoSaveが有効かどうかをテストします。trueの場合、リストを保存します。
    /// </summary>
    private void AutoSaveStates()
    {
        if (AutoSave)
        {
            SaveAchievementState();
        }
    }

    /// <summary>
    /// アチーブメントの進捗を画面に表示します。
    /// </summary>
    /// <param name="Index">表示するアチーブメントのインデックス。</param>
    private void DisplayUnlock(int Index)
    {
        if (DisplayAchievements && !AchievementList[Index].Spoiler || States[Index].Achieved)
        {
            // 未達成の場合
            if (AchievementList[Index].Progression && States[Index].Progress < AchievementList[Index].ProgressGoal)
            {
                int Steps = (int)AchievementList[Index].ProgressGoal / (int)AchievementList[Index].NotificationFrequency;

                // 最後の可能なオプションから逆方向にすべての通知ポイントをループします。
                for (int i = Steps; i > States[Index].LastProgressUpdate; i--)
                {
                    // 最大の有効な通知ポイントが見つかったとき
                    if (States[Index].Progress >= AchievementList[Index].NotificationFrequency * i)
                    {
                        PlaySound(ProgressMadeSound);
                        States[Index].LastProgressUpdate = i;
                        Stack.ScheduleAchievementDisplay(Index);
                        return;
                    }
                }
            }
            else
            {
                PlaySound(AchievedSound);
                Stack.ScheduleAchievementDisplay(Index);
            }
        }
    }
}