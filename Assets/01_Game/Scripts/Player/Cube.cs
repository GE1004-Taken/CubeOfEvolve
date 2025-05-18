using R3;
using UnityEngine;

public class Cube : MonoBehaviour
{
    // ---------- Field
    [SerializeField] private Vector3 _index;

    // ---------- Property
    public Vector3 Index
    {
        get => _index;
        set => _index = value;
    }
}
