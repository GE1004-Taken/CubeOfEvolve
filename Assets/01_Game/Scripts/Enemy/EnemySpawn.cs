using NUnit.Framework;
using R3;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    // ---------------------------- SerializeField
    [Header("¶¬‚·‚é“G"),SerializeField] private List<EnemyMove> _enemyList = new();

    [Header("‰Šú‚‚³"), SerializeField] private float _startHeight;


    // ---------------------------- Field


    // ---------------------------- Property
    private void Awake()
    {
        foreach(EnemyMove enemy in _enemyList)
        {

        }
    }

    // ---------------------------- UnityMessage


    // ---------------------------- PublicMethod


    // ---------------------------- PrivateMethod
}
