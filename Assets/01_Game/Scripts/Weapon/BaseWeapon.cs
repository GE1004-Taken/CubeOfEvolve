using Assets.IGC2025.Scripts.GameManagers;
using R3;
using R3.Triggers;
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public abstract class BaseWeapon : MonoBehaviour
{
    // ---------- SerializeField
    [SerializeField, Tooltip("UŒ‚—Í")] protected float atk;
    [SerializeField, Tooltip("UŒ‚‘¬“x")] protected float attackSpeed;
    [SerializeField, Tooltip("UŒ‚”ÍˆÍ")] protected float range;
    [SerializeField, Tooltip("UŒ‚ŠÔŠu")] protected float interval;
    [SerializeField, Tooltip("‘ÎÛŒŸ’m—p")] protected SphereCollider sphereCollider;

    // ---------- Field
    protected float currentInterval;
    protected List<Transform> inRangeEnemies = new();
    protected Transform nearestEnemyTransform;

    // ---------- UnityMethod
    private void Start()
    {
        sphereCollider.radius = range;

        this.OnTriggerEnterAsObservable()
            .Where(x => x.CompareTag("Enemy"))
            .Subscribe(x =>
            {
                inRangeEnemies.Add(x.transform);
            });

        this.OnTriggerExitAsObservable()
            .Where(x => x.CompareTag("Enemy"))
            .Subscribe(x =>
            {
                inRangeEnemies[inRangeEnemies.IndexOf(x.transform)] = null;
            });


        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                var nearestEnemyDist = 0f;

                // ˆê”Ô‹ß‚¢“G‚ğæ“¾
                foreach (var enemyTransform in inRangeEnemies)
                {
                    if (enemyTransform == null) continue;

                    var dist = Vector3.Distance(
                        transform.position,
                        enemyTransform.position);

                    if (nearestEnemyDist == 0f || dist < nearestEnemyDist)
                    {
                        nearestEnemyDist = dist;
                        nearestEnemyTransform = enemyTransform;
                    }
                }

                // ”ÍˆÍŠO(null)‚É‚È‚Á‚½—v‘f‚ğÁ‚·
                if (inRangeEnemies.Count > 0)
                {
                    inRangeEnemies.RemoveAll(x => x == null);
                }

                // ƒCƒ“ƒ^[ƒoƒ‹’†‚È‚ç
                if (currentInterval < interval)
                {
                    currentInterval += Time.deltaTime;
                }
                // ƒCƒ“ƒ^[ƒoƒ‹I—¹‚©‚Â“G‚ª‚¢‚½‚ç
                else
                {
                    if (inRangeEnemies.Count <= 0) return;

                    Debug.Log("ˆê”Ô‹ß‚¢“G" + nearestEnemyTransform.gameObject.name);

                    Attack();
                    currentInterval = 0f;
                }
            });
    }

    // ---------- AbstractMethod
    protected abstract void Attack();
}
