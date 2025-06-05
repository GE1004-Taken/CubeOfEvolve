using UnityEngine;

[System.Serializable]
public class CheckActionReference
{
    [SerializeField]private ScriptableObject actionObject;

    public ICheckAction GetAction()
    {
        return actionObject as ICheckAction;
    }
}