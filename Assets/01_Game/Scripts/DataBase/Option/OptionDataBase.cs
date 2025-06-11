using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/DataBase/OptionDataBase")]
public class OptionDataBase : ScriptableObject
{
    public List<StatusEffectData> Data;
}
