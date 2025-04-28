using System;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(AudioChorusFilter))]
public class AudioChorusRate : MonoBehaviour
{
    [SerializeField] private AudioChorusFilter audioChorusFilter;

    private void Reset()
    {
        audioChorusFilter = GetComponent<AudioChorusFilter>();
    }

    [Button]
    public void SetRate(float rate)
    {
        audioChorusFilter.rate = rate;
    }
    
    public void SetupRate(PartData head)
    {
        SetRate((float)head.GetVisualLevel() / PartData.LimitLevel);
    }
}
