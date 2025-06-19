using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Assets.AT
{
    public class AudioCtrl : MonoBehaviour
    {
        [Serializable]
        public struct AudioSliderPair
        {
            public string GroupName;
            public Slider Slider;
        }

        // ---------------------------- SerializeField
        [SerializeField] private AudioMixer _audioMixer;

        [SerializeField] private List<AudioSliderPair> _audioSliderList = new List<AudioSliderPair>();

        // AudioMixerの音量の最小値（dB）と最大値（dB）
        [SerializeField] private float _minVolumeDB = -79f;
        [SerializeField] private float _maxVolumeDB = 1f;

        // ---------------------------- Field


        // ---------------------------- UnityMessage

        private void Start()
        {
            SetSliderCtrl();
        }

        // ---------------------------- PublicMethod

        public void SetSliderCtrl()
        {
            if (_audioMixer == null)
                return;

            foreach (var group in _audioSliderList)
            {
                string groupName = group.GroupName;
                Slider slider = group.Slider;

                // AudioMixerから現在の音量を取得（dB）
                if (slider != null)
                {
                    // AudioMixerから現在の音量を取得（dB）
                    if (_audioMixer.GetFloat(groupName, out float volumeDB))
                    {
                        // dB値を0～1のSlider値に変換
                        float sliderValue = Mathf.InverseLerp(_minVolumeDB, _maxVolumeDB, volumeDB);
                        slider.value = sliderValue;

                        // 既存のリスナーを削除して新しいリスナーを追加
                        //slider.onValueChanged.RemoveAllListeners();
                        slider.onValueChanged.AddListener(value => OnValueChangedGroup(groupName, value));
                    }
                    else
                    {
                        Debug.LogWarning($"AudioMixerのExposed Parameter '{groupName}' が見つかりません。");
                    }
                }
            }
        }

        // ---------------------------- PrivateMethod

        private void OnValueChangedGroup(string GroupName, float Value)
        {
            // 0～1のSlider値をdBに変換
            float dBValue = Mathf.Lerp(_minVolumeDB, _maxVolumeDB, Value);
            _audioMixer.SetFloat(GroupName, dBValue); // SEグループの音量を設定
        }
    }
}