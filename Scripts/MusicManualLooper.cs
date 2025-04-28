using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManualLooper : MonoBehaviour
{
    public AudioSource AudioSource;

    public bool isTime = false;
    
    public int LoopEndSamples; // ループ終端時刻のサンプル数 (A)
    public int LoopLengthSamples; // ループ時間に含まれるサンプル数 (B)
    
    public float LoopEndTime; // ループ終端時刻のサンプル数 (A)
    public float LoopLengthTime; // ループ時間に含まれるサンプル数 (B)

    [SerializeField] int frequency = 48000; // 元の周波数

    private void Reset()
    {
        AudioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        int CorrectFrequency(long n) => (int)(n * AudioSource.clip.frequency / frequency );

        if (isTime)
        {
            if (AudioSource.time >= LoopEndTime)
            {
                var delta = AudioSource.time - LoopLengthTime;
                AudioSource.time = delta;
            }
        }
        else
        {
            if (AudioSource.timeSamples >= CorrectFrequency(LoopEndSamples))
            {
                AudioSource.timeSamples -= CorrectFrequency(LoopLengthSamples);
            }
        }
    }
}
