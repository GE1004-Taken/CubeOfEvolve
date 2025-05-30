using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using R3;
using R3.Triggers;
using System;

/// <summary>
/// ゲーム全体のサウンド管理を行うシングルトンクラスです。
/// BGM、SEの再生、AudioSourceのプール管理、フェード処理、シーンごとのBGM切り替えなどを担当します。
/// </summary>
public class SoundManager : MonoBehaviour
{
    // ----- Singleton (シングルトン)
    /// <summary>
    /// SoundManager のシングルトンインスタンスです。
    /// </summary>
    public static SoundManager Instance { get; private set; }

    // ----- Serializable Fields (シリアライズフィールド)
    /// <summary>
    /// シリアライズ可能なサウンドデータのラッパークラスです。
    /// インスペクター上で名前と SoundData アセットを紐付けるために使用します。
    /// </summary>
    [System.Serializable]
    public class SoundDataWrapper
    {
        public string name;
        public SoundData soundData;
    }

    /// <summary>
    /// インスペクターから設定されるサウンドデータの配列です。
    /// </summary>
    [SerializeField]
    private SoundDataWrapper[] soundDatas;

    /// <summary>
    /// シリアライズ可能なシーンごとの BGM 設定クラスです。
    /// </summary>
    [System.Serializable]
    public class SceneBGM
    {
        public string sceneName;
        public string bgmName;
    }
    /// <summary>
    /// インスペクターから設定されるシーンごとの BGM 設定の配列です。
    /// </summary>
    [SerializeField]
    private SceneBGM[] inScenePlay;

    [Header("AudioSource Pooling Settings")]
    [Tooltip("SoundManagerが自身で保持するAudioSourceの初期数（UI/System SE用）")]
    [SerializeField]
    private int _initialManagerAudioSources = 20;

    [Tooltip("各ゲームオブジェクトが保持できるAudioSourceの最大数")]
    [SerializeField]
    private int _maxPooledAudioSourcesPerObject = 5;

    // ----- Private Fields (プライベートフィールド)
    /// <summary>
    /// SoundManagerが自身で再生に使用する AudioSource のプール (UI/System SE用)。
    /// </summary>
    private Stack<AudioSource> _managerAudioSourcePool = new Stack<AudioSource>();
    /// <summary>
    /// サウンド名と SoundData の対応を保持する辞書です。
    /// </summary>
    private Dictionary<string, SoundData> soundDictionary = new Dictionary<string, SoundData>();
    /// <summary>
    /// シーン名と BGM 名の対応を保持する辞書です。
    /// </summary>
    private Dictionary<string, string> sceneBGMMapping = new Dictionary<string, string>();
    /// <summary>
    /// 現在再生中の BGM の AudioSource です。
    /// </summary>
    private AudioSource currentBGMSource;
    /// <summary>
    /// 現在再生中の BGM の SoundData です。
    /// </summary>
    private SoundData currentSoundData;

    /// <summary>
    /// BGMのフェード処理中の購読を管理するためのDisposableです。
    /// </summary>
    private IDisposable _bgmFadeDisposable;

    // --- AudioSource Pooling Fields (AudioSourceプール関連フィールド)
    /// <summary>
    /// SoundManagerが管理する、どのGameObjectにも貸し出されていないAudioSourceのグローバルプールです。
    /// </summary>
    private Stack<AudioSource> _globalAudioSourcePool = new Stack<AudioSource>();
    /// <summary>
    /// 各GameObjectに貸し出されている、またはそのGameObjectにアタッチされて利用可能なAudioSourceのリストです。
    /// </summary>
    private Dictionary<GameObject, List<AudioSource>> _objectAudioSourcePools = new Dictionary<GameObject, List<AudioSource>>();
    /// <summary>
    /// AudioSourceとそのオーナーGameObjectを紐付けるマップです。
    /// </summary>
    private Dictionary<AudioSource, GameObject> _audioSourceOwnerMap = new Dictionary<AudioSource, GameObject>();

    /// <summary>
    /// AudioMixerGroupのキャッシュです。名前で素早く参照できます。
    /// </summary>
    private Dictionary<string, AudioMixerGroup> _mixerGroupCache = new Dictionary<string, AudioMixerGroup>();

    // ----- Unity Messages (Unityイベントメッセージ)
    /// <summary>
    /// スクリプトインスタンスがロードされたときに呼び出されます。
    /// シングルトンの初期化、AudioSourceプールの設定、サウンドデータとシーンBGMのマッピング、ミキサーグループのキャッシュ初期化を行います。
    /// </summary>
    private void Awake()
    {
        // シングルトンの設定
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject); // シーンをまたいでも破棄されないようにします。
        }
        else
        {
            Destroy(gameObject); // すでにインスタンスが存在する場合は、新しいインスタンスを破棄します。
            return;
        }

        // SoundManager自身が使うAudioSourceの初期化 (UI/System SE用)
        for (var i = 0; i < _initialManagerAudioSources; ++i)
        {
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.playOnAwake = false; // 自動再生しないように設定します。
            newSource.enabled = false; // 初期状態では無効にしておきます。
            _managerAudioSourcePool.Push(newSource); // プールに追加します。
        }

        // soundDictionaryにサウンドデータをセット
        foreach (var soundDataWrapper in soundDatas)
        {
            if (soundDataWrapper.soundData != null)
            {
                // 重複登録を避けます。
                if (!soundDictionary.ContainsKey(soundDataWrapper.name))
                {
                    soundDictionary.Add(soundDataWrapper.name, soundDataWrapper.soundData);
                }
                else
                {
                    Debug.LogWarning($"[SoundManager] サウンド名 '{soundDataWrapper.name}' は既に登録されています。上書きは行いません。");
                }
            }
            else
            {
                Debug.LogError($"[SoundManager] SoundData '{soundDataWrapper.name}' が設定されていません。");
            }
        }

        // sceneBGMMappingにシーンBGMデータをセット
        foreach (var sceneBGM in inScenePlay)
        {
            sceneBGMMapping[sceneBGM.sceneName] = sceneBGM.bgmName;
        }

        // オーディオミキサーグループのキャッシュ初期化
        InitMixerGroupCache();

        // シーンロード時のBGM設定イベントに登録
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// 初回Updateの前に呼び出されます。
    /// BGMループチェックの購読を設定します。
    /// </summary>
    private void Start()
    {
        // BGMループチェックの購読
        this.UpdateAsObservable()
            .Where(x => (currentBGMSource != null && currentBGMSource.isPlaying && currentSoundData is LoopSoundData))
            .Subscribe(_ =>
            {
                LoopCheck(currentBGMSource, currentSoundData);
            })
            .AddTo(this); // SoundManagerが破棄されたら自動的に購読解除されます。
    }

    /// <summary>
    /// このスクリプトがアタッチされたゲームオブジェクトが破棄されるときに呼び出されます。
    /// シーンロードイベントの解除、フェード処理中の購読解除、および全てのAudioSourceプールのクリーンアップを行います。
    /// </summary>
    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded; // イベントの解除
            _bgmFadeDisposable?.Dispose(); // フェード処理中の購読を解除

            // グローバルプール内のAudioSourceを全て破棄
            foreach (var audioSource in _globalAudioSourcePool)
            {
                if (audioSource != null) Destroy(audioSource.gameObject);
            }
            _globalAudioSourcePool.Clear();

            // SoundManager自身のAudioSourceプール内のAudioSourceを全て破棄
            foreach (var audioSource in _managerAudioSourcePool)
            {
                if (audioSource != null) Destroy(audioSource.gameObject);
            }
            _managerAudioSourcePool.Clear();

            // 各オブジェクトプール内のAudioSourceを全て破棄
            foreach (var pair in _objectAudioSourcePools)
            {
                foreach (var audioSource in pair.Value)
                {
                    if (audioSource != null) Destroy(audioSource.gameObject);
                }
            }
            _objectAudioSourcePools.Clear();
            _audioSourceOwnerMap.Clear();
        }
    }

    // ----- Public Methods (公開メソッド)
    /// <summary>
    /// 特定の空間位置に紐づかないサウンド（UI音、システムSEなど）を再生します。
    /// SoundManager自身のAudioSourceプールから利用可能なものを取得して再生します。
    /// </summary>
    /// <param name="name">再生するサウンドの別名</param>
    /// <param name="mixerGroupName">割り当てるAudioMixerGroupの名前（オプション）</param>
    public void Play(string name, string mixerGroupName = null)
    {
        if (!soundDictionary.TryGetValue(name, out var soundData))
        {
            Debug.LogWarning($"[SoundManager] サウンド'{name}'は登録されていません。");
            return;
        }

        AudioClip clipToPlay = soundData.GetAudioClip();
        if (clipToPlay == null)
        {
            Debug.LogWarning($"[SoundManager] オーディオクリップが見つかりません: {name}");
            return;
        }

        AudioSource audioSource = GetUnusedManagerAudioSource();
        if (audioSource == null)
        {
            Debug.LogWarning("[SoundManager] SoundManagerが利用可能なAudioSourceを持っていません。SEが再生できませんでした。");
            return;
        }

        ResetAudioSourceProperties(audioSource); // プロパティを初期状態にリセットします。
        audioSource.clip = clipToPlay;
        audioSource.loop = false; // 通常、SEはループしません。
        audioSource.spatialBlend = 0f; // 2Dサウンドとして再生します。

        SetAudioMixerGroup(audioSource, mixerGroupName);
        audioSource.Play();

        // Playメソッドで再生された単発SEも自動で解放するように、クリップの再生時間後にプールに戻します。
        Observable.Timer(TimeSpan.FromSeconds(clipToPlay.length))
            .Subscribe(_ =>
            {
                ReleaseManagerAudioSource(audioSource);
            })
            .AddTo(this); // SoundManagerが破棄されたら自動購読解除されます。
    }

    /// <summary>
    /// 指定されたGameObjectの位置から空間的なサウンド（SFX）を再生します。
    /// オブジェクトプールからAudioSourceを取得し、再生終了後にプールに戻します。
    /// </summary>
    /// <param name="name">再生するサウンドの別名</param>
    /// <param name="sourceObject">サウンドの発生源となるGameObject</param>
    /// <param name="mixerGroupName">割り当てるAudioMixerGroupの名前（オプション）</param>
    public void PlaySFXAt(string name, GameObject sourceObject, string mixerGroupName = null)
    {
        if (sourceObject == null)
        {
            Debug.LogWarning($"[SoundManager] サウンド'{name}'の発生源となるGameObjectがnullです。呼び出し元: {GetCallingMethodName()}");
            return;
        }
        if (!soundDictionary.TryGetValue(name, out var soundData))
        {
            Debug.LogWarning($"[SoundManager] サウンド'{name}'は登録されていません。呼び出し元オブジェクト: {sourceObject.name}");
            return;
        }

        AudioClip clipToPlay = soundData.GetAudioClip();
        if (clipToPlay == null)
        {
            Debug.LogWarning($"[SoundManager] オーディオクリップが見つかりません: {name}");
            return;
        }

        AudioSource audioSource = GetAudioSource(sourceObject);
        if (audioSource == null)
        {
            Debug.LogWarning($"[SoundManager] オブジェクト'{sourceObject.name}'に利用可能なAudioSourceがありません。サウンド'{name}'が再生できませんでした。");
            return;
        }

        ResetAudioSourceProperties(audioSource);
        audioSource.clip = clipToPlay;
        audioSource.loop = false;
        audioSource.spatialBlend = 1f; // 3Dサウンドとして再生します。

        SetAudioMixerGroup(audioSource, mixerGroupName);
        audioSource.Play();

        // SFXの再生終了を検知し、AudioSourceをプールに戻す
        audioSource.gameObject.UpdateAsObservable() // AudioSourceがアタッチされているGameObjectのUpdateを購読します。
            .Where(_ => !audioSource.isPlaying) // 再生中でないことを検知します。
            .Take(1) // 一度だけ実行します。
            .Subscribe(_ =>
            {
                ReleaseAudioSource(audioSource);
            })
            .AddTo(audioSource.gameObject); // audioSourceがアタッチされているGameObjectが破棄されたら自動購読解除されます。
    }

    /// <summary>
    /// 現在再生中のBGMをフェードアウトさせて停止します。
    /// </summary>
    /// <param name="fadeDuration">フェードアウトにかける時間（秒）</param>
    public void StopBGMWithFade(float fadeDuration)
    {
        if (currentBGMSource == null || !currentBGMSource.isPlaying)
        {
            Debug.Log("[SoundManager] 再生中のBGMがありません。停止処理は不要です。");
            return;
        }

        _bgmFadeDisposable?.Dispose(); // 既存のフェード処理を中止します。

        float startVolume = currentBGMSource.volume;
        _bgmFadeDisposable = Observable.Interval(TimeSpan.FromSeconds(Time.deltaTime))
            .TakeWhile(_ => currentBGMSource != null && currentBGMSource.volume > 0)
            .Subscribe(
                _ =>
                {
                    if (currentBGMSource != null)
                    {
                        currentBGMSource.volume -= startVolume * (Time.deltaTime / fadeDuration);
                        if (currentBGMSource.volume <= 0)
                        {
                            currentBGMSource.Stop();
                            currentBGMSource.volume = startVolume; // 元のボリュームに戻しておきます。
                            _bgmFadeDisposable?.Dispose(); // 購読を解除します。
                            Debug.Log("[SoundManager] BGMのフェードアウトが完了し、停止しました。");
                        }
                    }
                    else
                    {
                        _bgmFadeDisposable?.Dispose(); // AudioSourceがnullになった場合も解除します。
                        Debug.LogWarning("[SoundManager] BGMのAudioSourceがフェード中に破棄されました。");
                    }
                }
            )
            .AddTo(this); // SoundManagerが破棄されたら自動的に購読解除されます。
    }

    /// <summary>
    /// 指定されたBGM名をフェードアウトさせて停止します。
    /// 現在再生中のBGMが指定されたBGM名と一致する場合のみ処理します。
    /// </summary>
    /// <param name="bgmName">停止するBGMの別名</param>
    /// <param name="fadeDuration">フェードアウトにかける時間（秒）</param>
    public void StopBGMByNameWithFade(string bgmName, float fadeDuration)
    {
        if (currentBGMSource == null || !currentBGMSource.isPlaying)
        {
            Debug.Log("[SoundManager] 再生中のBGMがありません。停止処理は不要です。");
            return;
        }

        string playingBGMName = soundDictionary.FirstOrDefault(x => x.Value == currentSoundData).Key;
        if (playingBGMName != bgmName)
        {
            Debug.LogWarning($"[SoundManager] 現在再生中のBGM ('{playingBGMName}') は、指定されたBGM名 ('{bgmName}') と異なります。停止しません。");
            return;
        }

        StopBGMWithFade(fadeDuration);
    }

    /// <summary>
    /// シーンがロードされた際に呼び出され、シーンに対応するBGMを再生します。
    /// </summary>
    /// <param name="scene">ロードされたシーンの情報</param>
    /// <param name="mode">シーンのロードモード</param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayBGMForScene(scene.name);
    }

    /// <summary>
    /// 指定されたシーン名に対応する BGM を再生します。
    /// </summary>
    /// <param name="sceneName">BGMを再生するシーンの名前</param>
    /// <param name="fadeInDuration">BGMのフェードインにかける時間（秒）。0の場合は即時再生。</param>
    public void PlayBGMForScene(string sceneName, float fadeInDuration = 0f)
    {
        if (sceneBGMMapping.TryGetValue(sceneName, out var bgmName))
        {
            Debug.Log($"[SoundManager] シーン'{sceneName}'に対応するBGM '{bgmName}' を再生します。");
            PlayBGM(bgmName, "BGM", fadeInDuration);
        }
        else
        {
            Debug.LogWarning($"[SoundManager] シーン名'{sceneName}'に対応するBGMが見つかりません。");
        }
    }

    /// <summary>
    /// 指定されたBGM名のBGMを再生します。ミキサーグループを指定することも可能。
    /// 既にBGMが再生されている場合は、そのBGMを停止してから新しいBGMを再生します。
    /// </summary>
    /// <param name="bgmName">再生するBGMの別名</param>
    /// <param name="mixerGroupName">割り当てるAudioMixerGroupの名前（オプション）</param>
    /// <param name="fadeInDuration">BGMのフェードインにかける時間（秒）。0の場合は即時再生。</param>
    public void PlayBGM(string bgmName, string mixerGroupName = null, float fadeInDuration = 0f)
    {
        _bgmFadeDisposable?.Dispose(); // 既存のフェード処理を中止します。

        if (currentBGMSource != null && currentBGMSource.isPlaying)
        {
            currentBGMSource.Stop();
        }

        if (!soundDictionary.TryGetValue(bgmName, out var soundData))
        {
            Debug.LogWarning($"[SoundManager] BGM'{bgmName}'が見つかりません。再生できません。");
            return;
        }

        currentSoundData = soundData;
        currentBGMSource = GetUnusedManagerAudioSource();
        if (currentBGMSource == null)
        {
            Debug.LogWarning("[SoundManager] SoundManagerがBGM再生に利用可能なAudioSourceを持っていません。BGMを再生できませんでした。");
            return;
        }

        ResetAudioSourceProperties(currentBGMSource); // プロパティを初期状態にリセットします。
        currentBGMSource.clip = soundData.GetAudioClip();
        currentBGMSource.loop = true;
        currentBGMSource.spatialBlend = 0f; // BGMは通常2Dサウンドです。

        SetAudioMixerGroup(currentBGMSource, mixerGroupName);

        if (fadeInDuration > 0f)
        {
            currentBGMSource.volume = 0f; // フェードイン開始時はボリュームを0にします。
            currentBGMSource.Play();

            _bgmFadeDisposable = Observable.Interval(TimeSpan.FromSeconds(Time.deltaTime))
                .TakeWhile(_ => currentBGMSource != null && currentBGMSource.volume < 1f)
                .Subscribe(
                    _ =>
                    {
                        if (currentBGMSource != null)
                        {
                            currentBGMSource.volume += (1f / fadeInDuration) * Time.deltaTime;
                            if (currentBGMSource.volume >= 1f)
                            {
                                currentBGMSource.volume = 1f; // 最大ボリュームに固定します。
                                _bgmFadeDisposable?.Dispose();
                                Debug.Log("[SoundManager] BGMのフェードインが完了しました。");
                            }
                        }
                        else
                        {
                            _bgmFadeDisposable?.Dispose();
                            Debug.LogWarning("[SoundManager] BGMのAudioSourceがフェードイン中に破棄されました。");
                        }
                    }
                )
                .AddTo(this); // SoundManagerが破棄されたら自動購読解除されます。
        }
        else
        {
            currentBGMSource.volume = 1f; // フェードインしない場合は即座に最大ボリュームを設定します。
            currentBGMSource.Play();
        }
    }

    // ----- Private Methods (プライベートメソッド)
    /// <summary>
    /// 現在シーンにある全てのAudioMixerGroupをキャッシュに登録します。
    /// </summary>
    private void InitMixerGroupCache()
    {
        foreach (var group in Resources.FindObjectsOfTypeAll<AudioMixerGroup>())
        {
            if (!_mixerGroupCache.ContainsKey(group.name))
            {
                _mixerGroupCache.Add(group.name, group);
            }
            else
            {
                Debug.LogWarning($"[SoundManager] オーディオミキサーグループ'{group.name}'が複数存在します。最初のものを使用します。");
            }
        }
    }

    /// <summary>
    /// SoundManager自身が持つAudioSourceプールから、現在再生中でないAudioSourceを取得します。
    /// 主にUIやシステムSE、BGMの再生に使用します。
    /// </summary>
    /// <returns>利用可能なAudioSource、またはプールが枯渇した場合はnull。</returns>
    private AudioSource GetUnusedManagerAudioSource()
    {
        if (_managerAudioSourcePool.Count > 0)
        {
            AudioSource source = _managerAudioSourcePool.Pop();
            if (source != null)
            {
                ResetAudioSourceProperties(source); // プロパティを初期状態にリセットします。
                source.enabled = true; // 有効にします。
                source.gameObject.SetActive(true); // GameObjectもアクティブにします (念のため)。
                return source;
            }
        }
        Debug.LogWarning("[SoundManager] SoundManagerのAudioSourceプールが枯渇しています。新しいAudioSourceは生成されません。");
        return null;
    }

    /// <summary>
    /// SoundManager自身のAudioSourceプールに戻します。
    /// 主にUIやシステムSE、BGMの再生に使用されたAudioSourceを再利用のためにプールします。
    /// </summary>
    /// <param name="audioSource">プールに戻すAudioSource。</param>
    private void ReleaseManagerAudioSource(AudioSource audioSource)
    {
        if (audioSource == null) return;
        audioSource.Stop();
        audioSource.clip = null; // クリップをクリアします。
        audioSource.enabled = false; // AudioSourceコンポーネントを無効にします。
        audioSource.gameObject.SetActive(false); // GameObjectを非アクティブにします。
        _managerAudioSourcePool.Push(audioSource); // プールに戻します。
    }

    /// <summary>
    /// サウンドの発生源となるGameObjectにアタッチされたAudioSourceを取得します。
    /// プールから再利用、または必要に応じて動的に生成します。
    /// </summary>
    /// <param name="owner">AudioSourceが必要なGameObject。</param>
    /// <returns>利用可能なAudioSource、またはプールが枯渇した場合はnull。</returns>
    private AudioSource GetAudioSource(GameObject owner)
    {
        // オーナーオブジェクトがnullまたは非アクティブの場合、新しいAudioSourceは割り当てません。
        if (owner == null || !owner.activeInHierarchy)
        {
            Debug.LogWarning($"[SoundManager] オーナーオブジェクトが有効ではありません。AudioSourceを取得できませんでした。");
            return null;
        }

        List<AudioSource> ownerSources;
        if (!_objectAudioSourcePools.TryGetValue(owner, out ownerSources))
        {
            ownerSources = new List<AudioSource>();
            _objectAudioSourcePools[owner] = ownerSources;
        }

        // 1. ownerに既存の未使用AudioSourceがあればそれを利用
        foreach (var source in ownerSources)
        {
            if (source != null && !source.isPlaying && !source.enabled) // 無効なものも対象とします。
            {
                ResetAudioSourceProperties(source); // プロパティを初期状態にリセットします。
                source.enabled = true; // 有効にします。
                // GameObjectが非アクティブな場合はアクティブにします（オーナーがアクティブでもAudioSource自身のGameObjectが非アクティブなケースを考慮）。
                if (!source.gameObject.activeInHierarchy) source.gameObject.SetActive(true);
                return source;
            }
        }

        // 2. ownerのAudioSource数が上限に達していないか確認
        if (ownerSources.Count >= _maxPooledAudioSourcesPerObject)
        {
            Debug.LogWarning($"[SoundManager] オブジェクト'{owner.name}'のAudioSource数が上限({_maxPooledAudioSourcesPerObject})に達しています。新しいAudioSourceを生成できません。");
            return null; // 上限に達しているため、新しいAudioSourceは生成しません。
        }

        // 3. グローバルプールから取得
        if (_globalAudioSourcePool.Count > 0)
        {
            AudioSource pooledSource = _globalAudioSourcePool.Pop();
            if (pooledSource != null)
            {
                // 所属GameObjectを変更
                pooledSource.transform.SetParent(owner.transform, false); // ローカル座標を維持しないようにします。
                pooledSource.gameObject.SetActive(true); // GameObjectをアクティブにします。
                ResetAudioSourceProperties(pooledSource); // プロパティを初期状態にリセットします。
                pooledSource.enabled = true; // AudioSourceコンポーネント自体も有効にします。

                ownerSources.Add(pooledSource);
                _audioSourceOwnerMap.Add(pooledSource, owner); // オーナーマップに登録します。
                return pooledSource;
            }
        }

        // 4. プールになければ新規生成
        AudioSource newSource = owner.AddComponent<AudioSource>();
        newSource.playOnAwake = false; // 自動再生しないように設定します。
        ResetAudioSourceProperties(newSource); // プロパティを初期状態にリセットします (新規生成時もリセット)。
        newSource.enabled = true; // 新規生成時は自動的に有効になります。

        ownerSources.Add(newSource);
        _audioSourceOwnerMap.Add(newSource, owner); // オーナーマップに登録します。
        return newSource;
    }

    /// <summary>
    /// 使用済みのAudioSourceをプールに戻します。
    /// このメソッドは、AudioSourceが属するGameObjectが破棄される可能性がある場合にも安全です。
    /// </summary>
    /// <param name="audioSource">プールに戻すAudioSource。</param>
    private void ReleaseAudioSource(AudioSource audioSource)
    {
        if (audioSource == null) return;

        // AudioSourceを非アクティブにし、SoundManagerの子にします。
        audioSource.Stop();
        audioSource.clip = null; // クリップをクリアします。
        audioSource.enabled = false; // AudioSourceコンポーネント自体を無効にします。
        audioSource.transform.SetParent(this.transform, false); // SoundManagerの子にします。
        audioSource.gameObject.SetActive(false); // GameObjectを非アクティブにします。

        _globalAudioSourcePool.Push(audioSource); // グローバルプールに戻します。

        // オブジェクトのプールからも参照を削除（_audioSourceOwnerMapからownerを特定）
        if (_audioSourceOwnerMap.TryGetValue(audioSource, out GameObject owner))
        {
            if (owner != null && _objectAudioSourcePools.TryGetValue(owner, out List<AudioSource> ownerSources))
            {
                ownerSources.Remove(audioSource);
                // オブジェクトのプールが空になったらエントリを削除（メモリ節約のため）
                if (ownerSources.Count == 0)
                {
                    _objectAudioSourcePools.Remove(owner);
                }
            }
            _audioSourceOwnerMap.Remove(audioSource); // マップからも削除します。
        }
    }

    /// <summary>
    /// AudioSourceにAudioMixerGroupを設定します。
    /// </summary>
    /// <param name="audioSource">設定対象のAudioSource。</param>
    /// <param name="mixerGroupName">割り当てるAudioMixerGroupの名前。</param>
    private void SetAudioMixerGroup(AudioSource audioSource, string mixerGroupName)
    {
        if (!string.IsNullOrEmpty(mixerGroupName))
        {
            if (_mixerGroupCache.TryGetValue(mixerGroupName, out AudioMixerGroup mixerGroup))
            {
                audioSource.outputAudioMixerGroup = mixerGroup;
            }
            else
            {
                Debug.LogWarning($"[SoundManager] ミキサーグループ'{mixerGroupName}'がキャッシュに見つかりません。AudioMixerGroupは設定されません。");
                audioSource.outputAudioMixerGroup = null; // 見つからない場合はデフォルトに戻します。
            }
        }
        else
        {
            audioSource.outputAudioMixerGroup = null; // ミキサーグループが指定されない場合はデフォルトに戻します。
        }
    }

    /// <summary>
    /// AudioSourceのプロパティを初期状態にリセットします。
    /// これは、AudioSourceをプールから再利用する際に、以前の設定が残らないようにするために行われます。
    /// </summary>
    /// <param name="audioSource">リセットするAudioSource。</param>
    private void ResetAudioSourceProperties(AudioSource audioSource)
    {
        audioSource.volume = 1f;
        audioSource.pitch = 1f;
        audioSource.panStereo = 0f;
        audioSource.spatialBlend = 0f; // デフォルトは2D。PlaySFXAtで3Dに上書きされる可能性があります。
        audioSource.outputAudioMixerGroup = null; // デフォルトはNone（ミキサーグループなし）
        audioSource.loop = false; // デフォルトは非ループ
        audioSource.clip = null; // クリップもクリアします。
    }

    /// <summary>
    /// LoopSoundData のBGM再生位置を監視し、シームレスなループ処理を行います。
    /// </summary>
    /// <param name="audioSource">ループ処理を行うAudioSource。</param>
    /// <param name="soundData">ループ情報を含むSoundData (LoopSoundDataである必要があります)。</param>
    private void LoopCheck(AudioSource audioSource, SoundData soundData)
    {
        if (soundData is LoopSoundData soundLoopData)
        {
            // サウンドデータの周波数とAudioSourceのクリップ周波数に基づいて、サンプリング位置を調整するヘルパー関数
            int CorrectFrequency(long n)
            {
                return (int)(n * audioSource.clip.frequency / soundLoopData.frequency);
            }
            // 現在の再生位置がループ終了地点を超えた場合、ループ開始地点に戻します。
            if (audioSource.timeSamples >= CorrectFrequency(soundLoopData.loopEnd))
            {
                audioSource.timeSamples -= CorrectFrequency(soundLoopData.loopEnd - soundLoopData.loopStart);
            }
        }
    }

    /// <summary>
    /// 呼び出し元のメソッド名を簡易的に取得します。主にデバッグ用途で、パフォーマンスに影響する可能性があります。
    /// </summary>
    /// <returns>呼び出し元のメソッド名を表す文字列。</returns>
    private string GetCallingMethodName()
    {
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
        // 0: GetCallingMethodName, 1: PlaySFXAt, 2: 呼び出し元メソッド
        if (stackTrace.FrameCount > 2)
        {
            return stackTrace.GetFrame(2).GetMethod().Name;
        }
        return "不明なメソッド";
    }
}