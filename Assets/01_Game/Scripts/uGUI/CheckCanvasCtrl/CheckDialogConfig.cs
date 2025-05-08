using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "CheckDialogConfig", menuName = "UI/Check Dialog Config")]
public class CheckDialogConfig : ScriptableObject
{
    public string message;
    public string yesButtonText = "はい";
    public string noButtonText = "いいえ";
    public CheckActionReference actionReference; // インターフェースを実装したクラスへの参照
}