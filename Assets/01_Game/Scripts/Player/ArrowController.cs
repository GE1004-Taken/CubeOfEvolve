using R3;
using R3.Triggers;
using UnityEngine;
using Assets.IGC2025.Scripts.GameManagers;

public class ArrowController : MonoBehaviour
{
    // ---------- SerializeField
    [SerializeField] private GameObject _arrow;
    [SerializeField] private PlayerBuilder _builder;
    [SerializeField, Tooltip("ˆÊ’u—Ê")] private float _moveAmount = 1f;

    // ---------- Field
    private float _initialPosZ;

    // ---------- UnityMessage
    private void Start()
    {
        _initialPosZ = _arrow.transform.localPosition.z;

        GameManager.Instance.CurrentGameState
            .Subscribe(x =>
            {
                if(x == GameState.BUILD)
                {
                    _arrow.SetActive(true);
                }
                else if(x == GameState.BATTLE)
                {
                    _arrow.SetActive(false);
                }
            })
            .AddTo(this);

        _builder.OnCreate
            .Subscribe(_ =>
            {
                var ray = new Ray(
                    _arrow.transform.position,
                    -_arrow.transform.forward);

                var dist = Vector3.Distance(
                    _arrow.transform.position,
                    this.transform.position);

                var hits = Physics.RaycastAll(
                    ray.origin,
                    ray.direction,
                    dist,
                    LayerMask.GetMask("Player"));

                _arrow.transform.localPosition = new Vector3(
                    _arrow.transform.localPosition.x,
                    _arrow.transform.localPosition.y,
                    _initialPosZ + _moveAmount * (hits.Length - 1));
            })
            .AddTo(this);
    }
}
