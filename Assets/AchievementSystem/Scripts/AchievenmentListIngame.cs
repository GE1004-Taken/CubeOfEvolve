using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// アチーブメントリストを画面に追加します。
/// </summary>
public class AchievenmentListIngame : MonoBehaviour
{
    [SerializeField] private GameObject scrollContent;
    [SerializeField] private GameObject prefab;
    [SerializeField] private GameObject Menu;
    [SerializeField] private Dropdown Filter;
    [SerializeField] private TextMeshProUGUI CountText;
    [SerializeField] private TextMeshProUGUI CompleteText;
    [SerializeField] private Scrollbar Scrollbar;

    private bool MenuOpen = false;
    [Tooltip("UIメニューを開くために使用するキー。キー入力を受け付けないようにするには「None」に設定します。")]
    public KeyCode OpenMenuKey; // ゲーム内メニューを開くキー

    /// <summary>
    /// フィルターに基づいてすべてのアチーブメントをUIに追加します。
    /// </summary>
    /// <param name="Filter">使用するフィルター（すべて、達成済み、未達成）。</param>
    public void AddAchievements(string Filter)
    {
        foreach (Transform child in scrollContent.transform)
        {
            Destroy(child.gameObject);
        }
        AchievementManager AM = AchievementManager.instance;
        int AchievedCount = AM.GetAchievedCount();

        CountText.text = "" + AchievedCount + " / " + AM.States.Count;
        CompleteText.text = "コンプリート (" + Mathf.RoundToInt(AM.GetAchievedPercentage()) + "%)";

        for (int i = 0; i < AM.AchievementList.Count; i++)
        {
            if ((Filter.Equals("すべて")) || (Filter.Equals("たっせいずみ") && AM.States[i].Achieved) || (Filter.Equals("みたっせい") && !AM.States[i].Achieved))
            {
                AddAchievementToUI(AM.AchievementList[i], AM.States[i]);
            }
        }
        Scrollbar.value = 1;
    }

    public void AddAchievementToUI(AchievementInfromation Achievement, AchievementState State)
    {
        UIAchievement UIAchievement = Instantiate(prefab, new Vector3(0f, 0f, 0f), Quaternion.identity).GetComponent<UIAchievement>();
        UIAchievement.Set(Achievement, State);
        UIAchievement.transform.SetParent(scrollContent.transform);
    }

    /// <summary>
    /// ロックされたアチーブメントまたはアンロックされたアチーブメントのセットをフィルタリングします。
    /// </summary>
    public void ChangeFilter()
    {
        AddAchievements(Filter.options[Filter.value].text);
    }

    /// <summary>
    /// UIウィンドウを閉じます。
    /// </summary>
    public void CloseWindow()
    {
        MenuOpen = false;
        Menu.SetActive(MenuOpen);
    }

    /// <summary>
    /// UIウィンドウを開きます。
    /// </summary>
    public void OpenWindow()
    {
        MenuOpen = true;
        Menu.SetActive(MenuOpen);
        AddAchievements("すべて");
    }

    /// <summary>
    /// UIウィンドウの状態（開いているか閉じているか）を切り替えます。
    /// </summary>
    public void ToggleWindow()
    {
        if (MenuOpen)
        {
            CloseWindow();
        }
        else
        {
            OpenWindow();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(OpenMenuKey))
        {
            ToggleWindow();
        }
    }
}