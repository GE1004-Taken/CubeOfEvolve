// 作成日：   250509
// 更新日：   250512
// 作成者： 安中 健人

// 概要説明(AIにより作成)：

// 使い方説明：

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

// サウンドデータの基底クラス（ScriptableObject）。
// 各サウンドデータはこのクラスを継承して具体的なデータを持つ。
public abstract class SoundData : ScriptableObject
{
    // サウンドの名前（識別子）。
    public string name;

    // サウンドのオーディオクリップを取得する抽象メソッド。
    public abstract AudioClip GetAudioClip();

    // サウンドの名前を取得する抽象メソッド。
    public abstract string GetName();
}

// 単一のオーディオクリップを持つサウンドデータ。
[CreateAssetMenu(fileName = "AudioClipData", menuName = "Sound/AudioClipData")]
public class AudioClipData : SoundData
{
    // 再生するオーディオクリップ。
    public AudioClip audioClip;

    // オーディオクリップを返す実装。
    public override AudioClip GetAudioClip()
    {
        return audioClip;
    }

    // サウンドの名前を返す実装。
    public override string GetName()
    {
        return name;
    }
}

// ループ再生に対応したサウンドデータ。
[CreateAssetMenu(fileName = "SoundLoopData", menuName = "Sound/SoundLoopData")]
public class LoopSoundData : SoundData
{
    // 再生するオーディオクリップ。
    public AudioClip audioClip;

    // ループ開始位置（サンプル単位）。
    public int loopStart;

    // ループ終了位置（サンプル単位）。
    public int loopEnd;

    // オーディオクリップのサンプリング周波数。
    public int frequency = 44100;

    // オーディオクリップを返す実装。
    public override AudioClip GetAudioClip()
    {
        return audioClip;
    }

    // サウンドの名前を返す実装。
    public override string GetName()
    {
        return name;
    }
}

// ゲーム全体のサウンド管理を行うシングルトンクラス。
public class SoundManager : MonoBehaviour
{
    // SoundManager のシングルトンインスタンス。
    public static SoundManager Instance { get; private set; }

    // シリアライズ可能なサウンドデータのラッパークラス。
    // インスペクター上で名前と SoundData アセットを紐付けるために使用。
    [System.Serializable]
    public class SoundDataWrapper
    {
        public string name;
        public SoundData soundData;
    }

    // インスペクターから設定されるサウンドデータの配列。
    [SerializeField]
    private SoundDataWrapper[] soundDatas;

    // 再生に使用する AudioSource のリスト。
    private AudioSource[] audioSourceList = new AudioSource[20];

    // サウンド名と SoundData の対応を保持する辞書。
    private Dictionary<string, SoundData> soundDictionary = new Dictionary<string, SoundData>();

    // シリアライズ可能なシーンごとの BGM 設定クラス。
    [System.Serializable]
    public class SceneBGM
    {
        public string sceneName;
        public string bgmName;
    }

    // インスペクターから設定されるシーンごとの BGM 設定の配列。
    [SerializeField]
    private SceneBGM[] inScenePlay;

    // シーン名と BGM 名の対応を保持する辞書。
    private Dictionary<string, string> sceneBGMMapping = new Dictionary<string, string>();

    // 現在再生中の BGM の AudioSource。
    private AudioSource currentBGMSource;

    // Awake メソッド：インスタンスの初期化、シングルトンの設定、AudioSource の作成、サウンドデータの登録などを行う。
    private void Awake()
    {
        // シングルトンの設定
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject); // シーンをまたいでも破棄されないようにする
        }
        else
        {
            Destroy(gameObject); // すでにインスタンスが存在する場合は新しいインスタンスを破棄
            return;
        }

        // AudioSourceの初期化
        for (var i = 0; i < audioSourceList.Length; ++i)
        {
            audioSourceList[i] = gameObject.AddComponent<AudioSource>();
        }

        // soundDictionaryにセット
        foreach (var soundDataWrapper in soundDatas)
        {
            if (soundDataWrapper.soundData != null)
            {
                soundDictionary.Add(soundDataWrapper.name, soundDataWrapper.soundData);
            }
            else
            {
                Debug.LogError($"SoundData '{soundDataWrapper.name}' が設定されていません。");
            }
        }

        // sceneBGMMappingにセット
        foreach (var sceneBGM in inScenePlay)
        {
            sceneBGMMapping[sceneBGM.sceneName] = sceneBGM.bgmName;
        }

        // シーンロード時のBGM設定
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // OnDestroy メソッド：インスタンスが破棄される際の処理。シーンロードイベントの解除を行う。
    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    // OnSceneLoaded メソッド：シーンがロードされた際に呼び出され、シーンに対応する BGM を再生する。
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayBGMForScene(scene.name);
    }

    // Update メソッド：毎フレーム呼び出され、現在再生中の BGM のループ処理を行う。
    private void Update()
    {
        if (currentBGMSource != null && currentBGMSource.isPlaying)
        {
            LoopCheck(currentBGMSource);
        }
    }

    // LoopCheck メソッド：AudioSource の再生位置を監視し、LoopSoundData に基づいてループ処理を行う。
    private void LoopCheck(AudioSource audioSource)
    {
        if (soundDictionary.TryGetValue(audioSource.clip.name, out var soundData))
        {
            if (soundData is LoopSoundData soundLoopData)
            {
                // サンプリング周波数を考慮した正しい頻度を計算するローカル関数。
                int CorrectFrequency(long n)
                {
                    return (int)(n * audioSource.clip.frequency / soundLoopData.frequency);
                }
                // 再生位置がループ終了位置を超えた場合、ループ開始位置に戻す。
                if (audioSource.timeSamples >= CorrectFrequency(soundLoopData.loopEnd))
                {
                    audioSource.timeSamples -= CorrectFrequency(soundLoopData.loopEnd - soundLoopData.loopStart);
                }
            }
        }
    }

    // Play メソッド：指定された名前のサウンドを再生する。ミキサーグループを指定することも可能。
    public void Play(string name, string mixerGroupName = null)
    {
        if (soundDictionary.TryGetValue(name, out var soundData))
        {
            AudioClip clipToPlay = soundData.GetAudioClip();
            if (clipToPlay != null)
            {
                AudioSource audioSource = GetUnusedAudioSource();
                if (audioSource != null)
                {
                    audioSource.clip = clipToPlay;
                    if (!string.IsNullOrEmpty(mixerGroupName))
                    {
                        AudioMixerGroup mixerGroup = Resources.FindObjectsOfTypeAll<AudioMixerGroup>().FirstOrDefault(group => group.name == mixerGroupName);
                        if (mixerGroup != null)
                        {
                            audioSource.outputAudioMixerGroup = mixerGroup;
                        }
                        else
                        {
                            Debug.LogWarning($"ミキサーグループ'{mixerGroupName}'が見つかりません。");
                        }
                    }
                    audioSource.Play();
                }
            }
            else
            {
                Debug.LogWarning($"オーディオクリップが見つかりません: {name}");
            }
        }
        else
        {
            Debug.LogWarning($"その別名は登録されていません: {name}");
        }
    }

    // PlayBGMForScene メソッド：指定されたシーン名に対応する BGM を再生する。
    public void PlayBGMForScene(string sceneName)
    {
        if (sceneBGMMapping.TryGetValue(sceneName, out var bgmName))
        {
            PlayBGM(bgmName, "BGM");
        }
        else
        {
            Debug.LogWarning($"シーン名に対応するBGMが見つかりません: {sceneName}");
        }
    }

    // PlayBGM メソッド：指定された BGM 名の BGM を再生する。ミキサーグループを指定することも可能。
    public void PlayBGM(string bgmName, string mixerGroupName = null)
    {
        // 現在再生中の BGM があれば停止する。
        if (currentBGMSource != null && currentBGMSource.isPlaying)
        {
            currentBGMSource.Stop();
        }

        if (soundDictionary.TryGetValue(bgmName, out var soundData))
        {
            currentBGMSource = GetUnusedAudioSource();
            if (currentBGMSource != null)
            {
                if (!string.IsNullOrEmpty(mixerGroupName))
                {
                    AudioMixerGroup mixerGroup = Resources.FindObjectsOfTypeAll<AudioMixerGroup>().FirstOrDefault(group => group.name == mixerGroupName);
                    if (mixerGroup != null)
                    {
                        currentBGMSource.outputAudioMixerGroup = mixerGroup;
                    }
                    else
                    {
                        Debug.LogWarning($"ミキサーグループ'{mixerGroupName}'が見つかりません。");
                    }
                }
                currentBGMSource.clip = soundData.GetAudioClip();
                currentBGMSource.loop = true;
                currentBGMSource.Play();
            }
        }
        else
        {
            Debug.LogWarning($"BGMが見つかりません: {bgmName}");
        }
    }

    // GetUnusedAudioSource メソッド：現在再生中でない AudioSource を取得する。
    private AudioSource GetUnusedAudioSource()
    {
        for (var i = 0; i < audioSourceList.Length; ++i)
        {
            if (!audioSourceList[i].isPlaying) return audioSourceList[i];
        }
        return null; // 全ての AudioSource が使用中の場合は null を返す。
    }
}