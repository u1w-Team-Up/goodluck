using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
public static class AudioSourceExtensions 
{
    public static void FadeOutVolume(this AudioSource bgm)
    {
        LMotion.Create(bgm.volume, 0, 0.6f).BindToVolume(bgm).AddTo(bgm);
    }
    
    public static void FadeInVolume(this AudioSource bgm)
    {
        LMotion.Create(bgm.volume, 1, 0.6f).BindToVolume(bgm).AddTo(bgm);
    }
}
