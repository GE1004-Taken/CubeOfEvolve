using UnityEngine;

[System.Serializable]
public class CheckActionReference
{
    private ScriptableObject actionObject;

    public ICheckAction GetAction()
    {
        return actionObject as ICheckAction;
    }
}