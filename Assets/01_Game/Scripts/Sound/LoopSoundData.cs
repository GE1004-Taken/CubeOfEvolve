// ループ再生に対応したサウンドデータ。
using UnityEngine;

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