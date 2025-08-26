using UnityEngine;
using System.Collections;
using App.GameSystem.Modules;
using App.BaseSystem.DataStores.ScriptableObjects.Modules;

namespace Assets.AT
{
    public class SoundSample : MonoBehaviour
    {
        [SerializeField] private GameObject _soundSourceObj;
        [SerializeField] private ModuleDataStore _moduleDataStore; // モジュールマスターデータを格納するデータストア。

        private GameSoundManager SM;
        private bool _isPlay = true;

        private void Start()
        {
            SM = GameSoundManager.Instance;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R) && _isPlay) // 音鳴らす
            {
                SM.PlaySE("SampleSE", "SE");
                _isPlay = false;
                StartCoroutine(ResetLogFlag());
            }

            if (Input.GetKeyDown(KeyCode.F) && _isPlay) // ブッピガン！ 指定箇所から
            {
                //_soundSourceObj.GetComponent<SFXManagerComponent>().PlaySFX("SampleSE", "SE", false);
                GameSoundManager.Instance.PlaySFX("Hit_Bom", _soundSourceObj.transform, "SE");
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

            /* L を押すと、全部のモジュールの数量を10個に、オプションを除くすべてのレベルを5にするコード */
            if (Input.GetKeyDown(KeyCode.L) && _isPlay)
            {
                _isPlay = false;
                var runtimeModuleManager = RuntimeModuleManager.Instance;
                foreach (var module in runtimeModuleManager.AllRuntimeModuleData)
                {
                    if (_moduleDataStore.FindWithId(module.Id).ModuleType != ModuleData.MODULE_TYPE.Options)
                    {
                        module.SetLevel(5);
                    }
                    else
                    {
                        // オプションはレベルを1に設定
                        module.SetLevel(1);
                    }
                    module.SetQuantity(10);
                }
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

