// 単一のオーディオクリップを持つサウンドデータ。
using UnityEngine;

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