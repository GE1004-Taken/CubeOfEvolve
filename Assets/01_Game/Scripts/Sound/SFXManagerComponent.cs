// 作成日：250618
// 作成者：AT
//   概要 ：空間に紐づくSE(SFX)の再生管理用カプセル。子が増えるにで、専用オブジェクトにアタッチ推奨。
// 使い方：任意のゲームオブジェクトにアタッチして、使用したい音を登録しましょう。

using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Assets.AT
{
    [System.Serializable]
    public class SFXDataWrapper
    {
        public string name;
        public SoundData soundData;
    }

    public class SFXManagerComponent : MonoBehaviour
    {
        // -----SerializeField

        [SerializeField, Tooltip("このSFXManagerComponentが管理するAudioSourceの初期プール数\nこれ以上の同時再生でも大丈夫。追加で生成する")]
        private int _initialAudioSourcePoolSize = 2;

        [SerializeField, Tooltip("このコンポーネントで再生するSFXのリスト")]
        private SFXDataWrapper[] _sfxDataArray; // このコンポーネントが直接SoundDataを保持

        // -----Field

        // このSFXManagerComponentが管理するAudioSourceのプール
        private Stack<AudioSource> _sfxAudioSourcePool = new Stack<AudioSource>();

        // 貸し出し中のAudioSourceとその監視コンポーネントの管理
        private Dictionary<AudioSource, OnAudioSourceFinished> _borrowedAudioSources = new Dictionary<AudioSource, OnAudioSourceFinished>();

        // SFX名とSoundDataの対応を保持する辞書
        private Dictionary<string, SoundData> _sfxDictionary = new Dictionary<string, SoundData>();

        // -----UnityMessage

        private void Awake()
        {
            // SFXデータを辞書にセット
            foreach (var sfxDataWrapper in _sfxDataArray)
            {
                if (sfxDataWrapper.soundData != null)
                {
                    if (!_sfxDictionary.ContainsKey(sfxDataWrapper.name))
                    {
                        _sfxDictionary.Add(sfxDataWrapper.name, sfxDataWrapper.soundData);
                    }
                    else
                    {
                        Debug.LogWarning($"[SFXManagerComponent] SFX名 '{sfxDataWrapper.name}' は既に登録されています。上書きは行いません。オブジェクト: {gameObject.name}");
                    }
                }
                else
                {
                    Debug.LogError($"[SFXManagerComponent] SFX '{sfxDataWrapper.name}' が設定されていません。オブジェクト: {gameObject.name}");
                }
            }

            // 自身のGameObjectの子としてAudioSourceプールを初期化
            for (int i = 0; i < _initialAudioSourcePoolSize; i++)
            {
                GameObject audioSourceGO = new GameObject($"_SFXAudioSource_Pooled_{i}");
                audioSourceGO.transform.SetParent(this.transform); // 自身の子供にする
                AudioSource newSource = audioSourceGO.AddComponent<AudioSource>();
                newSource.playOnAwake = false; // 自動再生しない
                newSource.enabled = false;     // 初期状態では無効
                _sfxAudioSourcePool.Push(newSource);
            }
        }

        private void OnDestroy()
        {
            // 借りているAudioSourceを全て停止し、参照をクリア
            foreach (var pair in _borrowedAudioSources.ToList()) // ToList()でコピーを作成
            {
                AudioSource audioSource = pair.Key;
                OnAudioSourceFinished monitor = pair.Value;

                if (monitor != null)
                {
                    monitor.StopMonitoring();
                }
                if (audioSource != null)
                {
                    audioSource.Stop();
                    audioSource.clip = null;
                    audioSource.enabled = false;
                    // GameObject自体が親と一緒に破棄されるため、プールに戻す必要はない
                }
            }
            _borrowedAudioSources.Clear();

            // プール内のAudioSourceも全て破棄（親が破棄されるため、自動的に子も破棄されるが明示的に）
            foreach (var audioSource in _sfxAudioSourcePool)
            {
                if (audioSource != null) Destroy(audioSource.gameObject);
            }
            _sfxAudioSourcePool.Clear();
        }

        // -----Public

        /// <summary>
        /// このオブジェクトからSFXを再生します。自身のプールからAudioSourceを借ります。
        /// </summary>
        /// <param name="sfxName">再生するSFXの別名</param>
        /// <param name="mixerGroupName">割り当てるAudioMixerGroupの名前（オプション）</param>
        /// <param name="loop">ループ再生するかどうか（デフォルト: false）</param>
        public void PlaySFX(string sfxName, string mixerGroupName = null, bool loop = false)
        {
            if (!_sfxDictionary.TryGetValue(sfxName, out var soundData)) // 自身の辞書から取得
            {
                Debug.LogWarning($"[SFXManagerComponent] SFX '{sfxName}' のSoundDataがこのコンポーネントに登録されていません。オブジェクト: {gameObject.name}");
                return;
            }

            AudioClip clipToPlay = soundData.GetAudioClip();
            if (clipToPlay == null)
            {
                Debug.LogWarning($"[SFXManagerComponent] SFXオーディオクリップが見つかりません: {sfxName}。オブジェクト: {gameObject.name}");
                return;
            }

            AudioSource audioSource = GetUnusedAudioSource();
            if (audioSource == null)
            {
                Debug.LogWarning($"[SFXManagerComponent] SFX '{sfxName}' 用のAudioSourceを取得できませんでした。プールが枯渇している可能性があります。オブジェクト: {gameObject.name}");
                return;
            }

            // AudioSourceの設定
            ResetAudioSourceProperties(audioSource); // プロパティを初期状態にリセットします
            audioSource.clip = clipToPlay;
            audioSource.loop = loop;
            audioSource.spatialBlend = 1f; // SFXは通常3Dサウンド

            // ミキサーグループ設定 (GameSoundManagerから取得)
            if (GameSoundManager.Instance != null) // GameSoundManagerのインスタンスがない場合は設定しない
            {
                GameSoundManager.Instance.SetAudioMixerGroup(audioSource, mixerGroupName);
            }
            else
            {
                Debug.LogWarning("[SFXManagerComponent] GameSoundManagerが見つからないため、AudioMixerGroupを設定できません。");
            }

            // AudioSourceの再生終了監視とプール返却処理
            OnAudioSourceFinished finishedMonitor = audioSource.gameObject.GetOrAddComponent<OnAudioSourceFinished>();

            Action onFinished = () =>
            {
                if (_borrowedAudioSources.ContainsKey(audioSource))
                {
                    _borrowedAudioSources.Remove(audioSource);
                    ReturnAudioSource(audioSource);
                }
            };

            // 監視を開始し、再生
            finishedMonitor.Monitor(audioSource, onFinished);
            audioSource.Play(); // ここで再生を開始
            _borrowedAudioSources.Add(audioSource, finishedMonitor);
        }

        /// <summary>
        /// 特定のSFXの再生を停止します。
        /// </summary>
        /// <param name="sfxName">停止するSFXの別名</param>
        public void StopSFX(string sfxName)
        {
            // SoundDataがこのコンポーネントに存在するか確認
            if (!_sfxDictionary.TryGetValue(sfxName, out var soundData))
            {
                Debug.LogWarning($"[SFXManagerComponent] 停止したいSFX '{sfxName}' のSoundDataがこのコンポーネントに登録されていません。オブジェクト: {gameObject.name}");
                return;
            }

            AudioClip clipToStop = soundData.GetAudioClip();
            if (clipToStop == null) return;

            foreach (var pair in _borrowedAudioSources.ToList())
            {
                AudioSource audioSource = pair.Key;
                OnAudioSourceFinished monitor = pair.Value;

                if (audioSource != null && audioSource.clip == clipToStop && audioSource.isPlaying)
                {
                    if (monitor != null) monitor.StopMonitoring();
                    ReturnAudioSource(audioSource);
                    _borrowedAudioSources.Remove(audioSource);
                    return; // 最初の見つかったものを停止して終了
                }
            }
        }

        /// <summary>
        /// このSFXManagerComponentが再生している全てのSFXを停止します。
        /// </summary>
        public void StopAllSFX()
        {
            foreach (var pair in _borrowedAudioSources.ToList())
            {
                AudioSource audioSource = pair.Key;
                OnAudioSourceFinished monitor = pair.Value;

                if (monitor != null) monitor.StopMonitoring();
                ReturnAudioSource(audioSource);
            }
            _borrowedAudioSources.Clear();
        }

        // -----Private

        /// <summary>
        /// 自身の子にあるAudioSourceプールから、現在再生中でないAudioSourceを取得します。
        /// プールが枯渇した場合は、新たに自身の子供として生成します。
        /// </summary>
        /// <returns>利用可能なAudioSource、またはnull（エラー発生時）。</returns>
        private AudioSource GetUnusedAudioSource()
        {
            if (_sfxAudioSourcePool.Count > 0)
            {
                AudioSource source = _sfxAudioSourcePool.Pop();
                if (source != null)
                {
                    ResetAudioSourceProperties(source);
                    source.enabled = true;
                    return source;
                }
            }

            Debug.LogWarning($"[SFXManagerComponent] AudioSourceプールが枯渇しています（オブジェクト: {gameObject.name}）。新しいAudioSourceを生成します。");
            GameObject audioSourceGO = new GameObject($"_SFXAudioSource_New_{Guid.NewGuid().ToString().Substring(0, 8)}");
            audioSourceGO.transform.SetParent(this.transform); // 自身の子供にする
            AudioSource newSource = audioSourceGO.AddComponent<AudioSource>();
            newSource.playOnAwake = false;
            ResetAudioSourceProperties(newSource);
            return newSource;
        }

        /// <summary>
        /// 使用済みのAudioSourceをプールに戻します。
        /// </summary>
        /// <param name="audioSource">プールに戻すAudioSource。</param>
        private void ReturnAudioSource(AudioSource audioSource)
        {
            if (audioSource == null) return;
            audioSource.Stop();
            audioSource.clip = null;
            audioSource.enabled = false;
            // 親は既にこのSFXManagerComponentのGameObjectなので変更不要
            _sfxAudioSourcePool.Push(audioSource);
        }

        /// <summary>
        /// AudioSourceのプロパティを初期状態にリセットします。
        /// </summary>
        /// <param name="audioSource">リセットするAudioSource。</param>
        private void ResetAudioSourceProperties(AudioSource audioSource)
        {
            audioSource.volume = 1f;
            audioSource.pitch = 1f;
            audioSource.panStereo = 0f;
            audioSource.spatialBlend = 0f; // デフォルトは2D。PlaySFXで3Dに上書きされます。
            audioSource.outputAudioMixerGroup = null;
            audioSource.loop = false;
            audioSource.clip = null;
        }
    }
}