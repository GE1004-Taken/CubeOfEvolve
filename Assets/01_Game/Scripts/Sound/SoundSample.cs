using UnityEngine;
using System.Collections;

namespace Assets.AT
{
    public class SoundSample : MonoBehaviour
    {
        [SerializeField] private GameObject _soundSourceObj;

        private SoundManager SM = SoundManager.Instance;
        private bool _isPlay = true;


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R) && _isPlay) // 音鳴らす
            {
                SM.Play("SampleSE", "SE");
                _isPlay = false;
                StartCoroutine(ResetLogFlag());
            }

            if (Input.GetKeyDown(KeyCode.F) && _isPlay) // ブッピガン！ 指定箇所から
            {
                SM.PlaySFXAt("SampleSE", _soundSourceObj, "SE");
                _isPlay = false;
                StartCoroutine(ResetLogFlag());
            }

            if (Input.GetKeyDown(KeyCode.T) && _isPlay) // bgm消すフェード
            {
                SM.StopBGMWithFade(1f);
                _isPlay = false;
                StartCoroutine(ResetLogFlag());
            }

            if (Input.GetKeyDown(KeyCode.G) && _isPlay) // bgm鳴らす
            {
                SM.PlayBGM("SampleBGM", "BGM", 3f);
                _isPlay = false;
                StartCoroutine(ResetLogFlag());
            }
        }

        private void OnDestroy()
        {
            StopCoroutine(ResetLogFlag());
        }

        private IEnumerator ResetLogFlag()
        {
            yield return new WaitForSeconds(0.5f);
            _isPlay = true;
        }
    }
}

