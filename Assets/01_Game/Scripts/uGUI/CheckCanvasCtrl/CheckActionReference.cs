using UnityEngine;

[System.Serializable]
public class CheckActionReference
{
    public ScriptableObject actionObject;

    public ICheckAction GetAction()
    {
        return actionObject as ICheckAction;
    }
}