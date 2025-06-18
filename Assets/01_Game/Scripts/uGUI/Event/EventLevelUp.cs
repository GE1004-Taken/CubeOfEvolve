// 作成日：   250521
// 更新日：   250521
// 作成者： 安中 健人

// 概要説明(AIにより作成)：

// 使い方説明：
// 専用の挙動をする非汎用的なコード群

using System.Collections;
using UnityEngine;

namespace Assets.IGC2025.Scripts.Event
{
    public class EventLevelUp : MonoBehaviour
    {
        // -----SerializeField
        [SerializeField] private ParticleSystem _particleSystem;
        [SerializeField] private GameObject _LevelupFlame;

        // -----UnityMessage

        private void Start()
        {
            Initialize();
        }

        // -----Public

        /// <summary>
        /// 
        /// </summary>
        public void event_Levelup()
        {
            StartCoroutine(CreateLevelup());
            _particleSystem.Play();
        }

        // -----Private

        /// <summary>
        /// 
        /// </summary>
        private void Initialize()
        {
            if (!_LevelupFlame || !_particleSystem)
            {
                Destroy(this);
                return;
            }

            _LevelupFlame.transform.localScale = Vector3.zero;
            _particleSystem.Stop();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IEnumerator ShowLevelup()
        {
            _LevelupFlame.transform.localScale = Vector3.one;
            yield return new WaitForSeconds(3f);
            _LevelupFlame.transform.localScale = Vector3.zero;
        }

        private IEnumerator CreateLevelup()
        {
            var obj = Instantiate(_LevelupFlame, _LevelupFlame.transform.position, Quaternion.identity, _LevelupFlame.transform.parent);
            obj.transform.localScale = Vector3.one;
            yield return new WaitForSeconds(3f);
            Destroy(obj);
        }
    }
}


