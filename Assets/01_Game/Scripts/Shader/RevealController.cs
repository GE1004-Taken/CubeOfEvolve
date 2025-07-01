using UnityEngine;

public class RevealController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Renderer[] targetRenderer;

    void Update()
    {
        if (targetRenderer != null && player != null)
        {
            Vector3 pos = player.position;

            foreach (Renderer r in targetRenderer)
            {
                r.material.SetVector("_PlayerPosition", pos);
            }
        }
    }
}
