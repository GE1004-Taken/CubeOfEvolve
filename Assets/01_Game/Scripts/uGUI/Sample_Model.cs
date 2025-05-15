using R3;
using UnityEngine;

namespace MVRP.Sample
{
    public sealed class Sample_Model : MonoBehaviour
    {
        /// <summary>
        /// 体力
        /// ReactivePropertyとして外部に状態をReadOnlyで公開
        /// </summary>
        public ReadOnlyReactiveProperty<int> Health => _health;
        // 体力の最大値
        public readonly int MaxHealth = 100;

        private readonly ReactiveProperty<int> _health = new ReactiveProperty<int>(100);

        /// <summary>
        /// 衝突イベント
        /// </summary>
        private void OnCollisionEnter(Collision collision)
        {
            _health.Value -= 10;

            // Enemyに触れたら体力を減らす
            //if (collision.gameObject.TryGetComponent<Enemy>(out var _))
            //{
            //    _health.Value -= 10;
            //}
        }

        private void OnDestroy()
        {
            _health.Dispose();
        }
    }
}
