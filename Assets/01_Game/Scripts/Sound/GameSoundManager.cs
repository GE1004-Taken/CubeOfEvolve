using R3;
using R3.Triggers;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace Assets.AT
{
    public class GameSoundManager : MonoBehaviour
    {
        public static GameSoundManager Instance { get; private set; }

        [Serializable]
        public class SoundDataWrapper
        {
            public string name;
            public SoundData soundData; // 実際のサウンド再生情報を保持
        }

        [Serializable]
        public class SceneBGM
        {
            public string sceneName;
            public string bgmName; // シーンに紐付けるBGM名
        }

        [SerializeField] private SoundDataWrapper[] _soundDataArray; // サウンドデータ一覧
        [SerializeField] private SceneBGM[] _inScenePlay; // シーンに紐づくBGM定義
        [SerializeField] private int _initialManagerAudioSources = 5; // AudioSourceプールの初期数
        [SerializeField] private AudioMixer _audioMixer; // 使用するAudioMixer

        private Stack<AudioSource> _audioSourcePool = new(); // 非空間SE/BGM用AudioSourceプール
        private Dictionary<string, SoundData> _soundDictionary = new(); // サウンド名→データ辞書
        private Dictionary<string, string> _sceneBGMMapping = new(); // シーン名→BGM名辞書
        private Dictionary<string, AudioMixerGroup> _mixerGroupCache = new(); // AudioMixerGroupキャッシュ

        private AudioSource _currentBGMSource; // 現在再生中のBGM AudioSource
        private (SoundData data, string name) _currentBGMInfo; // 現在のBGM情報
        private IDisposable _bgmFadeDisposable; // BGMフェード購読解除用
        private IDisposable _bgmLoopDisposable; // BGMループ監視解除用

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject); // シングルトン重複防止
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject); // シーンをまたいで保持

            // 初期AudioSourceを生成
            for (int i = 0; i < _initialManagerAudioSources; ++i)
                _audioSourcePool.Push(CreateAudioSource($"_PooledAudioSource_{i}"));

            // サウンド辞書を初期化
            foreach (var s in _soundDataArray)
            {
                if (s.soundData == null)
                {
                    Debug.LogError($"[GameSoundManager] SoundData '{s.name}' が設定されていません。");
                    continue;
                }
                if (!_soundDictionary.TryAdd(s.name, s.soundData))
                    Debug.LogWarning($"[GameSoundManager] サウンド名 '{s.name}' は既に登録されています。");
            }

            // シーンとBGMのマッピング
            foreach (var sceneBGM in _inScenePlay)
                _sceneBGMMapping[sceneBGM.sceneName] = sceneBGM.bgmName;

            InitMixerGroupCache();
            SceneManager.sceneLoaded += OnSceneLoaded; // シーン変更時のBGM切り替え登録
        }

        private void OnDestroy()
        {
            if (Instance != this) return;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            DisposeBGMSubscriptions();
            foreach (var src in _audioSourcePool)
                if (src) Destroy(src.gameObject);
            _audioSourcePool.Clear();
        }

        /// <summary>
        /// UIやシステムSEなど、空間に依存しないSEを再生
        /// </summary>
        public void PlaySE(string name, string mixerGroupName = null)
        {
            if (!TryGetClip(name, out var clip)) return;
            var source = GetOrCreateAudioSource();
            ResetAudioSource(source);
            source.clip = clip;
            source.loop = false;
            source.spatialBlend = 0f;
            SetAudioMixerGroup(source, mixerGroupName);
            source.Play();
            source.gameObject.GetOrAddComponent<OnAudioSourceFinished>().Monitor(source, () => ReleaseAudioSource(source));
        }

        public void PlaySFX(string name, Transform transform, string mixerGroupName = null)
        {
            if (!TryGetClip(name, out var clip)) return;
            var source = GetOrCreateAudioSource();
            ResetAudioSource(source);
            source.clip = clip;
            source.loop = false;
            source.spatialBlend = 1f;
            source.gameObject.transform.position = transform.position;
            SetAudioMixerGroup(source, mixerGroupName);
            source.Play();
            source.gameObject.GetOrAddComponent<OnAudioSourceFinished>().Monitor(source, () => ReleaseAudioSource(source));
        }

        /// <summary>
        /// BGM再生（指定名・フェードインあり）
        /// </summary>
        public void PlayBGM(string bgmName, string mixerGroupName = null, float fadeIn = 0f)
        {
            if (!_soundDictionary.TryGetValue(bgmName, out var soundData))
            {
                Debug.LogWarning($"[GameSoundManager] BGM'{bgmName}'が見つかりません。");
                return;
            }

            DisposeBGMSubscriptions();
            ReleaseAudioSource(_currentBGMSource);

            _currentBGMSource = GetOrCreateAudioSource();
            ResetAudioSource(_currentBGMSource);
            _currentBGMSource.clip = soundData.GetAudioClip();
            _currentBGMSource.loop = true;
            _currentBGMSource.spatialBlend = 0f;
            SetAudioMixerGroup(_currentBGMSource, mixerGroupName);
            _currentBGMInfo = (soundData, bgmName);

            if (fadeIn > 0f)
            {
                _currentBGMSource.volume = 0f;
                _currentBGMSource.Play();
                _bgmFadeDisposable = FadeAudio(_currentBGMSource, 1f, fadeIn);
            }
            else
            {
                _currentBGMSource.volume = 1f;
                _currentBGMSource.Play();
            }

            if (soundData is LoopSoundData)
            {
                _bgmLoopDisposable = this.UpdateAsObservable()
                    .Where(_ => _currentBGMSource != null && _currentBGMSource.isPlaying)
                    .Subscribe(_ => LoopCheck(_currentBGMSource, soundData))
                    .AddTo(this);
            }
        }

        /// <summary>
        /// 現在のBGMをフェードアウト停止
        /// </summary>
        public void StopBGMWithFade(float fadeDuration)
        {
            if (_currentBGMSource == null || !_currentBGMSource.isPlaying)
                return;
            _bgmFadeDisposable?.Dispose();
            _bgmFadeDisposable = FadeAudio(_currentBGMSource, 0f, fadeDuration, () =>
            {
                _currentBGMSource.Stop();
                ReleaseAudioSource(_currentBGMSource);
                _currentBGMSource = null;
                _currentBGMInfo = (null, null);
            });
        }

        /// <summary>
        /// 指定名のBGMのみを停止（現在再生中と一致する場合）
        /// </summary>
        public void StopBGMByNameWithFade(string bgmName, float fadeDuration)
        {
            if (_currentBGMInfo.name != bgmName) return;
            StopBGMWithFade(fadeDuration);
        }

        /// <summary>
        /// シーン名に対応するBGM再生
        /// </summary>
        public void PlayBGMForScene(string sceneName, float fadeIn = 0f)
        {
            if (_sceneBGMMapping.TryGetValue(sceneName, out var bgmName))
                PlayBGM(bgmName, "BGM", fadeIn);
        }

        public SoundData GetSoundData(string name) =>
            _soundDictionary.TryGetValue(name, out var data) ? data : null;

        private void OnSceneLoaded(Scene scene, LoadSceneMode _) => PlayBGMForScene(scene.name);

        private AudioSource GetOrCreateAudioSource()
        {
            if (_audioSourcePool.Count > 0)
                return EnableAudioSource(_audioSourcePool.Pop());
            return EnableAudioSource(CreateAudioSource($"_PooledAudioSource_New_{Guid.NewGuid():N}"));
        }

        private AudioSource EnableAudioSource(AudioSource source)
        {
            if (source) source.enabled = true;
            return source;
        }

        private AudioSource CreateAudioSource(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.enabled = false;
            return source;
        }

        private void ReleaseAudioSource(AudioSource source)
        {
            if (source == null) return;
            source.Stop();
            source.clip = null;
            source.enabled = false;
            _audioSourcePool.Push(source);
        }

        private void ResetAudioSource(AudioSource source)
        {
            source.volume = 1f;
            source.pitch = 1f;
            source.panStereo = 0f;
            source.spatialBlend = 0f;
            source.loop = false;
            source.clip = null;
            source.outputAudioMixerGroup = null;
        }

        private void InitMixerGroupCache()
        {
            if (_audioMixer == null)
            {
                Debug.LogError("[GameSoundManager] AudioMixerが設定されていません。");
                return;
            }
            foreach (var group in _audioMixer.FindMatchingGroups(""))
                _mixerGroupCache.TryAdd(group.name, group);
        }

        public void SetAudioMixerGroup(AudioSource source, string groupName)
        {
            if (string.IsNullOrEmpty(groupName)) return;
            if (_mixerGroupCache.TryGetValue(groupName, out var group))
                source.outputAudioMixerGroup = group;
        }

        private void DisposeBGMSubscriptions()
        {
            _bgmFadeDisposable?.Dispose();
            _bgmLoopDisposable?.Dispose();
        }

        private IDisposable FadeAudio(AudioSource source, float to, float duration, Action onComplete = null)
        {
            float from = source.volume;
            return this.UpdateAsObservable()
                .Subscribe(_ =>
                {
                    if (!source) return;
                    source.volume = Mathf.MoveTowards(source.volume, to, Time.unscaledDeltaTime * (1f / duration));
                    if (Mathf.Approximately(source.volume, to))
                    {
                        onComplete?.Invoke();
                        _bgmFadeDisposable?.Dispose();
                    }
                })
                .AddTo(this);
        }

        private bool TryGetClip(string name, out AudioClip clip)
        {
            clip = null;
            if (!_soundDictionary.TryGetValue(name, out var data))
            {
                Debug.LogWarning($"[GameSoundManager] サウンド'{name}'は登録されていません。");
                return false;
            }
            clip = data.GetAudioClip();
            if (clip == null)
            {
                Debug.LogWarning($"[GameSoundManager] サウンド'{name}'にAudioClipが見つかりません。");
                return false;
            }
            return true;
        }

        private void LoopCheck(AudioSource source, SoundData data)
        {
            if (data is not LoopSoundData loopData || !source.isPlaying || source.clip == null) return;

            int ConvertFreq(long sample) =>
                (int)(sample * (double)source.clip.frequency / loopData.frequency);

            if (source.timeSamples >= ConvertFreq(loopData.loopEnd))
                source.timeSamples -= ConvertFreq(loopData.loopEnd - loopData.loopStart);
        }
    }

    // AudioSourceの再生終了を検出し、コールバックを呼ぶコンポーネント
    public class OnAudioSourceFinished : MonoBehaviour
    {
        private AudioSource _audioSource;
        private Action _onFinishedCallback;
        private bool _isMonitoring = false;

        public void Monitor(AudioSource source, Action onFinished)
        {
            StopMonitoring();
            _audioSource = source;
            _onFinishedCallback = onFinished;
            _isMonitoring = true;
        }

        private void Update()
        {
            if (_isMonitoring && _audioSource && !_audioSource.isPlaying)
            {
                _onFinishedCallback?.Invoke();
                StopMonitoring();
            }
        }

        public void StopMonitoring()
        {
            _isMonitoring = false;
            _audioSource = null;
            _onFinishedCallback = null;
        }

        private void OnDisable() => StopMonitoring();
        private void OnDestroy() => StopMonitoring();
    }

    // 指定コンポーネントがなければ追加して返す拡張
    public static class GameObjectExtensions
    {
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            var comp = go.GetComponent<T>();
            return comp != null ? comp : go.AddComponent<T>();
        }
    }
}
