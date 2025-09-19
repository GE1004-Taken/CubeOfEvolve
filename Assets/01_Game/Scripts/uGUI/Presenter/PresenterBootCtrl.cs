using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.IGC2025.Scripts.Presenter
{
    public class PresenterBootCtrl : MonoBehaviour
    {
        [Tooltip("初期化対象のPresenter(MonoBehaviour)。IPresenterを実装している必要があります。")]
        [SerializeField] private MonoBehaviour[] presenterBehaviours;

        [Tooltip("モデル初期化の完了を待つためのフレーム待機数")]
        [SerializeField, Min(0)] private int waitFrames = 10;

        private readonly List<IPresenter> presenters = new();

        private void Awake()
        {
            presenters.Clear();
            if (presenterBehaviours == null) return;

            foreach (var mb in presenterBehaviours)
            {
                if (mb == null) continue;

                if (mb is IPresenter p)
                {
                    presenters.Add(p);
                }
                else
                {
                    Debug.LogError($"{name}: {mb.GetType().Name} は IPresenter を実装していません。");
                }
            }
        }

        private void Start()
        {
            StartCoroutine(BootSequence());
        }

        private IEnumerator BootSequence()
        {
            for (int i = 0; i < waitFrames; i++)
                yield return null;

            // presentersをまとめて初期化
            foreach (var p in presenters)
            {
                if (p == null) continue;

                if (!p.IsInitialized)
                {
                    p.Initialize();
#if UNITY_EDITOR
                    Debug.Log($"{name}: Initialized {p.GetType().Name}", this);
#endif
                }
            }
        }
    }
}