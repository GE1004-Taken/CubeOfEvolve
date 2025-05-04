using UnityEngine;
using System.Collections;

namespace SoundManagerSample.TestPlay
{
    public class SoundSample : MonoBehaviour
    {
        private bool _isPlay = true;

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) && _isPlay)
            {
                SoundManager.Instance.Play("SampleSE", "SE");
                _isPlay = false;
                StartCoroutine(ResetLogFlag());
            }
        }

        private IEnumerator ResetLogFlag()
        {
            yield return new WaitForSeconds(1f);
            _isPlay = true;
        }
    }
}

