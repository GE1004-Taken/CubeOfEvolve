using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoundManagerSample.Loop
{
    public class SoundLoop : MonoBehaviour
    {
        [SerializeField] int loop_start;
        [SerializeField] int loop_end;
        [SerializeField] int frequency = 44100; // Œ³‚ÌŽü”g”
        [SerializeField] AudioSource src;

        void Update()
        {
            int CorrectFrequency(long n)
            {
                return (int)(n * src.clip.frequency / frequency);
            }
            if (src.timeSamples >= CorrectFrequency(loop_end)) { src.timeSamples -= CorrectFrequency(loop_end - loop_start); }
        }
    }
}
