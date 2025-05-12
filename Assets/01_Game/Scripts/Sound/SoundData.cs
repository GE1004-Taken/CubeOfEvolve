// サウンドデータの基底クラス（ScriptableObject）。
// 各サウンドデータはこのクラスを継承して具体的なデータを持つ。
using UnityEngine;

public abstract class SoundData : ScriptableObject
{
    // サウンドの名前（識別子）。
    public string name;

    // サウンドのオーディオクリップを取得する抽象メソッド。
    public abstract AudioClip GetAudioClip();

    // サウンドの名前を取得する抽象メソッド。
    public abstract string GetName();
}

