using System.Collections;
using UnityEngine;

namespace AT.uGUI.TEST
{
    public sealed class Test_View : MonoBehaviour
    {
        [SerializeField] PlayerCore _playerCore;
        private bool _isPossible = true;

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) && _isPossible)
            {
                _playerCore.TakeDamage(10f);
                
                _playerCore.ReceiveExp(50);
                Debug.Log($"Exp:{_playerCore.Exp}");
                _playerCore.ReceiveMoney(100);

                _isPossible = false;
                StartCoroutine(ResetPossible());
            }
        }

        private IEnumerator ResetPossible()
        {
            yield return new WaitForSeconds(1f);
            _isPossible = true;
        }
    }
}


