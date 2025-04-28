using System;
using System.Linq;
using NRandom.Linq;
using UnityEngine;
[Serializable]
public class RandomClipStore
{
    [SerializeField] private AudioClip[] clips;
    
    private int _index = -1;

    public AudioClip GetRandomClip()
    {
        // 前回の抽選と重複させない.
        _index = Enumerable.Range(0, clips.Length).Where(x=> _index != x).RandomElement();
        return clips[_index];
    }
}
